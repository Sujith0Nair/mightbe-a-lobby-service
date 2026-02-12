using Microsoft.AspNetCore.SignalR;
using MatchMakingService.Domain.Entities;
using MatchMakingService.Application.Constants;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Hubs;

public class LobbyNotifier(IHubContext<LobbyHub> hubContext, ILogger<LobbyNotifier> logger)
    : ILobbyNotifier
{
    private IGroupManager GroupManager => hubContext.Groups;
    private IHubClients Clients => hubContext.Clients;
    
    public async Task NotifyLobbyCreatedAsync(string connectionId, string lobbyCode, string lobbyName)
    {
        await GroupManager.AddToGroupAsync(connectionId, lobbyCode);
        await Clients.Client(connectionId).SendAsync(SignalRConstants.OnLobbyCreated, lobbyCode, lobbyName);
    }

    public async Task NotifyLobbyCreationFailedAsync(string connectionId, string errorMessage)
    {
        await Clients.Client(connectionId).SendAsync(SignalRConstants.OnLobbyCreationFailed, errorMessage);
    }

    public async Task NotifyLobbyJoinedAsync(string connectionId, string lobbyCode, string lobbyName, List<Player> currentPlayers)
    {
        await GroupManager.AddToGroupAsync(connectionId, lobbyCode);
        await Clients.Client(connectionId).SendAsync(SignalRConstants.OnLobbyJoined, lobbyCode, lobbyName, currentPlayers);
        await NotifyPlayerJoinedAsync(lobbyCode, connectionId, currentPlayers);
    }

    public async Task NotifyLobbyJoinFailedAsync(string connectionId, string errorMessage)
    {
        await Clients.Client(connectionId).SendAsync(SignalRConstants.OnLobbyJoinFailed, errorMessage);
    }

    public async Task NotifyPlayerJoinedAsync(string lobbyCode, string connectionIdOfPlayerJoined, List<Player> currentPlayers)
    {
        await Clients.GroupExcept(lobbyCode, connectionIdOfPlayerJoined).SendAsync(SignalRConstants.OnPlayerJoined, currentPlayers);
    }

    public async Task NotifyPlayerLeftAsync(string lobbyCode, string connectionIdOfPlayerLeft, List<Player> currentPlayers)
    {
        throw new NotImplementedException();
    }

    public async Task NotifyLobbyLockedAsync(string lobbyCode, List<Player> currentPlayers)
    {
        await Clients.Group(lobbyCode).SendAsync(SignalRConstants.OnLobbyLocked, currentPlayers);
    }
}