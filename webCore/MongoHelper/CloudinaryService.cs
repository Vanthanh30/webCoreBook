
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace webCore.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Timeout = 600000;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            using (var stream = imageFile.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageFile.FileName, stream)
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult.SecureUrl.ToString();
            }
        }
        public async Task<string> UploadMediaAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File is null or empty");
                return null;
            }

            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };
                var isVideo = videoExtensions.Contains(extension);

                _logger.LogInformation($"Uploading {(isVideo ? "video" : "image")}: {file.FileName}, Size: {file.Length} bytes");

                using (var stream = file.OpenReadStream())
                {
                    if (isVideo)
                    {
                        var uploadParams = new VideoUploadParams()
                        {
                            File = new FileDescription(file.FileName, stream),
                            Folder = "reviews",
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.Error != null)
                        {
                            _logger.LogError($"Cloudinary video upload error: {uploadResult.Error.Message}");
                            return null;
                        }

                        _logger.LogInformation($"Video uploaded successfully: {uploadResult.SecureUrl}");
                        return uploadResult.SecureUrl?.ToString();
                    }
                    else
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(file.FileName, stream),
                            Folder = "reviews",
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.Error != null)
                        {
                            _logger.LogError($"Cloudinary image upload error: {uploadResult.Error.Message}");
                            return null;
                        }

                        _logger.LogInformation($"Image uploaded successfully: {uploadResult.SecureUrl}");
                        return uploadResult.SecureUrl?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during upload: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}