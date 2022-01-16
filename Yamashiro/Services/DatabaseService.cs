using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Yamashiro.Database;
using Yamashiro.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Yamashiro.Services
{
    public class DatabaseService
    {
        private readonly MongoClient client;
        private readonly Logger logger;
        public int Calls;

        public DatabaseService(IConfigurationRoot _config)
        {
            client = new MongoClient(_config["databaseUrl"]);
            logger = new Logger("DatabaseService");
            Calls = 0;
        }

        public IMongoDatabase GetDatabase() => client.GetDatabase("Yamashiro");

        public async Task<UserModel> GetUser(ulong id)
        {
            Calls++;
            var database = this.GetDatabase();
            var collection = database.GetCollection<UserModel>("users");
            var filter = Builders<UserModel>.Filter.Eq("userID", id);
            var doc = collection.Find(filter).FirstOrDefault();

            return doc;
        }

        public async Task<UserModel> CreateUser(ulong id)
        {
            Calls++;
            var database = this.GetDatabase();
            var collection = database.GetCollection<UserModel>("users");
            var pkt = new UserModel
            {
                UserID = id,
                Opted = false,
                Id = new ObjectId()
            };

            await collection.InsertOneAsync(pkt);
            return pkt;
        }

        public async Task UpdateUser(ulong id, UpdateDefinition<UserModel> update)
        {
            Calls++;
            var database = this.GetDatabase();
            var collection = database.GetCollection<UserModel>("users");
            var filter = Builders<UserModel>.Filter.Eq("userID", id);
            await collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteUser(ulong id)
        {
            Calls++;
            var database = this.GetDatabase();
            var collection = database.GetCollection<UserModel>("users");
            var filter = Builders<UserModel>.Filter.Eq("userID", id);

            await collection.DeleteOneAsync(filter);
        }
    }
}