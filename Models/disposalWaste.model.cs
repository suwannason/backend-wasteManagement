

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class DisposalWaste
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string wasteType { get; set; }

        public string company { get; set; }
        public string carType { get; set; }

        public string date { get; set; }
        public string time { get; set; }
        public DisposalDetail[] detail { get; set; }
    }

    public class DisposalDetail {
        public string _id { get; set; }

    }
}