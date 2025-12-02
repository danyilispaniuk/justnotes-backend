using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class Model
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    [BsonRepresentation(BsonType.DateTime)]
    public required DateTime Created { get; set; }

    [BsonRepresentation(BsonType.DateTime)]
    public required DateTime Updated { get; set; }

    [BsonRepresentation(BsonType.DateTime)]
    public DateTime? Deleted { get; set; }
}
