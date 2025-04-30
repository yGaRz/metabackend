using GraphQL.Client.Http;
using Moq.Protected;
using Moq;
using System.Net;
using System.Text;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using NeoDaoBackend.Util;

namespace NeoDaoBackendTest.Infrastructure;

public abstract class TestBase
{
    protected CustomWebApplicationFactory _appFactory = null!;
    protected GraphQLHttpClient _graphQlClient = null!;
    protected JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new OrderedContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatString = "yyyy-MM-dd HH:mm:ss.fffK"
    };

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
        _graphQlClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer(), client);
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
}
