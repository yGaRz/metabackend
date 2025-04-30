using NeoDaoBackend.Models.graphQL.Responses;
using NeoDaoBackend.Models.UserData;
using NeoDaoBackendTest.Models;
using Newtonsoft.Json;

namespace NeoDaoBackendTest.WsEndpoints;

public class UserDataTest : WebSocketTestBase
{
    [Fact]
    public async Task LoadUserDataHappyPath()
    {
        await SetupWebSocketTest();

        MessageFromServer<GetMePayload, UserDataEndpointKind>? response = JsonConvert.DeserializeObject<MessageFromServer<GetMePayload, UserDataEndpointKind>>(loadedUserDataMessage!);
        Assert.NotNull(response);
        Assert.Equal(UserDataEndpointKind.UserInfo, response.EventType);
        GetMePayload expectedSavedUserData = new GetMePayload
        {
            Address = address,
            Id = userId,
            Name = name,
            ProfilePic = profilePic,
            TactileLevel = tactileLevel,
            UniteVerseLevel = uniteVerseLevel,
        };
        Assert.Equal(internalSessionId, updatedSessionIds.Single());
        Assert.Equal(userId, updatedUserIds.Single());
        Assert.Equal(expectedSavedUserData.Id, response.Data!.Id);
        Assert.Equal(expectedSavedUserData.Name, response.Data!.Name);
        Assert.Equal(expectedSavedUserData.Address, response.Data!.Address);
        Assert.Equal(expectedSavedUserData.ProfilePic, response.Data!.ProfilePic);
        Assert.Equal(expectedSavedUserData.TactileLevel, response.Data!.TactileLevel);
        Assert.Equal(expectedSavedUserData.UniteVerseLevel, response.Data!.UniteVerseLevel);
    }
}
