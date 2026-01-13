using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using webCore.Models;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ForgotPasswordService
    {
        private readonly IMongoCollection<ForgotPassword> _forgotPasswords;
        private readonly IConfiguration _configuration;

        public ForgotPasswordService(MongoDBService mongoDBService, IConfiguration configuration)
        {
            _forgotPasswords = mongoDBService.ForgotPasswords;
            _configuration = configuration;
        }

        public async Task<string> GenerateAndSendOTP(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(5);
            var hashedOtp = BCrypt.Net.BCrypt.HashPassword(otp);

            var updateDefinition = Builders<ForgotPassword>.Update
                .Set(f => f.OTP, hashedOtp)
                .Set(f => f.OTPExpiry, otpExpiry);

            await _forgotPasswords.UpdateOneAsync(
                f => f.Email == email,
                updateDefinition,
                new UpdateOptions { IsUpsert = true }
            );

            try
            {
                SendEmail(email, "Mã OTP của bạn", $"Mã OTP của bạn là: {otp}");
                return "OTP đã được gửi đến email của bạn.";
            }
            catch (Exception)
            {
                throw new Exception("Không thể gửi email. Vui lòng thử lại.");
            }
        }
        public async Task<bool> VerifyOTP(string email, string otp)
        {
            var forgotPassword = await _forgotPasswords
                .Find(f => f.Email == email)
                .FirstOrDefaultAsync();

            if (forgotPassword == null || forgotPassword.OTPExpiry < DateTime.UtcNow)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(otp, forgotPassword.OTP))
                return false;

            await _forgotPasswords.DeleteOneAsync(f => f.Email == email);
            return true;
        }

        private void SendEmail(string to, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SMTPServer"];
            var port = int.Parse(emailSettings["Port"]);
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("TYB Book", senderEmail));
            emailMessage.To.Add(new MailboxAddress(to, to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = body };

            using (var client = new SmtpClient())
            {
                client.Connect(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(senderEmail, senderPassword);
                client.Send(emailMessage);
                client.Disconnect(true);
            }
        }
    }
}
