

namespace backend.Models
{

    public class InvoicePrinted
    {
        public company company { get; set; }
        public InvoiceprintingDetail invoice { get; set; }
        public InvoiceprintingItems[] detail { get; set; }
    }


    public class InvoiceprintingDetail
    {
        public string invoiceNo { get; set; }
        public string invoiceDate { get; set; }
        public string termOfPayment { get; set; }
        public string dueDate { get; set; }
        public string customerCode { get; set; }
        public string poNo { get; set; }
    }
    public class InvoiceprintingItems {
        public int no { get; set; }
        public string wastename { get; set; }
        public string quantity { get; set; }
        public string unit { get; set; }
        public string unitPrice { get; set; }
        public string totalPrice { get; set; }

    }
}