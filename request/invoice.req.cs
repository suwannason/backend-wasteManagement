using System.ComponentModel.DataAnnotations;

namespace backend.request
{
    public class RequestInvoiceUpdateStatus {
        public string status { get; set; }

        public string[] id { get; set; }
    }

    public class createInvoice {
        public string[] summaryId { get; set; }
        
        [Required]
        public string invoiceDate { get; set; }
        public backend.Models.company company { get; set; }
    }

    public class getInvoice {
        public string id { get; set; }
    }

    public class AccPrepareInvoice {
        public string[] id { get; set; }
        public string invoiceNo { get; set; }
        public string termsOfPayment { get; set; }
        public string dueDate { get; set; }

    }
    public class ITCapproveInvoice {
        public string[] id { get; set; }
        public string status { get; set; }
    }
}