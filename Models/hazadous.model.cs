using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using backend.request;
namespace backend.Models
{
    public class HazadousSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string dept { get; set; }
        public string div { get; set; }
        public string phase { get; set; }
        public string filename { get; set; }
        public string netWasteWeight { get; set; }
        public HazadousItems[] items { get; set; }
        public string status { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string description { get; set; }
        public string rejectCommend { get; set; }
        public string runningNo { get; set; } // HZ-001/2021

        // APPROVING STEP
        public Profile req_prepared { get; set; }
        public Profile req_checked { get; set; }
        public Profile req_approved { get; set; }
        public Profile fae_prepared { get; set; }
        public Profile fae_checked { get; set; }
        public Profile fae_approved { get; set; }
        public Profile fae_received { get; set; }

        
        // APPROVING STEP
    }
    public class HazadousItems
    {
        public string no { get; set; }
        public string wasteName { get; set; }
        public string biddingType { get; set; }
        public string containerType { get; set; }
        public string howTodestroy { get; set; }
        public string workCount { get; set; }
        public string totalWeight { get; set; }
        public bool allowed { get; set; }
        public bool burn { get; set; }
        public bool recycle { get; set; }
    }
}
