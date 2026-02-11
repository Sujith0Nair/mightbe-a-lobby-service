using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MatchMakingService.Domain.Entities;
using MatchMakingService.Domain.Interfaces;
using MatchMakingService.Infrastructure.Data.Settings;

namespace MatchMakingService.Infrastructure.Data.Repositories;

public class MongoDbRepository : ILobbyRepository
{
    private readonly IMongoCollection<Lobby> _lobbiesCollection;

    public MongoDbRepository(IOptions<MongoDbSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _lobbiesCollection = database.GetCollection<Lobby>(dbSettings.Value.CollectionName);
    }

    public async Task CreateLobbyAsync(Lobby lobby) => await _lobbiesCollection.InsertOneAsync(lobby);

    public async Task<Lobby?> GetLobbyByCodeAsync(string lobbyId) => await  _lobbiesCollection.Find(x => x.Id == lobbyId).FirstOrDefaultAsync();

    public async Task UpdateLobbyAsync(Lobby lobby) => await _lobbiesCollection.ReplaceOneAsync(x => x.Id == lobby.Id, lobby);

    public async Task DeleteLobbyAsync(Lobby lobby) => await  _lobbiesCollection.DeleteOneAsync(x => x.Id == lobby.Id);

    public async Task<bool> DoesLobbyCodeExistAsync(string lobbyCode) => await _lobbiesCollection.Find(x => x.Code == lobbyCode).AnyAsync();

    public async Task<Lobby?> FindLobbyByPlayerConnectionIdAsync(string playerConnectionId) => await _lobbiesCollection.Find(x => x.Players.Any(p => p.ConnectionId == playerConnectionId)).FirstOrDefaultAsync();
}