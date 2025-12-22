using VotingService.Repositories;

namespace VotingService.Services;

public class VotingExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VotingExpirationService> _logger;

    public VotingExpirationService(IServiceProvider serviceProvider, ILogger<VotingExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба завершения голосований запущена.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireVotings(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке завершения голосований.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }

        _logger.LogInformation("Служба завершения голосований остановлена.");
    }

    private async Task CheckAndExpireVotings(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IVotingRepository>();

        var expiredVotings = await repository.GetExpiredVotingsAsync();

        if (expiredVotings.Any())
        {
            foreach (var voting in expiredVotings)
            {
                voting.IsCompleted = true;
                _logger.LogInformation("Голосование {VotingId} завершено по времени.", voting.Id);
            }

            await repository.SaveChangesAsync();
        }
    }
}