
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models
{
    public class FAEPreparedLotSchema
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string lotNo { get; set; }
        public string howTodestory { get; set; }
        public bool allowToDestroy { get; set; }
        public string remark { get; set; }
        public backend.request.Profile preparedBy { get; set; }
        public string createDate { get; set; }
    }
}