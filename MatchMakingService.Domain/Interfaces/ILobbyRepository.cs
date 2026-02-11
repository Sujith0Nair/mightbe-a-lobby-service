using MatchMakingService.Domain.Entities;

namespace MatchMakingService.Domain.Interfaces;

public interface ILobbyRepository
{
    public Task CreateLobbyAsync(Lobby lobby);
    public Task<Lobby?> GetLobbyByCodeAsync(string lobbyId);
    public Task UpdateLobbyAsync(Lobby lobby);
    public Task DeleteLobbyAsync(Lobby lobby);
    public Task<bool> DoesLobbyCodeExistAsync(string lobbyCode);
    public Task<Lobby?> FindLobbyByPlayerConnectionIdAsync(string playerConnectionId);
}