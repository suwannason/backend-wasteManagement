

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class RecycleWeste
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string carType { get; set; }

        public string company { get; set; }
        public string wasteType { get; set; }

        public string date { get; set; }

        public string time { get; set; }

        public bool isBoi { get; set; }

        public string year { get; set; }

        public string month { get; set; }

        public string status { get; set; }

        public string idMapping { get; set; }
    }
}