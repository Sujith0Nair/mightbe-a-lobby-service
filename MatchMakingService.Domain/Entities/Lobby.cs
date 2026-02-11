using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatchMakingService.Domain.Entities;

public class Lobby
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public List<Player> Players { get; set; } = [];
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public bool IsFull => Players.Count >= MaxPlayers;
    public bool HasPlayer (string connectionId) => Players.Any(p => p.Id == connectionId);
    public Player? GetPlayerByConnectionId (string connectionId) => Players.FirstOrDefault(p => p.Id == connectionId);
}