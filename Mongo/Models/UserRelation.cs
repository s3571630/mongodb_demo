using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Models
{
    public class UserRelation
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; } // 關聯的使用者業務 ID

        [BsonElement("related_user_id")]
        public int RelatedUserId { get; set; } // 關聯使用者 ID

        [BsonElement("relation_type")]
        public string RelationType { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
