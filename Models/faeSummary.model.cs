using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using backend.request;

namespace backend.Models
{

    public class SummaryInvoiceSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string status { get; set; } // preapred, checked, approved
        public string type { get; set; }
        public bool exportRef { get; set; }
        public double requesterWeight { get; set; }
        public double recycleWeight { get; set; }
        public Profile prepare { get; set; }
        public Profile check { get; set; }
        public Profile approve { get; set; }
        public requesterUploadSchema[] requester { get; set; }
        public Waste[] recycle { get; set; }
        public string createDate { get; set; }

        public string totalWeight { get; set; }
        public string totalPrice { get; set; }
    }
}