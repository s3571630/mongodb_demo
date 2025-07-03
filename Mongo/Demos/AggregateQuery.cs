using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Mongo.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo.Demos
{
    /// <summary>
    /// Aggregate BSON查詢
    /// </summary>
    public class AggregateQuery
    {
        private IMongoDatabase? TestDB;
        // 使用 JsonWriterSettings 格式化輸出
        private JsonWriterSettings jsonSettings = new JsonWriterSettings { Indent = true };
        public AggregateQuery(IMongoDatabase? db)
        {
            TestDB = db;
        }

        /// <summary>
        /// 篩選資料（$match）
        /// </summary>
        /// <remarks>
        /// 這個方法用來篩選訂單，查詢總金額大於 500 的訂單。
        /// </remarks>
        public async Task FilterOrdersAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 $match 進行篩選操作
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("total", new BsonDocument("$gt", 500)))
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("篩選總金額大於 500 的訂單:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }


        /// <summary>
        /// 分組和計算（$group）
        /// </summary>
        /// <remarks>
        /// 這個方法用來按使用者 ID 分組，計算每個使用者的訂單總數和總金額。
        /// </remarks>
        public async Task GroupOrdersByUserAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 $group 進行分組和計算操作
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$user_id" },
                    { "total_orders", new BsonDocument("$sum", 1) },
                    { "total_amount", new BsonDocument("$sum", "$total") }
                })
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("按使用者 ID 分組並計算總數和總金額:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }


        /// <summary>
        /// 排序（$sort）
        /// </summary>
        /// <remarks>
        /// 這個方法用來按使用者 ID 分組，計算每個使用者的訂單總數和總金額。
        /// </remarks>
        public async Task SortOrdersByTotalAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 $sort 進行排序操作
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$sort", new BsonDocument("total", -1))
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("按總金額降序排序訂單:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }

        /// <summary>
        /// 投影（$project）
        /// </summary>
        /// <remarks>
        /// 這個方法用來只顯示 OrderId 和 Total 欄位。
        /// </remarks>
        public async Task ProjectOrderFieldsAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 $project 選擇特定欄位
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$project", new BsonDocument
                {
                    { "order_id", 1 },
                    { "total", 1 },
                    { "_id", 0 }
                })
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("只顯示 OrderId 和 Total 欄位:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }

        /// <summary>
        /// 連接集合（$lookup）。
        /// </summary>
        /// <remarks>
        /// 這個方法用來查詢訂單並連接對應的使用者資訊。
        /// </remarks>
        public async Task LookupOrdersWithUsersAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 $lookup 連接集合
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" },
                    { "localField", "user_id" },
                    { "foreignField", "user_id" },
                    { "as", "user_details" }
                })
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("查詢訂單並連接使用者資訊:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }

        /// <summary>
        /// 動態計算和新增欄位（$addFields）
        /// </summary>
        /// <remarks>
        /// 這個方法用來按使用者分組並計算每個使用者的訂單總金額及其稅金。
        /// </remarks>
        public async Task AggregateAndCalculateWithBsonDocumentAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 BsonDocument 定義聚合管道
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$user_id" },
                    { "total_amount", new BsonDocument("$sum", "$total") }
                }),
                new BsonDocument("$addFields", new BsonDocument("Tax", new BsonDocument("$multiply", new BsonArray { "$total_amount", 0.05 })))
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("動態計算聚合結果:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }

        /// <summary>
        /// 查詢訂單及其相關使用者的詳細資訊
        /// </summary>
        /// <remarks>
        /// 這個方法使用 $lookup 來查詢訂單，並獲取每個訂單的相關使用者資訊。
        /// </remarks>
        public async Task LookupOrdersWithUserDetailsAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 $lookup 連接 Orders 和 Users 集合
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" }, // 要連接的集合
                    { "localField", "user_id" }, // Orders 集合中的欄位
                    { "foreignField", "user_id" }, // Users 集合中的欄位
                    { "as", "user_details" } // 結果中的欄位名稱
                }),
                new BsonDocument("$unwind", "$user_details") // 將 user_details 陣列展開
                    };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("查詢訂單及其相關使用者的詳細資訊:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }

        /// <summary>
        /// 查詢使用者及其所有訂單的總金額
        /// </summary>
        /// <remarks>
        /// 這個方法使用 $lookup 查詢每個使用者的所有訂單，並計算每個使用者的訂單總金額。
        /// </remarks>
        public async Task LookupUsersWithTotalOrderAmountAsync()
        {
            var usersCollection = TestDB!.GetCollection<BsonDocument>("Users");

            // 使用 $lookup 連接 Users 和 Orders 集合，並計算總金額
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Orders" }, // 要連接的集合
                    { "localField", "user_id" }, // Users 集合中的欄位
                    { "foreignField", "user_id" }, // Orders 集合中的欄位
                    { "as", "Orders" } // 結果中的欄位名稱
                }),
                new BsonDocument("$unwind", "$Orders"), // 將 Orders 陣列展開
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$user_id" },
                    { "Username", new BsonDocument("$first", "$username") }, // 獲取使用者名稱
                    { "TotalOrderAmount", new BsonDocument("$sum", "$Orders.total") } // 計算總金額
                })
            };

            var results = await usersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("查詢使用者及其所有訂單的總金額:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }

        /// <summary>
        /// 查詢訂單及其相關的使用者和產品資訊
        /// </summary>
        /// <remarks>
        /// 這個方法使用兩個 $lookup 操作來查詢每個訂單的使用者和產品資訊。
        /// </remarks>
        public async Task LookupOrdersWithUserAndProductDetailsAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用兩個 $lookup 操作連接 Orders 和 Users, Products 集合
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" }, // 連接使用者集合
                    { "localField", "user_id" }, // 調整為你的欄位命名風格
                    { "foreignField", "user_id" }, // 調整為你的欄位命名風格
                    { "as", "user_details" } // 保留連接結果的別名
                }),
                // 保持 user_details 為嵌套陣列，不使用 $unwind
        
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Products" }, // 連接產品集合
                    { "localField", "items.product_id" }, // 調整為你的欄位命名風格
                    { "foreignField", "product_id" }, // 調整為你的欄位命名風格
                    { "as", "product_details" } // 保留連接結果的別名
                }),
                // 保持 product_details 為嵌套陣列，不使用 $unwind

                // 使用 $project 操作來整理輸出格式
                new BsonDocument("$project", new BsonDocument
                {
                    { "order_id", 1 }, // 調整為你的欄位命名風格
                    { "user_id", 1 }, // 調整為你的欄位命名風格
                    { "items", 1 },
                    { "total", 1 },
                    { "order_date", 1 }, // 調整為你的欄位命名風格
                    { "user_details", 1 }, // 保留使用者資訊
                    { "product_details", 1 } // 保留產品資訊
                })
            };

            var results = await ordersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("查詢訂單及其相關的使用者和產品資訊:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }


        /// <summary>
        /// 查詢使用者及其最新的訂單
        /// </summary>
        /// <remarks>
        /// 這個方法使用 $lookup 來查詢每個使用者的所有訂單，然後使用 $sort 和 $limit 來查詢最新的訂單。
        /// </remarks>
        public async Task LookupUsersWithLatestOrderAsync()
        {
            var usersCollection = TestDB!.GetCollection<BsonDocument>("Users");

            // 使用 $lookup 連接 Users 和 Orders 集合，並查詢最新訂單
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Orders" }, // 連接 Orders 集合
                    { "localField", "user_id" },
                    { "foreignField", "user_id" },
                    { "as", "Orders" }
                }),
                new BsonDocument("$unwind", "$Orders"), // 展開 Orders 陣列
                new BsonDocument("$sort", new BsonDocument("Orders.order_date", -1)), // 按訂單日期降序排序
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$user_id" },
                    { "Username", new BsonDocument("$first", "$username") },
                    { "LatestOrder", new BsonDocument("$first", "$Orders") } // 獲取最新的訂單
                })
            };

            var results = await usersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("查詢使用者及其最新的訂單:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }


        /// <summary>
        /// 查詢所有產品並找出購買過該產品的使用者
        /// </summary>
        /// <remarks>
        /// 這個方法使用 $lookup 查詢每個產品及其購買過該產品的使用者。
        /// </remarks>
        public async Task LookupProductsWithBuyersAsync()
        {
            var productsCollection = TestDB!.GetCollection<BsonDocument>("Products");

            var pipeline = new BsonDocument[]
            {
                // 連接 Products 和 Orders 集合
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Orders" },
                    { "localField", "product_id" },
                    { "foreignField", "items.product_id" },
                    { "as", "order_details" }
                }),
        
                // 使用 $lookup 將 Orders 集合的 UserId 連接到 Users 集合
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" },
                    { "localField", "order_details.user_id" },
                    { "foreignField", "user_id" },
                    { "as", "buyer_details" }
                }),
        
                // 使用 $group 合併 buyer_details 的購買者資訊
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$_id" },
                    { "product_id", new BsonDocument("$first", "$product_id") },
                    { "productName", new BsonDocument("$first", "$productName") },
                    { "category", new BsonDocument("$first", "$category") },
                    { "price", new BsonDocument("$first", "$price") },
                    { "order_details", new BsonDocument("$first", "$order_details") },
                    { "buyer_details", new BsonDocument("$first", "$buyer_details") }
                }),

                // 新增欄位：購買者數量和訂單數量，並處理可能的空值
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "order_count", new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$order_details", new BsonArray() })) },
                    { "buyer_count", new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$buyer_details", new BsonArray() })) }
                }),

                // 投影欄位，只保留所需資訊
                new BsonDocument("$project", new BsonDocument
                {
                    { "product_id", 1 },
                    { "productName", 1 },
                    { "category", 1 },
                    { "price", 1 },
                    { "order_count", 1 },
                    { "buyer_count", 1 },
                    { "buyer_details.username", 1 },
                    { "buyer_details.email", 1 }
                })
            };

            var results = await productsCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            Console.WriteLine("查詢所有產品並找出購買過該產品的使用者:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToJson(jsonSettings));
            }
        }


    }
}
