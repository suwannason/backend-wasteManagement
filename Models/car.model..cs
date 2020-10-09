
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class Cars
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]

    public string _id { get; set; }

    public string carType { get; set; }
}