using MatchMakingService.Shared;

namespace MatchMakingService.Application.Interfaces;

public interface ILobbyCodePool
{
    public Task<Result<string>> GetCodeAsync();
    public Task AddCodeToRecyclingBinAsync(string lobbyCode);
    public Task<IEnumerable<string>> RetrieveCodeFromRecyclingBinAsync(int batchSize);
    public Task ReturnCodeToAvailablePoolAsync(IEnumerable<string> codes);
}