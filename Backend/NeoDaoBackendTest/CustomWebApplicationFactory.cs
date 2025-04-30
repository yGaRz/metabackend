using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace NeoDaoBackendTest;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
    private readonly Action<IServiceCollection>? _overrideDependencies;

    public CustomWebApplicationFactory(Action<IServiceCollection>? overrideDependencies = null) {
        _overrideDependencies = overrideDependencies;
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("SELF_ADDRESS", "https://localhost:80/whatever");
        Environment.SetEnvironmentVariable("APP_DEEPLINK", "app://whatever");
        Environment.SetEnvironmentVariable("OM_FRONTEND_ADDRESS", "https://whatever:-1");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureServices(services => _overrideDependencies?.Invoke(services));
    }
}
