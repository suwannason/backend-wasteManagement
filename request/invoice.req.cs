namespace backend.request
{
    public class RequestInvoiceUpdateStatus {
        public string status { get; set; }

        public string[] id { get; set; }
    }

    public class createInvoice {
        public string[] lotNo { get; set; }
        
        public string invoiceDate { get; set; }
        public backend.Models.company company { get; set; }
    }

    public class getInvoice {
        public string id { get; set; }
    }
}