
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string role { get; set; }

        public string position { get; set; }

    }
}