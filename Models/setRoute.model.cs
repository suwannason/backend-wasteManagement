
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class Route
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string[] topic {get; set; }
    }

    public class DetailRouteStep {

        public string empNo { get; set; }
        public string email { get; set; }
    }
}