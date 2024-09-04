using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using Mongo.Services;
using Mongo.Models;
using Mongo.Demos;
using MongoDB.Bson.IO;

namespace Mongo
{
    public class Program
    {
        private static Config config = new Config();
        private static MongoClient? mongoClient;
        private static IMongoDatabase? TestDB;
        private static MongoSchemaManager? SchemaManager;
        private static JsonWriterSettings jsonSettings = new JsonWriterSettings { Indent = true };

        static async Task Main(string[] args)
        {

            mongoClient = new MongoClient(config.appSettings.Mongo.ConnectionString);

            // 取得資料庫 (若不存在則會自動建立)
            TestDB = mongoClient.GetDatabase("TestDB");
            SchemaManager = new MongoSchemaManager(TestDB);
            OrmQuery ormQuery = new OrmQuery(TestDB);
            AggregateQuery aggregateQuery = new AggregateQuery(TestDB);

            #region Testing
            // 創建測試collection
            //await CreateDefaultCollection();
            // 更新測試collection
            //await UpdateDefaultCollectionSchema();
            #endregion

            #region Initial
            Console.WriteLine("\n************************   初始化  ****************************\n");
            // 創建user相關表
            await Task.WhenAll(
                DropCollectionAsync("Users"),
                DropCollectionAsync("Orders"),
                DropCollectionAsync("UserDetails"),
                DropCollectionAsync("UserRelations"),
                DropCollectionAsync("Products")
            );

            await Task.WhenAll(
                CreateUsers(),
                CreateOrders(),
                CreateUserDetails(),
                CreateUserRelations(),
                CreateProducts()
            );

            await InsertUserDataAsync();
            await InsertProductDataAsync();

            Console.WriteLine("\n***************************************************************\n");

            #endregion

            #region 物件對應查詢範例
            Console.WriteLine("\n************************   物件查詢範例  **************************\n");

            Console.WriteLine("\n物件-篩選資料\n");
            await ormQuery.FilterOrdersAsync();
            Console.WriteLine("\n物件-分組和計算\n");
            await ormQuery.GroupOrdersByUserAsync();
            Console.WriteLine("\n物件-排序\n");
            await ormQuery.SortOrdersByTotalAsync();
            Console.WriteLine("\n物件-投影\n");
            await ormQuery.ProjectOrderFieldsAsync();
            Console.WriteLine("\n物件-連接集合\n");
            await ormQuery.LookupOrdersWithUsersAsync();
            Console.WriteLine("\n物件-動態計算和新增欄位\n");
            await ormQuery.AggregateAndCalculateWithObjectAsync();

            Console.WriteLine("\n************************   物件綜合查詢範例  **************************\n");

            Console.WriteLine("\n物件-查詢訂單及其相關使用者的詳細資訊\n");
            await ormQuery.LookupOrdersWithUserDetailsAsync();
            Console.WriteLine("\n物件-查詢使用者及其所有訂單的總金額\n");
            await ormQuery.LookupUsersWithTotalOrderAmountAsync();
            Console.WriteLine("\n物件-查詢訂單及其相關的使用者和產品資訊\n");
            await ormQuery.LookupOrdersWithUserAndProductDetailsAsync();
            Console.WriteLine("\n物件-查詢使用者及其最新的訂單\n");
            await ormQuery.LookupUsersWithLatestOrderAsync();
            Console.WriteLine("\n物件-查詢所有產品並找出購買過該產品的使用者\n");
            await ormQuery.LookupProductsWithBuyersAsync();

            Console.WriteLine("\n************************************************************************\n");

            #endregion

            #region Aggregate

            Console.WriteLine("\n************************   Aggregate查詢範例  **************************\n");

            Console.WriteLine("\nAggregate-篩選資料\n");
            await aggregateQuery.FilterOrdersAsync();
            Console.WriteLine("\nAggregate-分組和計算\n");
            await aggregateQuery.GroupOrdersByUserAsync();
            Console.WriteLine("\nAggregate-排序\n");
            await aggregateQuery.SortOrdersByTotalAsync();
            Console.WriteLine("\nAggregate-投影\n");
            await aggregateQuery.ProjectOrderFieldsAsync();
            Console.WriteLine("\nAggregate-連接集合\n");
            await aggregateQuery.LookupOrdersWithUsersAsync();
            Console.WriteLine("\nAggregate-動態計算和新增欄位\n");
            await aggregateQuery.AggregateAndCalculateWithBsonDocumentAsync();

            Console.WriteLine("\n************************   Aggregate綜合查詢範例  **************************\n");

            Console.WriteLine("\nAggregate-查詢訂單及其相關使用者的詳細資訊\n");
            await aggregateQuery.LookupOrdersWithUserDetailsAsync();
            Console.WriteLine("\nAggregate-查詢使用者及其所有訂單的總金額\n");
            await aggregateQuery.LookupUsersWithTotalOrderAmountAsync();
            Console.WriteLine("\nAggregate-查詢訂單及其相關的使用者和產品資訊\n");
            await aggregateQuery.LookupOrdersWithUserAndProductDetailsAsync();
            Console.WriteLine("\nAggregate-查詢使用者及其最新的訂單\n");
            await aggregateQuery.LookupUsersWithLatestOrderAsync();
            Console.WriteLine("\nAggregate-查詢所有產品並找出購買過該產品的使用者\n");
            await aggregateQuery.LookupProductsWithBuyersAsync();

            Console.WriteLine("\n************************************************************************\n");

            #endregion

            #region 其他
            Console.WriteLine("\n************************   其他範例  ****************************\n");

            Console.WriteLine("\nBson格式查詢\n");
            await QueryOrdersAsBsonDocumentAsync();

            Console.WriteLine("\n使用Dynamic查詢\n");
            await QueryUsersAsDynamicAsync();

            Console.WriteLine("\n******************************************************************\n");

            #endregion
        }

