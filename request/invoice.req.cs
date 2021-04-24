namespace backend.request
{
    public class RequestInvoiceUpdateStatus {
        public string status { get; set; }

        public string[] body { get; set; }
    }

    public class createInvoice {
        public string[] lotNo { get; set; }

        public backend.Models.Companies company { get; set; }
    }
}