using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Models
{

    public class User
    {
        [BsonId] // 指定這是 MongoDB 中的主鍵
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; }  // 業務 ID

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("details_id")]
        public ObjectId DetailsId { get; set; }

        [BsonElement("order_ids")]
        public List<ObjectId> OrderIds { get; set; }

        [BsonElement("relation_ids")]
        public List<ObjectId> RelationIds { get; set; }
    }
}
