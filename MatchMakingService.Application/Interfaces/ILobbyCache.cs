using MatchMakingService.Shared;
using MatchMakingService.Domain.Entities;

namespace MatchMakingService.Application.Interfaces;

public interface ILobbyCache
{
    public Task<Result<Lobby>> GetLobbyAsync(string lobbyCode);
    public Task SetLobbyAsync(string lobbyCode, Lobby lobby);
    public Task RemoveLobbyAsync(string lobbyCode);
}