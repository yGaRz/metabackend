using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation;
using NeoDaoBackendTest.Infrastructure;
using System.Net;
using System.Text;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackendTest.Controllers;

public class AuthorizationTest(WebApplicationFactoryWithDatabase factory) : TestBaseWithDatabase(factory)
{
    private static readonly string _code = "611228";
    private static readonly string _token = "Very-Secret-(not)-JWT-Token";
    private static readonly string _serverSecret = "Top-secret";
    private static readonly string _expiredAt = "2024-04-19T08:36:45.198+00:00";

    [Fact]
    public async Task CreateSessionRequestHappyPath()
    {
        //TODO: разобраться в изменениях в авторизации.
        Guid externalSessionId = Guid.NewGuid();
        string sessionUrl = GenerateSessionUrl(externalSessionId);
        string expectedCreateSessionResponse = GenerateExpectedCreateSessionResponse(externalSessionId, sessionUrl, _code, _expiredAt);
        string bridgeCreateSessionResponse = GenerateBridgeCreateSessionResponse(externalSessionId, _expiredAt, sessionUrl, _code);
        SetGraphQLResponse(new List<string> { bridgeCreateSessionResponse });
        HttpClient client = ConfigureClientWithGraphQL();
        client.DefaultRequestHeaders.Add(PlatformHeader, PlatformMobileKey);
        var response = await client.PostAsync("/api/public/createSessionRequest", null);
        var stringResponse = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Equal(expectedCreateSessionResponse, stringResponse);

        Session actualSession = _testDb.GetSession(externalSessionId)!;
        Assert.Equal(SessionStatus.CREATED, actualSession.Status);
        Assert.Null(actualSession.Token);
    }

    [Fact]
    public async Task CreateSessionRequestNoPlatformFail()
    {
        HttpClient client = _webApplicationFactory.CreateClient();
        var response = await client.PostAsync("/api/public/createSessionRequest", null);
        var stringResponse = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertErrorCodeFromResponse(ErrorCode.PlatformIsMissing, stringResponse);
    }

    [Theory]
    [InlineData(SessionStatus.CREATED)]
    [InlineData(SessionStatus.CONFIRMED)]
    public async Task GetSessionStatusHappyPath(SessionStatus sessionStatus)
    {
        Guid externalSessionId = _testDb.CreateUserSession(sessionStatus);

        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());
        var response = await client.GetAsync("/api/public/getSessionStatus");
        var stringResponse = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Equal($"{{\"status\":\"{sessionStatus}\"}}", stringResponse);
    }

    [Fact]
    public async Task GetRemovedSessionStatusFail()
    {
        Guid externalSessionId = _testDb.CreateUserSession(SessionStatus.REMOVED);

        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());
        var response = await client.GetAsync("/api/public/getSessionStatus");
        var stringResponse = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(SessionStatus.CREATED)]
    [InlineData(SessionStatus.CONFIRMED)]
    public async Task ConfirmSessionHappyPath(SessionStatus sessionStatus)
    {
        Guid externalSessionId = _testDb.CreateUserSession(sessionStatus);

        var confirmSessionPayload = GenerateConfirmSessionPayload(externalSessionId, _token);
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());
        client.DefaultRequestHeaders.Add(ServerSecretHeader, _serverSecret);
        HttpContent content = new StringContent(confirmSessionPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/confirmSession", content);

        response.EnsureSuccessStatusCode();

        _testDb.ClearEFChanges();
        Session actualSession = _testDb.GetSession(externalSessionId)!;
        Assert.Equal(SessionStatus.CONFIRMED, actualSession.Status);
    }

    [Fact]
    public async Task ConfirmSessionInconsistentPayloadFail()
    {
        Guid externalSessionId = _testDb.CreateUserSession(SessionStatus.CREATED);

        var confirmSessionPayload = GenerateConfirmSessionPayload(externalSessionId, null);
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());
        client.DefaultRequestHeaders.Add(ServerSecretHeader, _serverSecret);
        HttpContent content = new StringContent(confirmSessionPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/confirmSession", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertErrorCodeFromResponse(ErrorCode.ConfirmSessionInconsistentPayload, await response.Content.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task ConfirmRemovedSessionFail()
    {
        Guid externalSessionId = _testDb.CreateUserSession(SessionStatus.REMOVED);

        var confirmSessionPayload = GenerateConfirmSessionPayload(externalSessionId, _token);
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());
        client.DefaultRequestHeaders.Add(ServerSecretHeader, _serverSecret);
        HttpContent content = new StringContent(confirmSessionPayload, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("/api/public/confirmSession", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertErrorCodeFromResponse(ErrorCode.UnknownSession, await response.Content.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task UnauthorizedConfirmSessionFail()
    {
        Guid externalSessionId = _testDb.CreateUserSession(SessionStatus.CREATED);

        var confirmSessionPayload = GenerateConfirmSessionPayload(externalSessionId, _token);
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(SessionIdHeader, externalSessionId.ToString());
        HttpContent content = new StringContent(confirmSessionPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/confirmSession", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    #region Private Methods
    private static string GenerateSessionUrl(Guid sessionId) =>
        $"https://sso.proksy.io/confirmSession/{sessionId}";

    private string GenerateBridgeCreateSessionResponse(
        Guid externalSessionId,
        string expiredAt,
        string sessionUrl,
        string code)
    {
        var response = new
        {
            data = new
            {
                addUserSessionRequest = new
                {
                    userSessionRequest = new
                    {
                        id = externalSessionId,
                        createdAt = "2024-04-19T08:06:45.198Z",
                        expiredAt,
                        status = "CREATED",
                        sessionUrl,
                        code
                    }
                }
            }
        };
        return Serialize(response);
    }
    
    private string GenerateExpectedCreateSessionResponse(
       Guid externalSessionId,
       string sessionUrl,
       string code,
       string expiredAt)
    {
        var response = new
        {
            id = externalSessionId,
            sessionUrl,
            code,
            expiredAt
        };
        return Serialize(response);
    }
    
    private string GenerateConfirmSessionPayload(Guid externalSessionId, string? token)
    {
        WebhookPayload payload = new()
        {
            SessionRequestId = externalSessionId,
            Success = true,
            Token = token
        };
        return Serialize(payload);
    }
    #endregion
}
