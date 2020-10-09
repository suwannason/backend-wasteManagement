using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class SubWasteType
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; }

        public string typeName { get; set; }

        public string mainType { get; set; }
    }

}