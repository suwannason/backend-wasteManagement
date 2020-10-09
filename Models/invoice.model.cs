
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class Invoices
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string factory { get; set; }

        public string wasteType { get; set; }

        public string date { get; set; }

        public bool isBoi { get; set; }

        public string receivWeightNo { get; set; }
        public string weight { get; set; }

        public string person { get; set; }

        public string [] files { get; set; }

        public string year {get; set; }

    }
}