using GraphQL;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL.Responses;
using NeoDaoBackendTest.Infrastructure;
using Newtonsoft.Json;
using System.Text;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackendTest.Controllers;

public class UserInfoTest(WebApplicationFactoryWithDatabase factory) : TestBaseWithDatabase(factory)
{
    private static readonly string token = "Very-secure-(not)-JWT-token";
    private static readonly string address = "VVV Leningrad SPB tochka ru";
    private static readonly string name = "Vasyalisa";
    private static readonly string profilePic = "whatever";
    private static readonly int tactileLevel = 12;
    private static readonly int uniteVerseLevel = 1;
    private static readonly AvatarGender gender = AvatarGender.FEMALE;
    private static readonly string avatarSettings = $"{{\"gender\":\"{gender}\"}}";

    [Fact]
    public async Task SaveAvatarSettingsHappyPath() {
        (Guid userId, Guid externalSessionId) = _testDb.CreateAuthenticatedUser();

        HttpClient client = _webApplicationFactory.CreateClient();

        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());

        HttpContent content = new StringContent(avatarSettings, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/saveAvatarSettings", content);

        response.EnsureSuccessStatusCode();

        UserAvatar? userAvatar = _testDb.GetUserAvatar(userId);
        Assert.NotNull(userAvatar);
        Assert.Equal(gender, userAvatar.Gender);
    }

    [Fact]
    public async Task GetAvatarSettingsHappyPath() {
        (Guid userId, Guid externalSessionId) = _testDb.CreateAuthenticatedUser();
        _testDb.CreateUserAvatar(userId, gender);

        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());

        var response = await client.GetAsync("/api/public/getAvatarSettings");
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(avatarSettings, responseContent);
    }

    private static string GenerateMeBridgeRequest(Guid userId)
    {
        GraphQLResponse<GetMeResponse> getMeResponse = new()
        {
            Data = new GetMeResponse
            {
                Me = GetMePayload(userId)
            }
        };
        return JsonConvert.SerializeObject(getMeResponse);
    }

    private static GetMePayload GetMePayload(Guid userId)
    {
        return new GetMePayload
        {
            Id = userId,
            Name = name,
            Address = address,
            ProfilePic = profilePic,
            TactileLevel = tactileLevel,
            UniteVerseLevel = uniteVerseLevel
        };
    }
}
