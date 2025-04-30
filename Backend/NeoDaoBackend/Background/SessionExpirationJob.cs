using NeoDaoBackend.Repository;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Background;

public class SessionExpirationJob : AbstractJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ILogger<SessionExpirationJob> _logger;

    public SessionExpirationJob(IServiceScopeFactory serviceScopeFactory, ILogger<SessionExpirationJob> logger) :
            base(logger, nameof(SessionExpirationJob), SessionExpirationJobWaitingTimeInMs)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
        {
            IUserSessionRepository userSessionRepository = scope.ServiceProvider.GetRequiredService<IUserSessionRepository>();
            var count = await userSessionRepository.RemoveStaleUserSessions(stoppingToken);
            _logger.LogInformation($"Found {count} sessions to remove");
        }
    }
}