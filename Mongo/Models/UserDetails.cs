using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Models
{
    public class UserDetails
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; } // 關聯的使用者業務 ID

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("preferences")]
        public UserPreferences Preferences { get; set; }
    }
}
