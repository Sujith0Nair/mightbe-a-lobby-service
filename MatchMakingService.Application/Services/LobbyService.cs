using MatchMakingService.Shared;
using MatchMakingService.Domain.Entities;
using MatchMakingService.Domain.Interfaces;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Application.Services;

public class LobbyService(ILobbyRepository lobbyRepository, ILobbyNotifier lobbyNotifier) : ILobbyService
{
    // Due to internet speed differences on client side - when the last player joins, we let the lobby join message to have reached
    // and processed - post which we move ahead with the room full and lock message. Since, order in this use case is important.
    // One more way to do this, is by receiving an event back from client that the message has been received - but up for debate.
    private const int CustomDelayOnMatchStartEvent = 2000;
    
    public async Task<Result<Lobby>> CreateLobbyAsync(string lobbyName, int maxPlayers, string creatorConnectionId, string creatorUserName)
    {
        try
        {
            var lobbyCode = GenerateUniqueLobbyCode();
            while (await lobbyRepository.DoesLobbyCodeExistAsync(lobbyCode))
            {
                lobbyCode = GenerateUniqueLobbyCode();
            }

            var creator = new Player { ConnectionId = creatorConnectionId, UserName = creatorUserName };
            var newLobby = new Lobby
            {
                Code = lobbyCode,
                Name = lobbyName,
                MaxPlayers = maxPlayers,
                Players = [creator]
            };

            await lobbyRepository.CreateLobbyAsync(newLobby);
            await lobbyNotifier.NotifyLobbyCreatedAsync(creatorConnectionId, lobbyCode, lobbyName);
            
            return Result<Lobby>.Success(newLobby);
        }
        catch (Exception exception) // Specific exceptions would be great!
        {
            await lobbyNotifier.NotifyLobbyCreationFailedAsync(creatorConnectionId, exception.Message);
            return Result<Lobby>.Fail(exception.Message);
        }
    }

    public async Task<Result<Lobby>> JoinLobbyAsync(string lobbyCode, string playerConnectionId, string playerName)
    {
        try
        {
            var lobby = await lobbyRepository.GetLobbyByCodeAsync(lobbyCode);
            if (lobby == null)
            {
                var errMessage = $"LobbyCode: {lobbyCode} not found";
                await lobbyNotifier.NotifyLobbyJoinFailedAsync(playerConnectionId, errMessage);
                return Result<Lobby>.Fail(errMessage);
            }

            if (lobby.IsFull)
            {
                var errMessage = $"LobbyCode: {lobbyCode} is full";
                await lobbyNotifier.NotifyLobbyJoinFailedAsync(playerConnectionId, errMessage);
                return Result<Lobby>.Fail(errMessage);
            }

            if (lobby.HasPlayer(playerConnectionId))
            {
                var errMessage = $"LobbyCode: {lobbyCode} has already been joined";
                await lobbyNotifier.NotifyLobbyJoinFailedAsync(playerConnectionId, errMessage);
                return Result<Lobby>.Fail(errMessage);
            }
        
            lobby.Players.Add(new Player { ConnectionId = playerConnectionId, UserName = playerName });
            lobby.LastActivity = DateTime.UtcNow;
            await lobbyRepository.UpdateLobbyAsync(lobby);
            await lobbyNotifier.NotifyLobbyJoinedAsync(playerConnectionId, lobbyCode, lobby.Name, lobby.Players);
            
            await Task.Delay(CustomDelayOnMatchStartEvent);

            if (lobby.IsFull)
            {
                await lobbyNotifier.NotifyLobbyLockedAsync(lobbyCode, lobby.Players);
            }
        
            return Result<Lobby>.Success(lobby);
        }
        catch (Exception exception)
        {
            return Result<Lobby>.Fail(exception.Message);
        }
    }

    public async Task LeaveLobbyAsync(string playerConnectionId)
    {   
        var lobby = await lobbyRepository.FindLobbyByPlayerConnectionIdAsync(playerConnectionId);
        if (lobby == null)
        {
            return;
        }
        
        lobby.Players.RemoveAll(x => x.ConnectionId == playerConnectionId);
        lobby.LastActivity = DateTime.UtcNow;
        if (lobby.Players.Count == 0)
        {
            await lobbyRepository.DeleteLobbyAsync(lobby);
            // We can voluntarily disconnect connection?
        }
        else
        {
            await lobbyRepository.UpdateLobbyAsync(lobby);
        }
    }

    public async Task<Result<Lobby>> GetLobbyByCodeAsync(string lobbyCode)
    {
        var lobby = await lobbyRepository.GetLobbyByCodeAsync(lobbyCode);
        return lobby == null ? 
            Result<Lobby>.Fail($"LobbyCode: {lobbyCode} not found") : 
            Result<Lobby>.Success(lobby);
    }

    private static string GenerateUniqueLobbyCode()
    {
        return Guid.NewGuid().ToString("N")[..6].ToUpper();
    }
}