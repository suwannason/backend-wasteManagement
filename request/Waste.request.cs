
using Microsoft.AspNetCore.Http;
using backend.Models;

namespace backend.request {

    public class RequestRecycle {

        public string date { get; set; }
        public string time { get; set; }
        public string phase { get; set; }
        public string cptMainType { get; set; }
        public string wasteType { get; set; }
        public string boiType { get; set; }
        public string partNormalType { get; set; }
        public string department { get; set; }
        public string division { get; set; }
        public string lotNo { get; set; }
        public string biddingType { get; set; }
        public string companyApprove { get; set; }

        public string contractorCompany { get; set; }

        public string productionType { get; set; }
        public string wasteGroup { get; set; }
        public string wasteName { get; set; }
        public string totalWeight { get; set; }
        public string containerWeight { get; set; }

        public string qtyOfContainer { get; set; }
        public string containerType { get; set; }
        public string netWasteWeight { get; set; }
        public string status { get; set; }
        public string[] imageCapture { get; set; }
        public  IFormFile[] files { get; set; }
    }

    public class RequestInvoiceDataAll {
        public string typeBoi { get; set; }
        public string lotNo { get; set; }
        public string date { get; set; }
        public string wasteName { get; set; }
    }

    public class RequestInvoiceDataWithName {
        public string wasteName { get; set; }
    }
}