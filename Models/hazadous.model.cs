using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using backend.request;
namespace backend.Models
{
    public class HazadousSchema {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string date { get; set; }
    public string time { get; set; }
    public string dept { get; set; }
    public string div { get; set; }
    public string phase { get; set; }
    public string wasteName { get; set; }
    public string wasteGroup { get; set; }
    public string contractorCompany { get; set; }
    public string binddingType { get; set; }
    public string cptMainType { get; set; }
    public string wasteType { get; set; }
    public string boiType { get; set; }
    public string partNormalType  { get; set; }
    public string productType { get; set; }
    public string lotNo { get; set; }
    public string companyApproveNo { get; set; }
    public string containerType { get; set; }
    public string qtyOfContainer { get; set; }
    public string weightPerContainer { get; set; }
    public string totalWeight { get; set; }

    // APPROVING STEP
    public Profile req_prepared { get; set; }
    public Profile req_checked { get; set; }
    public Profile req_approved { get; set; }
    public Profile fae_prepared { get; set; }
    public Profile fae_checked { get; set; }
    public Profile fae_approved { get; set; }
    // APPROVING STEP

    public string status { get; set; }
    public string year { get; set; }
    public string record { get; set; }
    }
}