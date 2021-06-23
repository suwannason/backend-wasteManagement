
using backend.request;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class requesterUploadSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string date { get; set; }
        public string dept { get; set; } // fix to imo
        public string div { get; set; } // fix
        public string lotNo { get; set; } // 
        public string moveOutDate { get; set; } //
        public string moveOutMonth { get; set; }
        public string moveOutYear { get; set; }
        public string phase { get; set; }
        public string no { get; set; } //
        public string wasteName { get; set; } //
        public string kind { get; set; }
        public string matrialCode { get; set; } //
        public string matrialName { get; set; } //
        public string boiType { get; set; } // from db itc <privilegeType>
        public string groupBoiNo { get; set; } // from db itc <bioGroup>
        public string groupBoiName { get; set; } // from db itc <bioName>
        public string unit { get; set; } // from db itc <unit>
        public string totalWeight { get; set; } //
        public string containerWeight { get; set; }
        public string qtyOfContainer { get; set; } //
        public string netWasteWeight { get; set; } //
        // APPROVING STEP
        public Profile req_prepared { get; set; }
        public Profile req_checked { get; set; }
        public Profile req_approved { get; set; }

        public Profile itc_checked { get; set; }
        public Profile itc_approved { get; set; }
        public Profile pdc_prepared { get; set; }
        public Profile pdc_checked { get; set; }
        public Profile pdc_approved { get; set; }
        public Profile fae_prepared { get; set; }
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

        public bool invoiceRef { get; set; } // true = invoice , false = refferance
    }

    public class requesterGroupedRecord {
        public string moveOutDate { get; set; }
        public string phase { get; set; }
        public string boiType { get; set; }
        public string netWasteWeight { get; set; }
        public string[] id { get; set; }
    }
}