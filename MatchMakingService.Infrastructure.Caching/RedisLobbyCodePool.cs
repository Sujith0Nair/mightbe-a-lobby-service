using StackExchange.Redis;
using MatchMakingService.Shared;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.Infrastructure.Caching;

public class RedisLobbyCodePool(IConnectionMultiplexer connectionMultiplexer) : ILobbyCodePool
{
    private readonly IDatabase _redisDatabase = connectionMultiplexer.GetDatabase(2);
    private const string AvailableCodesKey = "available_lobby_codes";
    private const string RecyclableCodesKey = "recyclable_lobby_codes";

    public async Task<Result<string>> GetCodeAsync()
    {
        var code = await _redisDatabase.SetPopAsync(AvailableCodesKey);
        return code.IsNullOrEmpty ? 
            Result<string>.Fail($"CRITICAL: '{AvailableCodesKey}' was not found. Run the population script first!") : 
            Result<string>.Success(code.ToString());
    }

    public async Task AddCodeToRecyclingBinAsync(string lobbyCode)
    {
        if (string.IsNullOrEmpty(lobbyCode) || string.IsNullOrWhiteSpace(lobbyCode)) return;
        await _redisDatabase.SetAddAsync(RecyclableCodesKey, lobbyCode);
    }

    public async Task<IEnumerable<string>> RetrieveCodeFromRecyclingBinAsync(int batchSize)
    {
        var codes = await _redisDatabase.SetPopAsync(RecyclableCodesKey, batchSize);
        return codes.Select(code => code.ToString());
    }

    public async Task ReturnCodeToAvailablePoolAsync(IEnumerable<string> codes)
    {
        var redisValues = codes.Select(code => new RedisValue(code)).ToArray();
        if (redisValues.Length == 0) return;
        await _redisDatabase.SetAddAsync(AvailableCodesKey, redisValues);
    }
}