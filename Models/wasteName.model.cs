

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models
{
    public class WasteName
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        
        public string wasteName { get; set; }

        public string biddingType { get; set; }

        public string wasteGroup { get; set; }

        public string[] company { get; set; }
    

    }
}