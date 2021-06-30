using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using backend.request;
namespace backend.Models
{
    public class InfectionSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string dept { get; set; }
        public string div { get; set; }
        public string phase { get; set; }
        public string netWasteWeight { get; set; }
        public InfectionItems[] items { get; set; }
        public string status { get; set; }
        public string year { get; set; }
        public string month { get; set; }

        // APPROVING STEP
        public Profile req_prepared { get; set; }
        public Profile req_checked { get; set; }
        public Profile req_approved { get; set; }
        public Profile fae_prepared { get; set; }
        // APPROVING STEP
    }
    public class InfectionItems
    {
        public string no { get; set; }
        public string date { get; set; }
        public string totalWeight { get; set; }
        public string agency { get; set; }
        public string remark { get; set; }
    }
}
