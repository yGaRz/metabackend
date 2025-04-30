using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models.UserTransport;
using NeoDaoBackend.Validation;
using NeoDaoBackendTest.Extensions;
using NeoDaoBackendTest.Infrastructure;
using Newtonsoft.Json;
using System.Text;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackendTest.Controllers;

public class UserTransportTest(WebApplicationFactoryWithDatabase factory) : TestBaseWithDatabase(factory)
{
    [Fact]
    public void UseTransportRequestValidationContract()
    {
        UseTransportRequest request = new UseTransportRequest()
        {
            Price = 200,
            TransportId = "qwerty123",
            UserId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
        };
        string requestString = JsonConvert.SerializeObject(request);
        string contract = $"{{\"UserId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"TransportId\":\"qwerty123\",\"Price\":200}}";
        Assert.True(requestString == contract);
    }

    [Fact]
    public void UseTaxiRequestValidationContract()
    {
        UseTaxiRequest request = new UseTaxiRequest()
        {
            Price = 200,
            UserId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
        };
        string requestString = JsonConvert.SerializeObject(request);
        string contract = $"{{\"UserId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"Price\":200}}";
        Assert.True(requestString == contract);
    }

    [Fact]
    public void AddTransportToUserRequestValidationContract()
    {
        AddTransportToUserRequest request = new AddTransportToUserRequest()
        {
            TransportId = "qwerty123",
            UserId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
        };
        string requestString = JsonConvert.SerializeObject(request);
        string contract = $"{{\"UserId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"TransportId\":\"qwerty123\"}}";
        Assert.True(requestString == contract);
    }

    [Fact]
    public async Task UserGetTaxi_HappyPath()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        var request = new UseTaxiRequest()
        {
            Price = 100,
            UserId = userId,
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/useTaxi", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var balance = _dbContext.UserBalances.Where(x=>x.UserId== userId).AsNoTracking().FirstOrDefault();
        Assert.NotNull(balance);
        Assert.Equal(900, balance.SoftAmount);
    }

    [Fact]
    public async Task UserGetTaxi_HasntSofCoins_BadRequest()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        var request = new UseTaxiRequest()
        {
            Price = 1500,
            UserId = userId,
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/useTaxi", content);
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.NotEnoughSoftCoins));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var balance = _dbContext.UserBalances.Where(x => x.UserId == userId).AsNoTracking().FirstOrDefault();
        Assert.NotNull(balance);
        Assert.Equal(1000, balance.SoftAmount);
    }

    [Fact]
    public async Task AddTransportToUser_HappyPath()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        var request = new AddTransportToUserRequest()
        {
            UserId = userId,
            TransportId = "123456"
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/addTransportToUser", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var userTransports = _dbContext.UserTransports.Where(x => x.UserId == userId).ToList();
        Assert.NotNull(userTransports);
        Assert.Single(userTransports);
        Assert.Equal("123456", userTransports[0].TransportUnrealId);
    }

    [Fact]
    public async Task AddTransportToUser_TransportAlreadyExists_BadRequest()
    {
        //Arrange
        string transportId = "123456";
        Guid userId = Guid.NewGuid();
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        _testDb.CreateTransportToUser(userId, transportId);
        var request = new AddTransportToUserRequest()
        {
            UserId = userId,
            TransportId = transportId
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/addTransportToUser", content);
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UserTransportAlreadyExists));
        var userTransports = _dbContext.UserTransports.Where(x => x.UserId == userId).ToList();
        Assert.NotNull(userTransports);
        Assert.Single(userTransports);
        Assert.Equal(transportId, userTransports[0].TransportUnrealId);
    }

    [Fact]
    public async Task UseTransport_HappyPath()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        string transportId = "123456";
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        _testDb.CreateTransportToUser(userId, transportId);
        UseTransportRequest request = new UseTransportRequest()
        {
            Price = 100,
            TransportId = transportId,
            UserId = userId
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/useTransport", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var balance = _dbContext.UserBalances.Where(x => x.UserId == userId).AsNoTracking().FirstOrDefault();
        Assert.NotNull(balance);
        Assert.Equal(900, balance.SoftAmount);
    }

    [Fact]
    public async Task UseTransport_HasntSofCoins_BadRequest()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        string transportId = "123456";
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        _testDb.CreateTransportToUser(userId, transportId);
        UseTransportRequest request = new UseTransportRequest()
        {
            Price = 1500,
            TransportId = transportId,
            UserId = userId
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/useTransport", content);
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert ErrorCode.NotEnoughSoftCoins.ToString()
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.NotEnoughSoftCoins));
        var balance = _dbContext.UserBalances.Where(x => x.UserId == userId).AsNoTracking().FirstOrDefault();
        Assert.NotNull(balance);
        Assert.Equal(1000, balance.SoftAmount);
    }

    [Fact]
    public async Task UseTransport_HasntTransport_BadRequest()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        string transportId = "123456";
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        UseTransportRequest request = new UseTransportRequest()
        {
            Price = 100,
            TransportId = transportId,
            UserId = userId
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/useTransport", content);
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UserTransportDoesNotExist));
        var balance = _dbContext.UserBalances.Where(x => x.UserId == userId).AsNoTracking().FirstOrDefault();
        Assert.NotNull(balance);
        Assert.Equal(1000, balance.SoftAmount);
    }

    [Fact]
    public async Task GetTransport_HappyPath()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        string transportId1 = "111111";
        string transportId2 = "222222";
        _testDb.CreateUser(userId, Guid.NewGuid(), 1000);
        _testDb.CreateTransportToUser(userId, transportId1);
        _testDb.CreateTransportToUser(userId, transportId2);

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var response = await client.GetAsync($"/api/public/getUserTransport?userId={userId}");
        string responseContent = await response.Content.ReadAsStringAsync();
        List<string>? actual = JsonConvert.DeserializeObject<List<string>>(responseContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Count);
        Assert.Contains(transportId1, actual);
        Assert.Contains(transportId2, actual);
    }
}
