using MatchMakingService.Domain.Interfaces;
using MatchMakingService.Application.Interfaces;

namespace MatchMakingService.BackgroundServices;

public class LobbyCodeRecyclingServices(ILogger<LobbyCodeRecyclingServices> logger, IServiceProvider serviceProvider)
    : IHostedService, IDisposable
{
    private Timer? _timer = null;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting LobbyCodeRecyclingServices");
        _timer = new Timer(Recycle, null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private void Recycle(object? state)
    {
        using var scope = serviceProvider.CreateScope();
        var lobbyCodePool = scope.ServiceProvider.GetRequiredService<ILobbyCodePool>();
        var lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
        ProcessRecycling(lobbyCodePool, lobbyRepository).GetAwaiter().GetResult();
    }

    private async Task ProcessRecycling(ILobbyCodePool lobbyCodePool, ILobbyRepository lobbyRepository)
    {
        const int batchSize = 1000;
        var recyclableCodes = (await lobbyCodePool.RetrieveCodeFromRecyclingBinAsync(batchSize)).ToList();

        var safeToRecycle = new List<string>();
        foreach (var recyclableCode in recyclableCodes)
        {
            if (!await lobbyRepository.DoesLobbyCodeExistAsync(recyclableCode))
            {
                safeToRecycle.Add(recyclableCode);
            }
            else
            {
                logger.LogWarning("Skipping recycling for '{Code}' since the lobby is still active", recyclableCode);
            }
        }

        if (safeToRecycle.Count != 0)
        {
            await lobbyCodePool.ReturnCodeToAvailablePoolAsync(safeToRecycle);
            logger.LogInformation("Successfully recycled {Count} recycling", safeToRecycle.Count);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping LobbyCodeRecyclingServices");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}