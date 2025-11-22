using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class Role
    {
        [BsonId]
        public string Id { get; set; }    

        public string Name { get; set; } 

        public string Description { get; set; }
    }
}
