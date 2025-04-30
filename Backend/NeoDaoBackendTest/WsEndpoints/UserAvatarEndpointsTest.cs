using Moq;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserData;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository;
using NeoDaoBackendTest.Models;
using Newtonsoft.Json;
using System.Net.WebSockets;
using static NeoDaoBackend.Util.WebSocketUtils;

namespace NeoDaoBackendTest.WsEndpoints;

public class UserAvatarEndpointsTest : WebSocketTestBase
{
    private static AvatarGender gender = AvatarGender.FEMALE;

    [Fact]
    public async Task GetAvatarSettingsHappyPath()
    {
        WebSocket ws = await SetupWebSocketTest(services =>
        {
            services.Mock<IUserAvatarRepository>(mock =>
            {
                mock.Setup(x => x.GetByUserId(It.Is<Guid>(value => value == userId), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new UserAvatar
                    {
                        Gender = gender,
                    }) as Task<UserAvatar?>);
            });
        });

        string request = $"{{\"eventCategory\":\"{EndpointCategory.AvatarSettings}\",\"eventType\":\"{UserDataEndpointKind.GetAvatarSettings}\"}}";
        await SendTestMessageToWebSocket(ws, request);

        string? rawResponse = await ReadMessageFromWebSocket(ws);
        MessageFromServer<GetAvatarSettingsData, UserDataEndpointKind>? response
            = JsonConvert.DeserializeObject<MessageFromServer<GetAvatarSettingsData, UserDataEndpointKind>>(rawResponse!);
        Assert.NotNull(response);
        Assert.Equal(UserDataEndpointKind.GetAvatarSettings, response.EventType);
        Assert.Equal(gender, response.Data!.Gender);
    }

    [Fact]
    public async Task SaveAvatarSettingsHappyPath()
    {
        List<AvatarGender> actualGenders = new List<AvatarGender>();
        WebSocket ws = await SetupWebSocketTest(services =>
        {
            services.Mock<IUserAvatarRepository>(mock =>
            {
                mock.Setup(x => x.Create(It.Is<Guid>(value => value == userId), Capture.In(actualGenders), It.IsAny<CancellationToken>()));
            });
        });

        string request = $"{{\"eventCategory\":\"{EndpointCategory.AvatarSettings}\",\"eventType\":\"{UserDataEndpointKind.SaveAvatarSettings}\"," +
            $"\"data\":{{\"gender\":\"{gender}\"}}}}";
        await SendTestMessageToWebSocket(ws, request);

        string? rawResponse = await ReadMessageFromWebSocket(ws);
        MessageFromServer<SaveAvatarSettingsData, UserDataEndpointKind>? response
            = JsonConvert.DeserializeObject<MessageFromServer<SaveAvatarSettingsData, UserDataEndpointKind>>(rawResponse!);
        Assert.NotNull(response);
        Assert.Equal(UserDataEndpointKind.SaveAvatarSettings, response.EventType);
        Assert.Equal(gender, response.Data!.Gender);
        Assert.Equal(gender, actualGenders.Single());
    }
}