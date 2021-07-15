
using backend.request;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{

    public class ITCinvoiceSchema
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string files { get; set; }
        public string summaryId { get; set; }
        public string status { get; set; }
        public string rejectCommend { get; set; }
        public Profile prepare { get; set; }
        public Profile check { get; set; }
        public Profile approve { get; set; }
        public string createDate { get; set; }
    }
}