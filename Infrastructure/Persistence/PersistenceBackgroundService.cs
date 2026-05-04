namespace MyFirstApi.Infrastructure.Persistence;

public class PersistenceBackgroundService : BackgroundService
{
    private readonly JsonPersistenceService _persistence;
    private readonly ILogger<PersistenceBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public PersistenceBackgroundService(
        JsonPersistenceService persistence,
        ILogger<PersistenceBackgroundService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);

            try
            {
                await _persistence.SaveAsync();
                _logger.LogInformation("[Persistence] Auto-saved at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Persistence] Auto-save failed");
            }
        }
    }
}