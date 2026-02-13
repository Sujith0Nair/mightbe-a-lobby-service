using MatchMakingService.Shared;
using MatchMakingService.Domain.Entities;

namespace MatchMakingService.Application.Interfaces;

public interface ILobbyService
{
    public Task<Result<Lobby>> CreateLobbyAsync(string lobbyName, int maxPlayers, string creatorConnectionId, string creatorUserName);
    public Task<Result<Lobby>> JoinLobbyAsync(string lobbyCode, string playerConnectionId, string playerName);
    public Task<Result> LeaveLobbyAsync(string playerConnectionId);
    public Task<Result> LeaveLobbyAsync(string lobbyCode, string playerConnectionId);
}