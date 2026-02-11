using MatchMakingService.Shared;
using MatchMakingService.Domain.Entities;
using MatchMakingService.Domain.Interfaces;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Application.Services;

public class LobbyService(ILobbyRepository lobbyRepository) : ILobbyService
{
    public async Task<Result<Lobby>> CreateLobbyAsync(string lobbyName, int maxPlayers, string creatorConnectionId, string creatorUserName)
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
        return Result<Lobby>.Success(newLobby);
    }

    public async Task<Result<Lobby>> JoinLobbyAsync(string lobbyCode, string playerConnectionId, string playerName)
    {
        var lobby = await lobbyRepository.GetLobbyByCodeAsync(lobbyCode);
        if (lobby == null)
            return Result<Lobby>.Fail($"LobbyCode: {lobbyCode} not found");

        if (lobby.IsFull)
            return Result<Lobby>.Fail($"LobbyCode: {lobbyCode} is full");
        
        if (lobby.HasPlayer(playerConnectionId))
            return Result<Lobby>.Fail($"Player is already in LobbyCode: {lobbyCode}");
        
        lobby.Players.Add(new Player { ConnectionId = playerConnectionId, UserName = playerName });
        lobby.LastActivity = DateTime.UtcNow;
        await lobbyRepository.UpdateLobbyAsync(lobby);
        
        return Result<Lobby>.Success(lobby);
    }

    public async Task LeaveLobbyAsync(string playerConnectionId)
    {   
        var lobby = await lobbyRepository.FindLobbyByPlayerConnectionIdAsync(playerConnectionId);
        if (lobby == null)
            return;
        
        lobby.Players.RemoveAll(x => x.ConnectionId == playerConnectionId);
        lobby.LastActivity = DateTime.UtcNow;
        if (lobby.Players.Count == 0)
        {
            await lobbyRepository.DeleteLobbyAsync(lobby);
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