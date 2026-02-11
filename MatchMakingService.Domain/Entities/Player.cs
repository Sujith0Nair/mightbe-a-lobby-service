using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatchMakingService.Domain.Entities;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}