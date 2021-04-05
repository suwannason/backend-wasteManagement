
using backend.request;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models {

    public class ScrapMatrialpmdSchema {
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string date { get; set; }
        public string time { get; set; } //
        public string phase { get; set; } // 
        public string cptMainType { get; set; } //
        public string wasteType { get; set; } //
        public string boiType { get; set; } // itc
        public string partNormalType { get; set; }

        public string dept { get; set; } // pmd
        public string productionType { get; set; }
        public string div { get; set; }
        public string lotNo { get; set; }
        public string companyApprove { get; set; }
        public string wasteGroup { get; set; }
        public string wasteName { get; set; }
        public string totalWeight { get; set; }
        public string containerWeight { get; set; }
        public string containerType { get; set; }
        public string netWasteWeight { get; set; }
        public string biddingType { get; set; }
        public string contractorCompany { get; set; }

        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }

        public string qtyOfContainer { get; set; }
        public long createDate { get; set; }

        public string[] files { get; set; }
        public Profile prepareBy { get; set; }
        public Profile checkBy { get; set; }
        public Profile approveBy { get; set; }
    }
}