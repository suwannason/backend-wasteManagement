

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class InvoicePrintedSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public company company { get; set; }
        public InvoiceprintingDetail invoice { get; set; }
        public InvoiceprintingItems[] detail { get; set; }
        public totalPrintingDetail total { get; set; }
        public string invoiceId { get; set; }
        public string printingDate { get; set; }
        public request.Profile printedBy { get; set; }
        public string attatchmentFile { get; set; }
    }


    public class InvoiceprintingDetail
    {
        public string invoiceNo { get; set; }
        public string invoiceDate { get; set; }
        public string termOfPayment { get; set; }
        public string dueDate { get; set; }
        public string customerCode { get; set; }
        public string poNo { get; set; }
        public string customerName { get; set; }
        public string address { get; set; }
        public string attnRef { get; set; }
    }
    public class InvoiceprintingItems
    {
        public int no { get; set; }
        public string wastename { get; set; }
        public string quantity { get; set; }
        public string unit { get; set; }
        public string unitPrice { get; set; }
        public string totalPrice { get; set; }

    }
    public class totalPrintingDetail
    {
        public string subTotal { get; set; }
        public string vat { get; set; }
        public string grandTotal { get; set; }
        public string bathString { get; set; }
    }
}