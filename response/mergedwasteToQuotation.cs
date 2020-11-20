
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.response
{
    public class WasteMergeQuotation {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public WasteRecordInvoice[] detail { get; set; }
        public double subTotal { get; set; }
        public double vat { get; set; }
        public double amount { get; set; }
    }

    public class WasteRecordInvoice {

        public string wasteId { get; set; }
        public string quotationNo { get; set; }
        public string wasteName { get; set; }
        public double quantity { get; set; }
        public double unitPrice { get; set; }
        public double amount { get; set; }
    }
}