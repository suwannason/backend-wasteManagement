

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class Waste
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string phase { get; set; }
        public string typeBoi { get; set; }
        public string cptType { get; set; }
        public string lotNo { get; set; }
        public string companyApprove { get; set; }
        public string gennerateGroup { get; set; }
        public string wasteGroup { get; set; }
        public string wasteName { get; set; }
        public string totalWeight { get; set; }
        public string containerWeight { get; set; }
        public string containerType { get; set; }
        public string netWasteWeight { get; set; }
        public string wasteContractor { get; set; }

        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }
        
        public string qtyOfContainer { get; set; }
        public long createDate { get; set; }

        public string createBy { get; set; }
        public  string[] files { get; set; }
    }
}