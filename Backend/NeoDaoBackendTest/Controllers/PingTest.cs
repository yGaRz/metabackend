using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NeoDaoBackendTest.Infrastructure;

namespace NeoDaoBackendTest.Controllers;

public class PingTest : TestBase {
    [Fact]
    public async Task GetPingResponse() {
        StubGraphQLClient();
        _appFactory = new CustomWebApplicationFactory(services => {
            services.Replace(ServiceDescriptor.Scoped(_ => _graphQlClient));
        });
        var client = _appFactory.CreateClient();
        var response = await client.GetAsync("/api/public/ping");

        response.EnsureSuccessStatusCode();
        Assert.Equal("O Captain! My Captain!",
            await response.Content.ReadAsStringAsync());
    }
}
