using NeoDaoBackend.Service;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Background;

public class NotificationJob : AbstractJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public NotificationJob(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationJob> logger) : base(logger, nameof(NotificationJob), NotificationExpirationJobWaitingTimeInMs)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
            await notificationService.NotificationCycle(stoppingToken);
        }
    }

    protected override async Task ExecuteOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            await DoWorkAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{name} has been stopped.", _className);
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{name} encountered error", _className);
        }
    }
}
