using MatchMakingService.Domain.Entities;

namespace MatchMakingService.Application.Interfaces;

public interface ILobbyNotifier
{
    public Task NotifyLobbyCreatedAsync(string connectionId, string lobbyCode, string lobbyName);
    public Task NotifyLobbyCreationFailedAsync(string connectionId, string errorMessage);
    public Task NotifyLobbyJoinedAsync(string connectionId, string lobbyCode, string lobbyName, List<Player> currentPlayers);
    public Task NotifyLobbyJoinFailedAsync(string connectionId, string errorMessage);
    public Task NotifyPlayerJoinedAsync(string lobbyCode, string connectionIdOfPlayerJoined, List<Player> currentPlayers);
    public Task NotifyPlayerLeftAsync(string lobbyCode, string connectionIdOfPlayerLeft, List<Player> currentPlayers);
    public Task NotifyLobbyLockedAsync(string lobbyCode, List<Player> currentPlayers);
}