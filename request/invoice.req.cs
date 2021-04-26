namespace backend.request
{
    public class RequestInvoiceUpdateStatus {
        public string status { get; set; }

        public string[] lotNo { get; set; }
    }

    public class createInvoice {
        public string[] lotNo { get; set; }

        public backend.Models.company company { get; set; }
    }
}