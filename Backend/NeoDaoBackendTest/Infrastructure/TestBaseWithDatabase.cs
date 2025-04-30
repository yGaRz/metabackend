using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Moq.Protected;
using NeoDaoBackend.Models;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NeoDaoBackendTest.Infrastructure;

public abstract class TestBaseWithDatabase : IClassFixture<WebApplicationFactoryWithDatabase>, IDisposable
{
    protected static string serverSecret = "Top-secret";
    protected static string ErrorResponsePattern = @"\[\{""(.*)"":"".*""\}\]";
    protected const string JsonMediaType = "application/json";

    protected static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new OrderedContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatString = "yyyy-MM-dd HH:mm:ss.fffK"
    };

    protected GraphQLHttpClient _graphQLClient = null!;

    protected readonly IServiceScope _serviceScope;
    protected readonly NeoDaoDbContext _dbContext;
    protected readonly TestDb _testDb;
    protected readonly WebApplicationFactoryWithDatabase _webApplicationFactory;

    protected TestBaseWithDatabase(WebApplicationFactoryWithDatabase factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("SELF_ADDRESS", "https://localhost:80/whatever");
        Environment.SetEnvironmentVariable("APP_DEEPLINK", "app://whatever");
        Environment.SetEnvironmentVariable("OM_FRONTEND_ADDRESS", "https://whatever:-1");
        Environment.SetEnvironmentVariable("SERVER_SECRET", serverSecret);

        _webApplicationFactory = factory ?? throw new ArgumentNullException(nameof(factory));

        _serviceScope =
            factory
            .Services
            .CreateScope() ??
            throw new InvalidOperationException(nameof(_serviceScope));

        _dbContext =
            _serviceScope
            .ServiceProvider
            .GetRequiredService<NeoDaoDbContext>() ??
            throw new InvalidOperationException(nameof(_dbContext));

        _testDb = new TestDb(_dbContext);
    }

    protected void StubGraphQLClient()
    {
        SetGraphQLResponse(new List<string>());
    }

    protected void SetGraphQLResponse(List<string> responseContents)
    {
        HttpMessageHandler mockedHttpMessageHandler = GetMockedHttpMessageHandler(responseContents);
        HttpClient client = new HttpClient(mockedHttpMessageHandler);
        client.BaseAddress = new Uri("https://chainbridge.io/api/v1/graphlq");
        GraphQLHttpClientOptions options = new GraphQLHttpClientOptions();
        _graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer(), client);
    }

    protected HttpClient ConfigureClientWithGraphQL() =>
        _webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Scoped(_ => _graphQLClient));
            });
        }).CreateClient();

    protected string Serialize(object subj)
    {
        return JsonConvert.SerializeObject(subj, _serializerSettings);
    }

    protected void AssertErrorCodeFromResponse(ErrorCode errorCode, string responseContent)
    {
        var match = Regex.Match(responseContent, ErrorResponsePattern);
        string got = match.Groups[1].Value;
        string expected = errorCode.ToString();
        Assert.Equal(expected, got);
    }

    protected HttpMessageHandler GetMockedHttpMessageHandler(List<string> responseContents)
    {
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        var result = httpMessageHandler.Protected()
                          .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        foreach (string responseContent in responseContents)
        {
            HttpContent content = new StringContent(responseContent, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            };
            result = result.ReturnsAsync(response);
        }
        return httpMessageHandler.Object;
    }

    public void Dispose()
    {
        _serviceScope?.Dispose();
        _dbContext?.Dispose();
    }
}