using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Models
{
    public class Order
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("order_id")]
        public int OrderId { get; set; } // 業務訂單 ID

        [BsonElement("user_id")]
        public int UserId { get; set; } // 關聯的使用者業務 ID

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; }

        [BsonElement("total")]
        public double Total { get; set; }

        [BsonElement("order_date")]
        public DateTime OrderDate { get; set; }
    }

}
