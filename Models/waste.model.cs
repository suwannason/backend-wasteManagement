

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using backend.request;

namespace backend.Models
{
    public class Waste
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string moveOutDate { get; set; }
        public string time { get; set; }
        public string phase { get; set; }
        public string cptMainType { get; set; }
        public string wasteType { get; set; }
        public string boiType { get; set; }
        public string partNormalType { get; set; }

        public string department { get; set; }
        public string productionType { get; set; }
        public string division { get; set; }
        public string lotNo { get; set; }
        public string companyApprove { get; set; }
        public string wasteGroup { get; set; }
        public string wasteName { get; set; }
        public string unit { get; set; }
        public string totalWeight { get; set; }
        public string containerWeight { get; set; }
        public string containerType { get; set; }
        public string netWasteWeight { get; set; }
        public string contractorCompany { get; set; }

        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }

        public string qtyOfContainer { get; set; }
        public long createDate { get; set; }
        public string rejectCommend { get; set; }

        public string[] files { get; set; }
        public Profile prepareBy { get; set; }
        public Profile checkBy { get; set; }
        public Profile approveBy { get; set; }
        public Profile makingBy { get; set; }
        public bool invoiceRef { get; set; } // true = invoice , false = refferance

        // pricing
        public string biddingNo { get; set; }
        public string biddingType { get; set; }
        public string color { get; set; }
        public string unitPrice { get; set; }
        public string totalPrice { get; set; }
        // pricing

        //company
        public string contractStartDate { get; set; }
        public string contractEndDate { get; set; }
        public string contractNo { get; set; }
        //company
    }

    public class wasteGroupedRecord {
        public string moveOutDate { get; set; }
        public string phase { get; set; }
        public string boiType { get; set; }
        public string netWasteWeight { get; set; }
        public string cptMainType { get; set; }
        public string lotNo { get; set; }
        public string companyApprove { get; set; }
        public string wasteGroup { get; set; }
        public string[] id { get; set; }
    }
}