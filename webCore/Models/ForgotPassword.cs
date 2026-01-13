using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class ForgotPassword
    {
        [BsonId] 
        public ObjectId Id { get; set; }
        public string Email { get; set; }  
        public string OTP { get; set; }   
        public DateTime? OTPExpiry { get; set; } 
    }
}
