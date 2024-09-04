using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Models
{
    public class Product
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; } // MongoDB 自動生成的 ObjectId
        [BsonElement("product_id")]
        public int ProductId { get; set; } // 商品 ID
        [BsonElement("productName")]
        public string ProductName { get; set; } // 商品名稱
        [BsonElement("category")]
        public string Category { get; set; } // 商品類別（如：Electronics, Clothing 等）
        [BsonElement("price")]
        public double Price { get; set; } // 商品價格
    }
}
