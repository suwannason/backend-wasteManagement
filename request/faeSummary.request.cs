
using System.ComponentModel.DataAnnotations;

namespace backend.request
{
    public class dataSearch
    {
        public string lotNo { get; set; }
        //  [Required]
        public string wasteName { get; set; }
        public string phase { get; set; }
        //  [Required]
        public string startDate { get; set; }
        //  [Required]
        public string endDate { get; set; }

    }

    public class lotAndboi
    {
        public string lotNo { get; set; }
        public string boiType { get; set; }
    }

    public class createSummary
    {

        [Required]
        public string type { get; set; }
        public lotAndboi[] requester { get; set; }
        public string[] recycle { get; set; }
        public string mainInvoice { get; set; }
    }
    public class updateStatus
    {

        public string[] id { get; set; }
        public string status { get; set; }
    }
    public class updateTotal
    {
        public string id { get; set; }
        public string totalPrice { get; set; }
        public string totalWeight { get; set; }
    }
    public class RejectSummary
    {
        public string[] id { get; set; }
        public string commend { get; set; }
    }
    public class updateConsistent {
        public string id { get; set; }
        public string consistent { get; set; }
    }
}