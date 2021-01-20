using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class WasteGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; }

        public string groupName { get; set; }

        public string[] wastename { get; set; }

        public subordnate[] subordnate {get; set; }
    }

    public class subordnate {
        public string subordnatewaste { get; set; }
        public string[] wastename { get; set; }
    }

}