
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models {

    public class ITC_IMO_SCHEMA {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string matrialCode { get; set; }
        public string matrialName { get; set; }
        public string type { get; set; }
        public string colorEn { get; set; }
        public string boiType { get; set; }
        public string groupBoiNo { get; set; }
        public string groupBoiName { get; set; }
    }
}