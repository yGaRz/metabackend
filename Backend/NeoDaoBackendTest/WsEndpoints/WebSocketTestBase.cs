using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL.Responses;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Service;
using NeoDaoBackendTest.Infrastructure;
using System.Net.WebSockets;
using System.Text;

using static NeoDaoBackend.Util.WebSocketUtils;

namespace NeoDaoBackendTest.WsEndpoints;

public abstract class WebSocketTestBase : TestBase
{
    protected readonly static Guid externalSessionId = Guid.NewGuid();
    protected readonly static Guid internalSessionId = Guid.NewGuid();
    protected readonly static Guid userId = Guid.NewGuid();
    protected readonly static string token = "Very-secure-(not)-JWT-token";
    protected readonly static string address = "VVV Leningrad SPB tochka ru";
    protected readonly static string name = "Vasyalisa";
    protected readonly static string profilePic = "whatever";
    protected readonly static int tactileLevel = 12;
    protected readonly static int uniteVerseLevel = 1;
    protected readonly static string userData = $"{{\"data\":{{\"me\":{{\"id\":\"{userId}\",\"address\":\"{address}\",\"name\":\"{name}\"," +
        $"\"profilePic\":\"{profilePic}\",\"tactileLevel\":{tactileLevel},\"uniteVerseLevel\":{uniteVerseLevel}}}}}}}";

    protected string? loadedUserDataMessage;
    protected List<Guid> updatedSessionIds = new List<Guid>();
    protected List<Guid> updatedUserIds = new List<Guid>();

    protected async Task<WebSocket> SetupWebSocketTest(Action<IServiceCollection>? overrideDependencies = null, string? graphQLResponse = null)
    {
        List<string> graphQLResponses = new List<string> { userData };
        if (graphQLResponse != null)
        {
            graphQLResponses.Add(graphQLResponse!);
        }
        SetGraphQLResponse(graphQLResponses);
        List<GetMePayload> userDataToSave = new List<GetMePayload>();
        _appFactory = new CustomWebApplicationFactory(services =>
        {
            services.Replace(ServiceDescriptor.Scoped(_ => _graphQlClient));
            services.Mock<IUserSessionRepository>(mock =>
            {
                mock.Setup(x => x.UpdateUserId(Capture.In(updatedSessionIds), Capture.In(updatedUserIds), It.IsAny<CancellationToken>()));
            });
            services.Mock<IUserDataRepository>(mock =>
            {
                mock.Setup(x => x.GetById(It.Is<Guid>(value => value == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new User
                    {
                        UserId = userId,
                        UserName = name,
                    });
                mock.Setup(x => x.GetByIdOptional(It.Is<Guid>(value => value == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            });

            services.Mock<IUserInventoryRepository>(mock =>
            {
                mock.Setup(x =>
                    x.CreateInventoryNoSave(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            });

            services.Mock<IUserBalanceRepository>(mock =>
            {
                mock.Setup(x =>
                    x.CreateBalanceNoSave(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()));
            });

            services.Mock<AuthorizationService>(mock =>
            {
                mock.Setup(x => x.GetUser(It.IsAny<Guid?>()))
                    .Returns(Task.FromResult(new NeoDaoUser
                    {
                        Token = token,
                        UserId = userId,
                        UserName = name,
                        Role = UserRole.AUTHENTICATED,
                        InternalSessionId = internalSessionId,
                        ExternalSessionId = externalSessionId,
                        Status = SessionStatus.CONFIRMED,
                    }));
            });
            overrideDependencies?.Invoke(services);
        });
        var wsClient = _appFactory.Server.CreateWebSocketClient();
        var wsUri = new UriBuilder(_appFactory.Server.BaseAddress)
        {
            Scheme = "ws",
            Path = "/api/public/connect"
        }.Uri;
        WebSocket ws = await wsClient.ConnectAsync(wsUri, CancellationToken.None);
        loadedUserDataMessage = await ReadMessageFromWebSocket(ws); // Loaded user data; ignore it for now
        return ws;
    }

    protected async Task SendTestMessageToWebSocket(WebSocket ws, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (ws.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes);
            await ws.SendAsync(arraySegment, messageType: WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }
    }
}
