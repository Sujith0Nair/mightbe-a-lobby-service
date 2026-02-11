using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MatchMakingService.Hubs;

public class LobbyHub : Hub
{
    private static readonly ConcurrentDictionary<string, Lobby> Lobbies = new();

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"{Context.ConnectionId} disconnected");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers)
    {
        var lobbyId = Guid.NewGuid().ToString()[..6]; // How to do we make sure no overlap happens?
        var newLobby = new Lobby()
        {
            Id = lobbyId,
            Name = lobbyName,
            MaxPlayers = maxPlayers
        };
        
        if (Lobbies.TryAdd(lobbyId, newLobby))
        {
            newLobby.Players.Add(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Caller.SendAsync("LobbyCreated", lobbyId, newLobby);
            Console.WriteLine($"Lobby with name '{lobbyName}' and id '{lobbyId}' is created by '{Context.ConnectionId}'");
        }
        else
        {
            await Clients.Caller.SendAsync("LobbyCreationFailed", "Could not create lobby, please try again!");
        }
    }

    public async Task JoinLobby(string lobbyId)
    {
        if (!Lobbies.TryGetValue(lobbyId, out var lobby))
        {
            await Clients.Caller.SendAsync("LobbyJoinFailed", "Requested lobby could not be found!");
            return;
        }

        if (lobby.Players.Count < lobby.MaxPlayers && !lobby.Players.Contains(Context.ConnectionId))
        {
            lobby.Players.Add(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Caller.SendAsync("JoinedLobby", lobbyId, lobby);
            Console.WriteLine(
                $"Player '{Context.ConnectionId}' is joined lobby with id '{lobbyId}' and name '{lobby.Name}'");
            if (lobby.Players.Count == lobby.MaxPlayers)
            {
                lobby.IsLocked = true;
                await Clients.Group(lobbyId).SendAsync("LobbyLocked", lobby, lobby.MaxPlayers);
                Console.WriteLine($"Lobby with id '{lobbyId}' and name '{lobby.Name}' is locked");
            }
        }
        else if (lobby.Players.Contains(Context.ConnectionId))
        {
            await Clients.Caller.SendAsync("LobbyJoinFailed", "You are already in the lobby!");
        }
        else
        {
            await Clients.Caller.SendAsync("LobbyJoinFailed", "Lobby is full!");
        }
    }

    private class Lobby
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int MaxPlayers { get; init; } = 0;
        public List<string> Players = [];
        public bool IsLocked { get; set; } = false;
    }
}