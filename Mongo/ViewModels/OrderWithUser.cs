using System;
using System.Collections.Generic;
using Mongo.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.ViewModels
{
    public class OrderWithUser
    {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("order_id")]
        public int OrderId { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; }

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; }

        [BsonElement("total")]
        public double Total { get; set; }

        [BsonElement("order_date")]
        public DateTime OrderDate { get; set; }

        [BsonElement("user_info")]
        public List<User> Users { get; set; } // 連接結果中的使用者資訊
    }

}
