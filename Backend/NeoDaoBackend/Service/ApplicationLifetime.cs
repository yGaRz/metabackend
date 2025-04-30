using NeoDaoBackend.Repository;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Service;

class ApplicationLifetime : IHostedService
{
    private readonly ILogger<ApplicationLifetime> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ApplicationLifetime( ILogger<ApplicationLifetime> logger, 
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
        _hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        _hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private async void OnStarted()
    {
        _logger.LogInformation("Application starting");
        //Добавил, чтобы при перезапуске бека, не получить правки на группах.
        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
        {
            IGroupRepository groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
            await groupRepository.DeleteAllGroups(CancellationToken.None);
        }
    }

    private void OnStopping()
    {
        _logger.LogInformation("Graceful shutdown starting");
    }

    private void OnStopped()
    {
        _logger.LogInformation("Graceful shutdown completed");
    }
}