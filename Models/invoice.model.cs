
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class Invoices
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string[] lotNo { get; set; }
        public company company { get; set; }
        public backend.request.Profile fae_prepared { get; set; }
        public backend.request.Profile fae_checked { get; set; }
        public backend.request.Profile fae_approved { get; set; }
        public backend.request.Profile gm_approved { get; set; }

        // HEADER INVOICE MAKING
        public string createDate { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }
    }
    public class company
    {
        public string no { get; set; }
        public string contractNo { get; set; }
        public string contractStartDate { get; set; }
        public string contractEndDate { get; set; }
        public string companyName { get; set; }
        public string fax { get; set; }
        public string tel { get; set; }
        public string address { get; set; }
        public string invoiceDate { get; set; }
    }
}