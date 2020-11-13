
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class Quotation
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string quotationNo { get; set; }

        public string wasteName { get; set; }

        public double unitPrice { get; set; }
    }
}