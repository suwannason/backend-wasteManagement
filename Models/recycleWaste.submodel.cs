
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class SubRecycleWaste
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; }

        public string wasteType { get; set; }

        public string factory { get; set; }

        public string allWeight { get; set; }

        public string containerWeight { get; set; }

        public string total { get; set; }

        public string idMapping { get; set; }
        
    }
}