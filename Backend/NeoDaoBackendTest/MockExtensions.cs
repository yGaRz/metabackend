using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace NeoDaoBackendTest;

public static class MockExtensions {
    public static void Mock<TService>(this IServiceCollection @this, Action<Mock<TService>> customize) where TService : class {
        var serviceType = typeof(TService);
        if (@this.FirstOrDefault(x => x.ServiceType == serviceType) is { } existingServiceDescriptor) {
            @this.Replace(new ServiceDescriptor(serviceType, _ => {
                var mock = new Mock<TService>();
                customize(mock);
                return mock.Object;
            }, existingServiceDescriptor.Lifetime));
            return;
        }

        throw new InvalidOperationException($"'{serviceType}' was not registered in DI Container");
    }
}