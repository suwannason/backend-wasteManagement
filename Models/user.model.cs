
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

        public string email { get; set; }

        public string tel { get; set; }

        public string band { get; set; }

        public bool canLogin { get; set; }

        public string name { get ;set; }

        public permission[] permission { get; set; }
    }

    public class permission {

        public permission() {
            _id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; }

        public string dept { get; set; }

        public string feature { get; set; }

        public string action { get; set; }
    }
}