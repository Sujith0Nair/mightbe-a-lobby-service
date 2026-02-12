using Microsoft.AspNetCore.SignalR;
using MatchMakingService.Domain.Entities;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Hubs;

public class LobbyNotifier : ILobbyNotifier
{
    public LobbyNotifier(IHubContext<LobbyHub> hubContext, ILogger<LobbyNotifier> logger)
    {
        
    }
    
    public Task NotifyLobbyCreatedAsync(string connectionId, string lobbyCode, string lobbyName)
    {
        throw new NotImplementedException();
    }

    public Task NotifyLobbyCreationFailedAsync(string connectionId, string errorMessage)
    {
        throw new NotImplementedException();
    }

    public Task NotifyLobbyJoinedAsync(string connectionId, string lobbyCode, string lobbyName, int currentPlayers,
        int maxPlayers)
    {
        throw new NotImplementedException();
    }

    public Task NotifyLobbyJoinFailedAsync(string connectionId, string errorMessage)
    {
        throw new NotImplementedException();
    }

    public Task NotifyPlayerJoinedAsync(string lobbyCode, List<Player> currentPlayers)
    {
        throw new NotImplementedException();
    }

    public Task NotifyPlayerLeftAsync(string lobbyCode, string connectionIdOfPlayerLeft, List<Player> currentPlayers)
    {
        throw new NotImplementedException();
    }

    public Task NotifyLobbyLockedAsync(string lobbyCode, List<Player> currentPlayers)
    {
        throw new NotImplementedException();
    }
}