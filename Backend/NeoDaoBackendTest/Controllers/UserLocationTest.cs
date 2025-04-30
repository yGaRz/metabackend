using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserLocation;
using NeoDaoBackendTest.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackendTest.Controllers;
public class UserLocationTest(WebApplicationFactoryWithDatabase factory) : TestBaseWithDatabase(factory)
{
    [Fact]
    public async Task CreateUsersLocations_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = CreateUsersLocationsRequest();
        var userIds = request.PlayerLocations.Select(x => x.UserId);

        CreateUsersInDbContext(userIds);

        // Act 
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createOrUpdateUsersLocations", content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        request.PlayerLocations.ForEach(x =>
        {
            PlayerLocation actual = GetUserLocationFromDbContext(x.UserId);
            PlayerLocationDTO expected = x.Location;
            AssertLocationsAreEqual(expected, actual);
        });
    }

    [Fact]
    public async Task UpdateUsersLocations_ValidRequest_ReturnsSuccess()
    {
        // Arrange

        var playerLocations = CreateUsersAndLocationsInDbContext(3).ToList();
        var userIds = playerLocations.Select(x => x.UserId).ToList();

        var request = CreateUsersLocationsRequest(userIds);

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createOrUpdateUsersLocations", content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // excepted update player locations
        request.PlayerLocations.ForEach(x =>
        {
            PlayerLocation actual = GetUserLocationFromDbContext(x.UserId);
            PlayerLocationDTO expected = x.Location;
            AssertLocationsAreEqual(expected, actual);
        });
    }

    [Fact]
    public async Task GetUserLocation_ReturnsCorrectLocations()
    {
        // Arrange 
        PlayerLocation playerLocation = CreateUserAndLocationInDbContext();

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var response = await client.GetAsync($"/api/public/getUserLocation?userId={playerLocation.UserId}");

        // Assert
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        PlayerLocationDTO actual = JsonConvert.DeserializeObject<PlayerLocationDTO>(responseContent)!;
        PlayerLocationDTO expected = new PlayerLocationDTO()
        {
            LevelName = playerLocation.LevelName,
            Tag = playerLocation.Tag,
            XCoordinate = playerLocation.XCoordinate,
            YCoordinate = playerLocation.YCoordinate,
            ZCoordinate = playerLocation.ZCoordinate
        };

        Assert.Equal(expected.YCoordinate, actual.YCoordinate);
        Assert.Equal(expected.XCoordinate, actual.XCoordinate);
        Assert.Equal(expected.ZCoordinate, actual.ZCoordinate);
        Assert.Equal(expected.LevelName, actual.LevelName);
        Assert.Equal(expected.Tag, actual.Tag);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateUsersLocations_MissingServerSecretHeader_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateUsersLocationsRequest();

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/createOrUpdateUsersLocations", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{invalidJson:}")]
    public async Task CreateUsersLocations_InvalidRequest_ReturnsBadRequest(string json)
    {
        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(json, Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/createOrUpdateUsersLocations", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUsersLocations_MissingPlayerLocations_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrUpdateUsersLocationsRequest
        {
            PlayerLocations = Enumerable.Empty<CreatePlayerLocationRequest>().ToList()
        };

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/createOrUpdateUsersLocations", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUsersLocations_NonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateUsersLocationsRequest();
        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, JsonMediaType);
        var response = await client.PostAsync("/api/public/createOrUpdateUsersLocations", content);
        var stringJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserLocation_NonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        Guid nonExistentUserId = Guid.NewGuid();

        // Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var response = await client.GetAsync($"/api/public/getUserLocation?userId={nonExistentUserId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void CreateUserLocationRequestValidateContract()
    {
        CreateOrUpdateUsersLocationsRequest createOrUpdateUsersLocationsRequest = new CreateOrUpdateUsersLocationsRequest()
        {
            PlayerLocations = new List<CreatePlayerLocationRequest>()
        };
        createOrUpdateUsersLocationsRequest.PlayerLocations.Add(new CreatePlayerLocationRequest()
        {
            UserId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Location = new PlayerLocationDTO()
            {
                LevelName = "L_MetacityGym",
                Tag = "R_MetacityGym",
                XCoordinate = 100,
                YCoordinate = -123.123,
                ZCoordinate = 0
            }
        });
        string result = JsonConvert.SerializeObject(createOrUpdateUsersLocationsRequest);
        string contract = $"{{\"PlayerLocations\":[" +
            $"{{" +
            $"\"UserId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
            $"\"Location\":" +
            $"{{" +
            $"\"LevelName\":\"L_MetacityGym\"," +
            $"\"XCoordinate\":100.0," +
            $"\"YCoordinate\":-123.123," +
            $"\"ZCoordinate\":0.0," +
            $"\"Tag\":\"R_MetacityGym\"}}" +
            $"}}" +
            $"]}}";
        Assert.True(result == contract);
    }

    [Fact]
    public void UserLocationResponseValidateContract()
    {
        PlayerLocationDTO playerLocationDTO = new PlayerLocationDTO()
        {
            LevelName = "L_MetacityGym",
            Tag = "R_MetacityGym",
            XCoordinate = 100,
            YCoordinate = -123.123,
            ZCoordinate = 0
        };
        string result = JsonConvert.SerializeObject(playerLocationDTO);
        string contract = $"{{" +
            $"\"LevelName\":\"L_MetacityGym\"," +
            $"\"XCoordinate\":100.0," +
            $"\"YCoordinate\":-123.123," +
            $"\"ZCoordinate\":0.0," +
            $"\"Tag\":\"R_MetacityGym\"}}";
        Assert.True(result == contract);
    }

    #region database methods  

    private void CreateUsersInDbContext(IEnumerable<Guid> userIds)
    {
        foreach (var userId in userIds)
            _testDb.CreateUser(userId, Guid.NewGuid());
    }

    private IList<PlayerLocation> CreateUsersAndLocationsInDbContext(int count)
    {
        var fixture = new Fixture();
        var playerLocations = fixture
            .Build<PlayerLocation>()
            .Without(u => u.User)
            .CreateMany(count)
            .ToList();

        playerLocations.ForEach(x => _testDb.CreateUser(x.UserId, Guid.NewGuid()));

        _dbContext.PlayerLocations.AddRange(playerLocations);
        _dbContext.SaveChanges();
        return playerLocations;
    }

    private PlayerLocation CreateUserAndLocationInDbContext()
    {
        var fixture = new Fixture();
        var playerLocation = fixture
            .Build<PlayerLocation>()
            .Without(u => u.User)
            .Create();

        _testDb.CreateUser(playerLocation.UserId, Guid.NewGuid());
        _dbContext.PlayerLocations.Add(playerLocation);
        _dbContext.SaveChanges();

        return playerLocation;
    }

    public PlayerLocation GetUserLocationFromDbContext(Guid userId)
    {
        return _dbContext.PlayerLocations
            .Where(us => us.UserId == userId)
            .AsNoTracking()
            .First();
    }
    #endregion

    #region build request methods 

    private CreateOrUpdateUsersLocationsRequest CreateUsersLocationsRequest()
    {
        var fixture = new Fixture();
        var request = fixture.Create<CreateOrUpdateUsersLocationsRequest>();
        return request;
    }

    private CreateOrUpdateUsersLocationsRequest CreateUsersLocationsRequest(IList<Guid> userIds)
    {
        var fixture = new Fixture();
        var playerLocations = fixture
                    .CreateMany<CreatePlayerLocationRequest>(userIds.Count)
                    .ToList();

        for (int i = 0; i < playerLocations.Count; i++)
        {
            playerLocations[i].UserId = userIds[i];
        }
        var request = fixture.Build<CreateOrUpdateUsersLocationsRequest>()
                             .With(x => x.PlayerLocations, playerLocations)
                             .Create();
        return request;
    }

    #endregion

    #region private methods

    private void AssertLocationsAreEqual(PlayerLocationDTO expected, PlayerLocation actual)
    {
        Assert.Equal(expected.YCoordinate, actual.YCoordinate);
        Assert.Equal(expected.XCoordinate, actual.XCoordinate);
        Assert.Equal(expected.ZCoordinate, actual.ZCoordinate);
        Assert.Equal(expected.LevelName, actual.LevelName);
        Assert.Equal(expected.Tag, actual.Tag);
    }

    #endregion
}