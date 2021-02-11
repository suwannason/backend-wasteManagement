
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models {

    public class pricingSchema {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string[] wasteName { get; set; }
        public string biddingType { get; set; }
        public string color { get; set; }
        public string kind { get; set; }
        public string[] matrialCode { get; set; }
        public double price { get; set; }
    }
}