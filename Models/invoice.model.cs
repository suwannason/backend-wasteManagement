
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class Invoices
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string contractNo { get; set; }

        public string contractStartDate { get; set; }

        public string contractEndDate { get; set; }

        public string counterpartyName { get; set; }

        public string phoneNo { get; set; }

        public string fax { get; set; }

        public string counterpartyAddress { get; set; }

        public string counterpartyChange { get; set; }

        public string counterPartyChangePosition { get; set; }

        public string invoiceDate { get; set; }

        public string subTotal { get; set; }
        public string grandTotal { get; set; }

        // HEADER INVOICE MAKING

        [BsonDefaultValue("")]
        public bool typeBoi { get; set; }

        [BsonDefaultValue("")]
        public string lotNo { get; set; }

        [BsonDefaultValue("")]
        public string moveOutDate { get; set; }

        [BsonDefaultValue("")]
        public string wasteName { get; set; }

        // HEADER INVOICE MAKING

        public string createBy { get; set; }
        public long createDate { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }

        public response.WasteRecordInvoice[] wasteItem { get; set; }
    }
}