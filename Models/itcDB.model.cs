
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class ITCDB
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string no { get; set; }
        public string matrialCode { get; set; }
        public string matrialName { get; set; }
        public string dept { get; set; }
        public string privilegeType { get; set; }
        public string groupBoiNo { get; set; }
        public string groupBoiName { get; set; }
        public string unit { get; set; }
        public string supplier { get; set; }
        public string remark { get; set; }


    }

}