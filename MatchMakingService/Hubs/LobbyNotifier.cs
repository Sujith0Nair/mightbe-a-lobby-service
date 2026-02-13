using Microsoft.AspNetCore.SignalR;
using MatchMakingService.Domain.Entities;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Hubs;

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyNotificationClient> hubContext) : ILobbyNotifier
{
    private IGroupManager GroupManager => hubContext.Groups;
    private IHubClients<ILobbyNotificationClient> Clients => hubContext.Clients;
    
    public async Task NotifyLobbyCreatedAsync(string connectionId, string lobbyCode, string lobbyName)
    {
        await GroupManager.AddToGroupAsync(connectionId, lobbyCode);
        await Clients.Client(connectionId).OnLobbyCreated(lobbyCode, lobbyName);
    }

    public async Task NotifyLobbyCreationFailedAsync(string connectionId, string errorMessage)
    {
        await Clients.Client(connectionId).OnLobbyCreationFailed(errorMessage);
    }

    public async Task NotifyLobbyJoinedAsync(string connectionId, string lobbyCode, string lobbyName, List<Player> currentPlayers)
    {
        await GroupManager.AddToGroupAsync(connectionId, lobbyCode);
        await Clients.Client(connectionId).OnLobbyJoined(lobbyCode, lobbyName, currentPlayers);
        await NotifyPlayerJoinedAsync(lobbyCode, connectionId, currentPlayers);
    }

    public async Task NotifyLobbyJoinFailedAsync(string connectionId, string errorMessage)
    {
        await Clients.Client(connectionId).OnLobbyJoinFailed(errorMessage);
    }

    public async Task NotifyPlayerJoinedAsync(string lobbyCode, string connectionIdOfPlayerJoined, List<Player> currentPlayers)
    {
        await Clients.GroupExcept(lobbyCode, connectionIdOfPlayerJoined).OnPlayerJoined(currentPlayers);
    }

    public async Task NotifyPlayerLeftAsync(string lobbyCode, string connectionIdOfPlayerLeft, List<Player> currentPlayers)
    {
        await Clients.Group(lobbyCode).OnPlayerLeft(connectionIdOfPlayerLeft, currentPlayers);
    }

    public async Task NotifyLobbyLockedAsync(string lobbyCode, List<Player> currentPlayers)
    {
        await Clients.Group(lobbyCode).OnLobbyLocked(currentPlayers);
    }

    public async Task NotifyLobbyNotFoundAsync(string connectionId)
    {
        await Clients.Client(connectionId).OnLobbyNotFound();
    }

    public async Task NotifyLobbyNotFoundAsync(string connectionId, string lobbyCode)
    {
        await Clients.Client(connectionId).OnLobbyNotFound(lobbyCode);
    }
}