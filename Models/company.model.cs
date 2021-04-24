

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models
{
    public class Companies
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string no { get; set; }
        public string contractNo { get; set; }
        public string contractStartDate { get; set; }
        public string contractEndDate { get; set; }
        public string companyName { get; set; }
        public string fax { get; set; }
        public string tel { get; set; }
        public string address { get; set; }

        // public string invoiceDate { get; set; }

    }
}