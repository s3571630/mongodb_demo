using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Services
{

    public class MongoSchemaManager
    {
        private readonly IMongoDatabase _database;

        public MongoSchemaManager(IMongoDatabase database)
        {
            _database = database;
        }

        // 創建集合並應用 Schema 驗證規則
        public async Task ApplySchemaValidationAsync(string collectionName, BsonDocument schema)
        {
            var collectionExists = await CollectionExistsAsync(collectionName);

            if (!collectionExists)
            {
                var options = new CreateCollectionOptions<BsonDocument>
                {
                    Validator = new BsonDocumentFilterDefinition<BsonDocument>(schema)
                };

                await _database.CreateCollectionAsync(collectionName, options);
                Console.WriteLine($"集合 '{collectionName}' 已創建並設定 Schema 驗證。");
            }
            else
            {
                Console.WriteLine($"集合 '{collectionName}' 已存在，無需重新創建。");
            }
        }

        // 更新已存在集合的 Schema 驗證規則
        public async Task UpdateSchemaValidationAsync(string collectionName, BsonDocument schema)
        {
            var collectionExists = await CollectionExistsAsync(collectionName);

            if (collectionExists)
            {
                var updateCommand = new BsonDocument
                {
                    { "collMod", collectionName },
                    { "validator", schema }, // 直接使用 BsonDocument 傳入
                    { "validationLevel", "moderate" } // 設定驗證層級，可選 strict, moderate, off
                };

                await _database.RunCommandAsync<BsonDocument>(updateCommand);
                Console.WriteLine($"集合 '{collectionName}' 的 Schema 驗證規則已更新。");
            }
            else
            {
                Console.WriteLine($"集合 '{collectionName}' 不存在，無法更新 Schema 驗證規則。");
            }
        }

        // 檢查集合是否存在
        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var collections = await _database.ListCollectionNamesAsync();
            var collectionNames = await collections.ToListAsync();
            return collectionNames.Contains(collectionName);
        }
    }
}
