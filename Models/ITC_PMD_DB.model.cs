
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class ITC_PMD_DB
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string matrialCode { get; set; } //C
        public string dim { get; set; }
        public string matrial { get; set; }
        public string matrialName { get; set; }
        public string partForCheck { get; set; }
        public string partNo { get; set; }
        public string unitNo { get; set; }
        public string partName { get; set; }
        public string part_matrial_name { get; set; } //K
        public string sizeT { get; set; }
        public string sizeW { get; set; }
        public string sizeL { get; set; }
        public string sizePitch { get; set; }
        public string CTsystem { get; set; }
        public string mc { get; set; }
        public string vendor { get; set; }
        public string remark { get; set; }


        // ITC CONFIRM
        public string privilegeType { get; set; }
        public string bioGroup { get; set; }
        public string bioName { get; set; }
        public string unit { get; set; }
        public string supplier { get; set; }
        public string remark_itc { get; set; }

    }

}