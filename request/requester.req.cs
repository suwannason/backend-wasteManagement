

namespace backend.request
{

    public class ReuqesterREQ
    {
        public string date { get; set; }
        public string time { get; set; }
        public string dept { get; set; }
        public string div { get; set; }

        public Hazardous[] hazardous { get; set; }
        public Infections[] infections { get; set; }
        public ScrapMatrialimo[] scrapImo { get; set; }
    }

    public class Hazardous
    {
        public string no { get; set; }
        public string phase { get; set; }
        public string wasteEachDept { get; set; }
        public string wasteName { get; set; }
        public string wasteGroup { get; set; }
        public string contractorCompany { get; set; }
        public string binddingType { get; set; }
        public string cptMainType { get; set; }
        public string wasteType { get; set; }
        public string boiType { get; set; }
        public string partNormalType { get; set; }
        public string productType { get; set; }
        public string lotNo { get; set; }
        public string companyApproveNo { get; set; }
        public string containerType { get; set; }
        public string qtyOfContainer { get; set; }
        public string weightPerContainer { get; set; }
        public string totalWeight { get; set; }
    }
    public class Infections
    {
        public string no { get; set; }
        public string wasteEachDept { get; set; }
        public string wasteName { get; set; }
        public string biddingType { get; set; }
        public string contractorCompany { get; set; }
        public string containerType { get; set; }
        public string qtyOfContainer { get; set; }
        public string weightPerContainer { get; set; }
        public string totalWeight { get; set; }
    }
    public class ScrapMatrialimo
    {
        public string summaryType { get; set; }
        public string imoLotNo { get; set; }
        public string moveOutDate { get; set; }
        public string no { get; set; }
        public string matrialCode { get; set; }
        public string matrialName { get; set; }
        public string boiType { get; set; }
        public string groupBoiNo { get; set; }
        public string groupBoiName { get; set; }
        public string unit { get; set; }
        public string totalWeight { get; set; }
        public string containerWeight { get; set; }
        public string qtyOfContainer { get; set; }
        public string containerType { get; set; }
        public string netWasteWeight { get; set; }
    }

    public class UpdateStatusFormRequester
    {
        public string[] trackingId { get; set; }
        public string status { get; set; }
    }
    public class getByStatus
    {
        public string trackingId { get; set; }
        public string status { get; set; }
    }
    public class maxlotRequest
    {
        public string dept { get; set; }
    }
    public class uploadData
    {
        public string form { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile file { get; set; }
    }
}