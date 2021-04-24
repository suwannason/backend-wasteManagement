
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class Invoices
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string lotNo { get; set; }
        public Companies company { get; set; }
        public backend.request.Profile fae_prepared { get; set; }
        public backend.request.Profile fae_checked { get; set; }
        public backend.request.Profile fae_approved { get; set; }
        public backend.request.Profile gm_approved { get; set; }

        // HEADER INVOICE MAKING
        public string createDate { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string status { get; set; }
    }
}