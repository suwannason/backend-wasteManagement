
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class Invoices
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string[] summaryId { get; set; }
        public company company { get; set; }
        public backend.request.Profile fae_prepared { get; set; }
        public backend.request.Profile fae_checked { get; set; }
        public backend.request.Profile fae_approved { get; set; }
        public backend.request.Profile gm_approved { get; set; }
        public backend.request.Profile acc_prepare { get; set; }
        public backend.request.Profile acc_check { get; set; }
        public backend.request.Profile acc_approve { get; set; }

        // HEADER INVOICE MAKING
        public string createDate { get; set; }
        public string invoiceDate { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }
        public string totalPrice { get; set; }
        public string rejectCommend { get; set; }

        // ACC INPUT
        public string invoiceNo { get; set; }
        public string termsOfPayment { get; set; }
        public string dueDate { get; set; }
        public string poNo { get; set; }
        // ACC INPUT
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
        public string attnRef { get; set; }
        public string customerCode { get; set; }
    }
}