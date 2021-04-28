
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string dept { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string permission { get; set; }
        public string filename { get; set; }
    }
}