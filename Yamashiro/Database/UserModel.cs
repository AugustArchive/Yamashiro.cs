using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Yamashiro.Database
{
    public class UserModel
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("opted")]
        public bool Opted { get; set; }

        [BsonElement("userID")]
        public ulong UserID { get; set; }
    }
}