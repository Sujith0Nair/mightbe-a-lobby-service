using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatchMakingService.Domain.Entities;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}