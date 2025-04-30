using NeoDaoBackend.Service;
namespace NeoDaoBackend.Background;
using static NeoDaoBackend.Models.Constants;

public class StreamJobService : AbstractJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public StreamJobService(IServiceScopeFactory serviceScopeFactory, ILogger<СalculatingAuctionJob> logger)
        : base(logger, nameof(StreamJobService), StreamingJobWaitingTimeInMs)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var streamingService = scope.ServiceProvider.GetRequiredService<StreamingService>();
                await streamingService.UpdateStreamTimesAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{name} encountered error during execution.", _className);
            throw;
        }
    }
}