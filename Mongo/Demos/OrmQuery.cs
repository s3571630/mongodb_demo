using Mongo.Models;
using Mongo.ViewModels;
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
    /// 物件對應查詢
    /// </summary>
    public class OrmQuery
    {
        private IMongoDatabase? TestDB;

        public OrmQuery(IMongoDatabase? db)
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var results = await ordersCollection.Find(order => order.Total > 500).ToListAsync();

            // 使用 $match 在資料庫篩選
            //var results = await ordersCollection.Aggregate()
            //    .Match(order => order.Total > 500)
            //    .ToListAsync();

            Console.WriteLine("篩選總金額大於 500 的訂單:");
            foreach (var order in results)
            {
                Console.WriteLine($"Order ID: {order.OrderId}, Total: {order.Total}");
                foreach (var item in order.Items)
                {
                    Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
                }
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var groupResult = await ordersCollection.Aggregate()
                .Group(
                    o => o.UserId,
                    g => new
                    {
                        UserId = g.Key,
                        total_orders = g.Count(),
                        total_amount = g.Sum(x => x.Total)
                    })
                .ToListAsync();

            Console.WriteLine("按使用者 ID 分組的結果:");
            foreach (var result in groupResult)
            {
                Console.WriteLine($"User ID: {result.UserId}, Total Orders: {result.total_orders}, Total Amount: {result.total_amount}");
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var results = await ordersCollection.Find(_ => true)
                .SortByDescending(order => order.Total)
                .ToListAsync();

            // 使用 $sort 來進行排序
            //var results = await ordersCollection.Aggregate()
            //    .SortByDescending(order => order.Total)
            //    .ToListAsync();

            Console.WriteLine("按總金額降序排序訂單:");
            foreach (var order in results)
            {
                Console.WriteLine($"Order ID: {order.OrderId}, Total: {order.Total}");
                foreach (var item in order.Items)
                {
                    Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
                }
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var results = await ordersCollection.Find(_ => true)
                .Project(order => new { order.OrderId, order.Total })
                .ToListAsync();

            //var results = await ordersCollection.Aggregate()
            //    .Project(order => new { order.OrderId, order.Total })
            //    .ToListAsync();

            Console.WriteLine("只顯示 OrderId 和 Total 欄位:");
            foreach (var order in results)
            {
                Console.WriteLine($"Order ID: {order.OrderId}, Total: {order.Total}");
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            // 使用 $lookup 直接在資料庫中連接
            var results = await ordersCollection.Aggregate()
                .Lookup<Order, User, OrderWithUser>(
                    TestDB.GetCollection<User>("Users"),
                    order => order.UserId,
                    user => user.UserId,
                    result => result.Users)
                .ToListAsync();

            Console.WriteLine("查詢訂單並連接使用者資訊:");
            foreach (var result in results)
            {
                var user = result.Users.FirstOrDefault();
                Console.WriteLine($"Order ID: {result.OrderId}, User: {user?.Username}, Total: {result.Total}");
                foreach (var item in result.Items)
                {
                    Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
                }
            }

            #region 效能差寫法

            //var ordersCollection = TestDB!.GetCollection<Order>("Orders");
            //var usersCollection = TestDB!.GetCollection<User>("Users");

            //var orders = await ordersCollection.Find(_ => true).ToListAsync();
            //var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            //var users = await usersCollection.Find(u => userIds.Contains(u.UserId)).ToListAsync();

            //Console.WriteLine("查詢訂單並連接使用者資訊:");
            //foreach (var order in orders)
            //{
            //    var user = users.FirstOrDefault(u => u.UserId == order.UserId);
            //    Console.WriteLine($"Order ID: {order.OrderId}, User: {user?.Username}, Total: {order.Total}");
            //    foreach (var item in order.Items)
            //    {
            //        Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
            //    }
            //}
            #endregion
        }



        /// <summary>
        /// 動態計算和新增欄位（$addFields）
        /// </summary>
        /// <remarks>
        /// 這個方法用來按使用者分組並計算每個使用者的訂單總金額及其稅金。
        /// </remarks>
        public async Task AggregateAndCalculateWithObjectAsync()
        {
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var results = await ordersCollection.Aggregate()
                .Group(
                    o => o.UserId,
                    g => new
                    {
                        UserId = g.Key,
                        TotalAmount = g.Sum(x => x.Total)
                    })
                .Project(result => new
                {
                    result.UserId,
                    result.TotalAmount,
                    Tax = result.TotalAmount * 0.05
                })
                .ToListAsync();

            Console.WriteLine("動態計算聚合結果:");
            foreach (var result in results)
            {
                Console.WriteLine($"User ID: {result.UserId}, Total Amount: {result.TotalAmount}, Tax: {result.Tax}");
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var results = await ordersCollection.Aggregate()
                .Lookup<Order, User, OrderWithUserDetails>(
                    TestDB.GetCollection<User>("Users"),
                    order => order.UserId,
                    user => user.UserId,
                    result => result.Users)
                .Lookup<OrderWithUserDetails, UserDetails, OrderWithUserDetails>(
                    TestDB.GetCollection<UserDetails>("UserDetails"),
                    order => order.Users.First().UserId,
                    details => details.UserId,
                    result => result.UserDetails)
                .ToListAsync();

            Console.WriteLine("查詢訂單及其相關使用者的詳細資訊:");
            foreach (var result in results)
            {
                var user = result.Users.FirstOrDefault();
                var details = result.UserDetails.FirstOrDefault();

                if (user != null && details != null)
                {
                    Console.WriteLine($"Order ID: {result.OrderId}, User: {user.Username}, Email: {user.Email}, Address: {details.Address}, Total: {result.Total}");
                    foreach (var item in result.Items)
                    {
                        Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
                    }
                }
            }

            #region 效能差寫法
            //var ordersCollection = TestDB!.GetCollection<Order>("Orders");
            //var usersCollection = TestDB!.GetCollection<User>("Users");
            //var userDetailsCollection = TestDB!.GetCollection<UserDetails>("UserDetails");

            //var orders = await ordersCollection.Find(_ => true).ToListAsync();
            //var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            //var users = await usersCollection.Find(u => userIds.Contains(u.UserId)).ToListAsync();
            //var userDetails = await userDetailsCollection.Find(d => userIds.Contains(d.UserId)).ToListAsync();

            //Console.WriteLine("查詢訂單及其相關使用者的詳細資訊:");
            //foreach (var order in orders)
            //{
            //    var user = users.FirstOrDefault(u => u.UserId == order.UserId);
            //    var details = userDetails.FirstOrDefault(d => d.UserId == order.UserId);

            //    if (user != null && details != null)
            //    {
            //        Console.WriteLine($"Order ID: {order.OrderId}, User: {user.Username}, Total: {order.Total}, Address: {details.Address}, Email: {user.Email}");
            //        foreach (var item in order.Items)
            //        {
            //            Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
            //        }
            //    }
            //}
            #endregion
        }






        /// <summary>
        /// 查詢使用者及其所有訂單的總金額
        /// </summary>
        /// <remarks>
        /// 這個方法使用 $lookup 查詢每個使用者的所有訂單，並計算每個使用者的訂單總金額。
        /// </remarks>
        public async Task LookupUsersWithTotalOrderAmountAsync()
        {
            var usersCollection = TestDB!.GetCollection<User>("Users");
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var users = await usersCollection.Find(_ => true).ToListAsync();
            var orders = await ordersCollection.Find(_ => true).ToListAsync();

            var result = from user in users
                         join order in orders on user.UserId equals order.UserId into userOrders
                         select new
                         {
                             user.UserId,
                             user.Username,
                             TotalOrderAmount = userOrders.Sum(o => o.Total)
                         };

            Console.WriteLine("查詢使用者及其所有訂單的總金額:");
            foreach (var item in result)
            {
                Console.WriteLine($"User ID: {item.UserId}, Username: {item.Username}, Total Order Amount: {item.TotalOrderAmount}");
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
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");
            var usersCollection = TestDB!.GetCollection<User>("Users");
            var userDetailsCollection = TestDB!.GetCollection<UserDetails>("UserDetails");
            var productsCollection = TestDB!.GetCollection<Product>("Products");

            // 查詢所有訂單
            var orders = await ordersCollection.Find(_ => true).ToListAsync();
            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var users = await usersCollection.Find(u => userIds.Contains(u.UserId)).ToListAsync();
            var userDetails = await userDetailsCollection.Find(d => userIds.Contains(d.UserId)).ToListAsync();

            // 查詢訂單中涉及的產品
            var productIds = orders.SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToList();
            var products = await productsCollection.Find(p => productIds.Contains(p.ProductId)).ToListAsync();

            Console.WriteLine("查詢訂單及其相關的使用者和產品資訊:");
            foreach (var order in orders)
            {
                var user = users.FirstOrDefault(u => u.UserId == order.UserId);
                var details = userDetails.FirstOrDefault(d => d.UserId == order.UserId);

                if (user != null && details != null)
                {
                    Console.WriteLine($"Order ID: {order.OrderId}, User: {user.Username}, Email: {user.Email}, Address: {details.Address}, Total: {order.Total}");
                    foreach (var item in order.Items)
                    {
                        var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                        Console.WriteLine($"  Product: {product?.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}, Category: {product?.Category}");
                    }
                }
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
            var usersCollection = TestDB!.GetCollection<User>("Users");
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");

            var users = await usersCollection.Find(_ => true).ToListAsync();
            var orders = await ordersCollection.Find(_ => true).ToListAsync();

            var latestOrders = orders
                .GroupBy(o => o.UserId)
                .Select(g => g.OrderByDescending(o => o.OrderDate).FirstOrDefault())
                .ToList();

            Console.WriteLine("查詢使用者及其最新的訂單:");
            foreach (var user in users)
            {
                var latestOrder = latestOrders.FirstOrDefault(o => o.UserId == user.UserId);
                if (latestOrder != null)
                {
                    Console.WriteLine($"User ID: {user.UserId}, Username: {user.Username}, Latest Order ID: {latestOrder.OrderId}, Total: {latestOrder.Total}");
                    foreach (var item in latestOrder.Items)
                    {
                        Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
                    }
                }
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
            var productsCollection = TestDB!.GetCollection<Product>("Products");
            var ordersCollection = TestDB!.GetCollection<Order>("Orders");
            var usersCollection = TestDB!.GetCollection<User>("Users");

            var products = await productsCollection.Find(_ => true).ToListAsync();
            var orders = await ordersCollection.Find(_ => true).ToListAsync();
            var users = await usersCollection.Find(_ => true).ToListAsync();

            Console.WriteLine("查詢所有產品並找出購買過該產品的使用者:");
            foreach (var product in products)
            {
                var relevantOrders = orders.Where(o => o.Items.Any(i => i.ProductId == product.ProductId)).ToList();
                var buyers = relevantOrders.Select(o => users.FirstOrDefault(u => u.UserId == o.UserId)).Distinct().ToList();

                Console.WriteLine($"Product: {product.ProductName}, Category: {product.Category}, Price: {product.Price}, Order Count: {relevantOrders.Count}, Buyer Count: {buyers.Count}");
                foreach (var buyer in buyers)
                {
                    Console.WriteLine($"  Buyer: {buyer?.Username}, Email: {buyer?.Email}");
                }
            }
        }

    }
}
