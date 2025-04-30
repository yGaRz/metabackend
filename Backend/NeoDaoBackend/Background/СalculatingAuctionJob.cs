using NeoDaoBackend.Service;
using static NeoDaoBackend.Models.Constants;
namespace NeoDaoBackend.Background;

public class СalculatingAuctionJob : AbstractJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public СalculatingAuctionJob(IServiceScopeFactory serviceScopeFactory, ILogger<СalculatingAuctionJob> logger)
        : base(logger, nameof(СalculatingAuctionJob), jobWaitingTimeMs: CalculatingJobWaitingTimeInMs)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        try        
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var nftAuctionService = scope.ServiceProvider.GetRequiredService<AuctionService>();
                await nftAuctionService.СalculatingAuctionLots(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{name} encountered error during execution.", _className);
            throw;
        }
    }
}