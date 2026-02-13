using System.Text.Json;
using MatchMakingService.Shared;
using MatchMakingService.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Infrastructure.Caching;

public class LobbyCache(IDistributedCache cache) : ILobbyCache
{
    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(1)
    };

    public async Task<Result<Lobby>> GetLobbyAsync(string lobbyCode)
    {
        var cacheLobbyInfo = await cache.GetStringAsync(lobbyCode);
        if (string.IsNullOrEmpty(cacheLobbyInfo))
        {
            return Result<Lobby>.Fail("Lobby not found");
        }
        
        var lobby = JsonSerializer.Deserialize<Lobby>(cacheLobbyInfo);
        return lobby == null ? Result<Lobby>.Fail("Lobby not found") : Result<Lobby>.Success(lobby);
    }

    public async Task SetLobbyAsync(string lobbyCode, Lobby lobby)
    {
        var json = JsonSerializer.Serialize(lobby);
        await cache.SetStringAsync(lobbyCode, json, _cacheOptions);
    }

    public async Task RemoveLobbyAsync(string lobbyCode)
    {
        await cache.RemoveAsync(lobbyCode);
    }
}