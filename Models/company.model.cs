

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models
{
    public class Companies
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        
        public string companyName { get; set; }

    }
}