
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class faeDBschema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string biddingNo { get; set; }
        public string biddingType { get; set; }
        public string wasteName { get; set; }
        public string color { get; set; }
        public string kind { get; set; }
        public string unit { get; set; }
        public string pricePerUnit { get; set; }
        public string matrialCode { get; set; }
        public string matrialName { get; set; }
        public string colorName { get; set; }
    }
}