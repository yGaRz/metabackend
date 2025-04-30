namespace NeoDaoBackend.Background;

public abstract class AbstractJob : BackgroundService
{
    protected readonly string _className;
    protected readonly int _jobWaitingTimeMs;
    protected readonly ILogger _logger;

    public AbstractJob(ILogger logger, string className, int jobWaitingTimeMs)
    {
        _className = className;
        _logger = logger;
        _jobWaitingTimeMs = jobWaitingTimeMs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(_jobWaitingTimeMs));
        await ExecuteOnceAsync(stoppingToken);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteOnceAsync(stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }

    protected virtual async Task ExecuteOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("{name} is running.", _className);
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
        _logger.LogInformation("{name} successfully executed.", _className);
    }

    protected abstract Task DoWorkAsync(CancellationToken stoppingToken);
}
