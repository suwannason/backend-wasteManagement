

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

        public string year { get; set; }

        public string month { get; set; }

        public string status { get; set; }
        public string time { get; set; }
        public DisposalDetail[] detail { get; set; }
    }

    public class DisposalDetail
    {
        public DisposalDetail()
        {
            _id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string factory { get; set; }
        public string subWasteType { get; set; }
        public string weight { get; set; }
        public string[] fileAttached { get; set; }
    }
}