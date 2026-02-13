using MatchMakingService.Domain.Entities;

namespace MatchMakingService.Application.Interfaces;

public interface ILobbyNotificationClient
{
    public Task OnLobbyCreated(string lobbyCode, string lobbyName);
    public Task OnLobbyCreationFailed(string errorMessage);
    public Task OnLobbyJoined(string lobbyCode, string lobbyName, List<Player> currentPlayers);
    public Task OnLobbyJoinFailed(string errorMessage);
    public Task OnPlayerJoined(List<Player> currentPlayers);
    public Task OnPlayerLeft(string connectionIdOfPlayerLeft, List<Player> currentPlayers);
    public Task OnLobbyLocked(List<Player> currentPlayers);
    public Task OnLobbyNotFound();
    public Task OnLobbyNotFound(string lobbyCode);
}