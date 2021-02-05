
using backend.request;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class ScrapMatrialimoSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string dept { get; set; }
        public string div { get; set; }

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
        // APPROVING STEP
        public Profile req_prepared { get; set; }
        public Profile req_checked { get; set; }
        public Profile req_approved { get; set; }
        public Profile itc_checked { get; set; }
        public Profile itc_approved { get; set; }
        public Profile fae_checked { get; set; }
        public Profile fae_approved { get; set; }
        // APPROVING STEP

        // FIELD ON MERGE
        public string biddingNo { get; set; }
        public string biddingType { get; set; }
        public string color { get; set; }
        public string unitPrice { get; set; }
        public string totalPrice { get; set; }
        // FIELD ON MERGE
        public string year { get; set; }
        public string status { get; set; }
    }
}