        #region Testing
        static async Task CreateDefaultCollection()
        {
            // 定義外部傳入的 Schema 驗證規則
            var externalSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "name", "age" } },
                        { "properties", new BsonDocument
                            {
                                { "name", new BsonDocument { { "bsonType", "string" } } },
                                { "age", new BsonDocument { { "bsonType", "int" } } },
                                { "city", new BsonDocument { { "bsonType", "string" } } }
                            }
                        }
                    }
                }
            };

            // 創建集合並設定 Schema
            await SchemaManager!.ApplySchemaValidationAsync("T_Person", externalSchema);
        }
        static async Task UpdateDefaultCollectionSchema()
        {
            // 更新已存在集合的 Schema
            var updatedSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "name", "age", "city" } },
                        { "properties", new BsonDocument
                            {
                                { "name", new BsonDocument { { "bsonType", "string" } } },
                                { "age", new BsonDocument { { "bsonType", "int" } } },
                                { "city", new BsonDocument { { "bsonType", "string" } } },
                                { "c_name", new BsonDocument { { "bsonType", "string" } } },
                            }
                        }
                    }
                }
            };

            // 更新 Schema 驗證規則
            await SchemaManager!.UpdateSchemaValidationAsync("T_Person", updatedSchema);

        }
        #endregion


        #region 準備表格與資料
        static async Task CreateUsers()
        {
            // Users
            var usersSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "username", "email", "user_id", "details_id" } },
                        { "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "user_id", new BsonDocument { { "bsonType", "int" }, { "description", "使用者業務 ID 必須為整數" } } },
                                { "username", new BsonDocument { { "bsonType", "string" }, { "description", "使用者名稱必須為字串" } } },
                                { "email", new BsonDocument
                                    {
                                        { "bsonType", "string" },
                                        { "pattern", "^\\S+@\\S+\\.\\S+$" },
                                        { "description", "電子郵件必須為有效格式" }
                                    }
                                },
                                { "details_id", new BsonDocument { { "bsonType", "objectId" }, { "description", "詳細資料 ID 必須為 ObjectId" } } },
                                { "order_ids", new BsonDocument
                                    {
                                        { "bsonType", "array" },
                                        { "items", new BsonDocument { { "bsonType", "objectId" } } },
                                        { "description", "訂單 ID 必須為 ObjectId 陣列" }
                                    }
                                },
                                { "relation_ids", new BsonDocument
                                    {
                                        { "bsonType", "array" },
                                        { "items", new BsonDocument { { "bsonType", "objectId" } } },
                                        { "description", "關係 ID 必須為 ObjectId 陣列" }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            await SchemaManager!.ApplySchemaValidationAsync("Users", usersSchema);

        }

        static async Task CreateOrders()
        {
            var ordersSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "order_id", "user_id", "items", "total", "order_date" } },
                        { "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "order_id", new BsonDocument { { "bsonType", "int" }, { "description", "訂單業務 ID 必須為整數" } } },
                                { "user_id", new BsonDocument { { "bsonType", "int" }, { "description", "使用者 ID 必須為整數" } } },
                                { "items", new BsonDocument
                                    {
                                        { "bsonType", "array" },
                                        { "items", new BsonDocument
                                            {
                                                { "bsonType", "object" },
                                                { "required", new BsonArray { "product_id", "product_name", "quantity", "price" } },
                                                { "properties", new BsonDocument
                                                    {
                                                        { "product_id", new BsonDocument { { "bsonType", "int" }, { "description", "商品 ID 必須為整數" } } },
                                                        { "product_name", new BsonDocument { { "bsonType", "string" }, { "description", "商品名稱必須為字串" } } },
                                                        { "quantity", new BsonDocument { { "bsonType", "int" }, { "description", "數量必須為整數" } } },
                                                        { "price", new BsonDocument { { "bsonType", "double" }, { "description", "價格必須為數字" } } }
                                                    }
                                                }
                                            }
                                        },
                                        { "description", "商品項目必須為物件陣列" }
                                    }
                                },
                                { "total", new BsonDocument { { "bsonType", "double" }, { "description", "訂單總額必須為數字" } } },
                                { "order_date", new BsonDocument { { "bsonType", "date" }, { "description", "訂單日期必須為日期" } } }
                            }
                        }
                    }
                }
            };
            await SchemaManager!.ApplySchemaValidationAsync("Orders", ordersSchema);
        }

        static async Task CreateUserDetails()
        {
            var userDetailsSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "user_id", "address", "phone" } },
                        { "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "user_id", new BsonDocument { { "bsonType", "int" }, { "description", "使用者 ID 必須為整數" } } },
                                { "address", new BsonDocument { { "bsonType", "string" }, { "description", "地址必須為字串" } } },
                                { "phone", new BsonDocument { { "bsonType", "string" }, { "description", "電話必須為字串" } } },
                                { "preferences", new BsonDocument
                                    {
                                        { "bsonType", "object" },
                                        { "properties", new BsonDocument
                                            {
                                                { "newsletter", new BsonDocument { { "bsonType", "bool" }, { "description", "訂閱設定必須為布林值" } } },
                                                { "notifications", new BsonDocument { { "bsonType", "string" }, { "description", "通知設定必須為字串" } } }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            await SchemaManager!.ApplySchemaValidationAsync("UserDetails", userDetailsSchema);

        }


        static async Task CreateUserRelations()
        {
            var userRelationsSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "user_id", "related_user_id", "relation_type" } },
                        { "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "user_id", new BsonDocument { { "bsonType", "int" }, { "description", "使用者 ID 必須為整數" } } },
                                { "related_user_id", new BsonDocument { { "bsonType", "int" }, { "description", "關聯使用者 ID 必須為整數" } } },
                                { "relation_type", new BsonDocument { { "bsonType", "string" }, { "description", "關係類型必須為字串" } } },
                                { "created_at", new BsonDocument { { "bsonType", "date" }, { "description", "建立時間必須為日期" } } }
                            }
                        }
                    }
                }
            };
            await SchemaManager!.ApplySchemaValidationAsync("UserRelations", userRelationsSchema);

        }

        static async Task CreateProducts()
        {
            var productsSchema = new BsonDocument
            {
                { "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "product_id", "productName", "category", "price" } },
                        { "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "product_id", new BsonDocument { { "bsonType", "int" }, { "description", "商品 ID 必須為整數" } } },
                                { "productName", new BsonDocument { { "bsonType", "string" }, { "description", "商品名稱必須為字串" } } },
                                { "category", new BsonDocument { { "bsonType", "string" }, { "description", "商品類別必須為字串" } } },
                                { "price", new BsonDocument { { "bsonType", "double" }, { "description", "商品價格必須為數字" } } }
                            }
                        }
                    }
                }
            };
            await SchemaManager!.ApplySchemaValidationAsync("Products", productsSchema);
        }

        // 插入測試資料
        static async Task InsertUserDataAsync()
        {
            //using (var session = await TestDB!.Client.StartSessionAsync())
            //{
            //    session.StartTransaction();

            try
            {
                // 插入 UserDetails
                var userDetails = new List<UserDetails>()
                    {
                        // UserId = 101
                        new UserDetails
                        {
                            Id = ObjectId.GenerateNewId(),
                            UserId = 101,
                            Address = "桃園市桃園區大有路16-1號",
                            Phone = "0912345678",
                            Preferences = new UserPreferences { Newsletter = true, Notifications = "email" }
                        },
                        // UserId = 102
                        new UserDetails
                        {
                            Id = ObjectId.GenerateNewId(),
                            UserId = 102,
                            Address = "台北市中正區廈門街10號",
                            Phone = "0912345677",
                            Preferences = new UserPreferences { Newsletter = true, Notifications = "email" }
                        }
                    };

                await TestDB!.GetCollection<UserDetails>("UserDetails").InsertManyAsync(userDetails);

                // 插入 Orders
                var orders = new List<Order>
                    {
                        // UserId = 101
                        new Order
                        {
                            Id = ObjectId.GenerateNewId(),
                            OrderId = 1,
                            UserId = 101,
                            Items = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 201, ProductName = "Laptop", Quantity = 1, Price = 1200 },
                                new OrderItem { ProductId = 202, ProductName = "Mouse", Quantity = 1, Price = 25 }
                            },
                            Total = 1225,
                            OrderDate = DateTime.UtcNow
                        },
                        new Order
                        {
                            Id = ObjectId.GenerateNewId(),
                            OrderId = 2,
                            UserId = 101,
                            Items = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 203, ProductName = "Keyboard", Quantity = 1, Price = 100 }
                            },
                            Total = 100,
                            OrderDate = DateTime.UtcNow
                        },
                        // UserId = 102
                        new Order
                        {
                            Id = ObjectId.GenerateNewId(),
                            OrderId = 3,
                            UserId = 102,
                            Items = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 204, ProductName = "Screen", Quantity = 1, Price = 25000 }
                            },
                            Total = 25000,
                            OrderDate = DateTime.UtcNow
                        },
                        new Order
                        {
                            Id = ObjectId.GenerateNewId(),
                            OrderId = 4,
                            UserId = 102,
                            Items = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 203, ProductName = "Keyboard", Quantity = 1, Price = 100 }
                            },
                            Total = 100,
                            OrderDate = DateTime.UtcNow
                        }

                    };
                await TestDB!.GetCollection<Order>("Orders").InsertManyAsync(orders);

                // 插入 UserRelations
                var userRelation = new List<UserRelation>()
                    {
                        // UserId = 101
                        new UserRelation(){
                            Id = ObjectId.GenerateNewId(),
                            UserId = 101,
                            RelatedUserId = 102,
                            RelationType = "friend",
                            CreatedAt = DateTime.UtcNow
                        },
                        // UserId = 102
                        new UserRelation(){
                            Id = ObjectId.GenerateNewId(),
                            UserId = 102,
                            RelatedUserId = 101,
                            RelationType = "friend",
                            CreatedAt = DateTime.UtcNow
                        },
                    };
                await TestDB!.GetCollection<UserRelation>("UserRelations").InsertManyAsync(userRelation);

                // 插入 Users
                var user = new List<User>()
                    {
                        // UserId = 101
                        new User
                        {
                            Id = ObjectId.GenerateNewId(),
                            UserId = 101,
                            Username = "john_doe",
                            Email = "john@example.com",
                            DetailsId = userDetails[0].Id,
                            OrderIds = new List<ObjectId> { orders[0].Id, orders[1].Id },
                            RelationIds = new List<ObjectId> { userRelation[0].Id }
                        },
                        // UserId = 102
                        new User
                        {
                            Id = ObjectId.GenerateNewId(),
                            UserId = 102,
                            Username = "naruto",
                            Email = "naruto@example.com",
                            DetailsId = userDetails[1].Id,
                            OrderIds = new List<ObjectId> { orders[2].Id, orders[3].Id },
                            RelationIds = new List<ObjectId> { userRelation[1].Id }
                        }

                    };
                await TestDB!.GetCollection<User>("Users").InsertManyAsync(user);

                //await session.CommitTransactionAsync();
                Console.WriteLine("資料已成功插入。");
            }
            catch (Exception ex)
            {
                //await session.AbortTransactionAsync();
                Console.WriteLine(ex.ToString());
            }
            //}
        }

        static async Task InsertProductDataAsync()
        {
            var productsCollection = TestDB!.GetCollection<Product>("Products");

            var products = new List<Product>
            {
                new Product { Id = ObjectId.GenerateNewId(), ProductId = 201, ProductName = "Laptop", Category = "Electronics", Price = 1200.0 },
                new Product { Id = ObjectId.GenerateNewId(), ProductId = 202, ProductName = "Mouse", Category = "Electronics", Price = 25.0 },
                new Product { Id = ObjectId.GenerateNewId(), ProductId = 203, ProductName = "Keyboard", Category = "Electronics", Price = 100.0 },
                new Product { Id = ObjectId.GenerateNewId(), ProductId = 204, ProductName = "Screen", Category = "Electronics", Price = 25000.0 }
            };

            await productsCollection.InsertManyAsync(products);
            Console.WriteLine("產品資料已成功插入。");
        }
        #endregion

        #region 其他查詢範例
        /// <summary>
        /// 使用Bson格式查詢
        /// </summary>
        /// <returns></returns>
        static async Task QueryOrdersAsBsonDocumentAsync()
        {
            var ordersCollection = TestDB!.GetCollection<BsonDocument>("Orders");

            // 使用 BsonDocument 查詢
            var filter = new BsonDocument(); // 空的篩選條件，表示查詢所有文件
            var orders = await ordersCollection.Find(filter).ToListAsync();

            Console.WriteLine("查詢結果（BsonDocument 格式）:");
            foreach (var order in orders)
            {
                Console.WriteLine(order.ToJson(jsonSettings)); // 將每個訂單的 BsonDocument 轉換為 JSON 格式並顯示
            }
        }

        /// <summary>
        /// 使用Dynamic查詢
        /// </summary>
        /// <returns></returns>
        static async Task QueryUsersAsDynamicAsync()
        {
            var usersCollection = TestDB!.GetCollection<BsonDocument>("Users");

            // 使用 BsonDocument 查詢
            var filter = new BsonDocument();
            var users = await usersCollection.Find(filter).ToListAsync();

            Console.WriteLine("查詢結果（Dynamic 格式）:");
            foreach (var userDoc in users)
            {
                dynamic user = BsonTypeMapper.MapToDotNetValue(userDoc); // 將 BsonDocument 映射為 .NET 動態類型
                Console.WriteLine($"User ID: {user["user_id"]}, Username: {user["username"]}, Email: {user["email"]}");
            }
        }
        #endregion

        #region 公用函式
        static async Task DropCollectionAsync(string collectionName)
        {
            try
            {
                // 刪除指定的集合
                await TestDB!.DropCollectionAsync(collectionName);
                Console.WriteLine($"集合 '{collectionName}' 已成功刪除。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除集合 '{collectionName}' 時發生錯誤: {ex.Message}");
            }
        }
        #endregion
    }

}
