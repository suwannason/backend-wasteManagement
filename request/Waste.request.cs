
using Microsoft.AspNetCore.Http;

namespace backend.request {

    public class RequestRecycle {

        public string date { get; set; }
        public string time { get; set; }
        public string phase { get; set; }
        public bool typeBoi { get; set; }
        public string cptType { get; set; }
        public string lotNo { get; set; }
        public string companyApprove { get; set; }
        public string gennerateGroup { get; set; }
        public string wasteGroup { get; set; }
        public string wasteName { get; set; }
        public string totalWeight { get; set; }
        public string containerWeight { get; set; }

        public string qtyOfContainer { get; set; }
        public string containerType { get; set; }
        public string netWasteWeight { get; set; }
        public string wasteContractor { get; set; }
        public string status { get; set; }
        public string[] imageCapture { get; set; }
        public  IFormFile[] files { get; set; }
    }

    public class RequestInvoiceDataAll {
        public bool typeBoi { get; set; }
        public string lotNo { get; set; }
        public string date { get; set; }
    }

    public class RequestInvoiceDataWithName {
        public string wasteName { get; set; }
    }
}