using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Mission;
using NeoDaoBackend.Validation;
using NeoDaoBackendTest.Extensions;
using NeoDaoBackendTest.Infrastructure;
using Newtonsoft.Json;
using System.Text;
using static NeoDaoBackend.Models.Constants;
namespace NeoDaoBackendTest.Controllers;

public class UserMissionTest(WebApplicationFactoryWithDatabase factory) : TestBaseWithDatabase(factory)
{
    [Fact]
    public async Task CreateUserMission_HappyPath()
    {
        // Arrange
        MissionRequest request = CreateUserMissionRequest(10);
        _testDb.CreateUser(request.UserId, Guid.NewGuid());

        // Act 
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createMission", content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        UserMission mission = _dbContext.UserMissions.Where(um => um.UserId == request.UserId).Include(x => x.Objectives).First();
        Assert.NotNull(mission);
        Assert.Equal(request.UserId, mission.UserId);
        Assert.Equal(request.Mission.MissionId, mission.MissionId);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Year, mission.ExpireTime?.Year);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Month, mission.ExpireTime?.Month);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Day, mission.ExpireTime?.Day);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Hour, mission.ExpireTime?.Hour);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Minute, mission.ExpireTime?.Minute);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Second, mission.ExpireTime?.Second);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Millisecond, mission.ExpireTime?.Millisecond);
        Assert.Equal(request.Mission.MissionType, mission.MissionType);
        Assert.Equal(request.Mission.Status, mission.Status);
        Assert.Equal(request.Mission.Objectives.Count, mission.Objectives.Count);
        foreach (var objective in request.Mission.Objectives)
        {
            var missionObjective = mission.Objectives.FirstOrDefault(x => x.ObjectiveId == objective.ObjectiveId);
            Assert.NotNull(missionObjective);
            Assert.Equal(objective.Metadata, missionObjective.Metadata);
            Assert.Equal(objective.Status, missionObjective.Status);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task CreateUserMission_MissionAlreadyExists(int count_objectives)
    {
        // Arrange
        MissionRequest request = CreateUserMissionRequest(count_objectives);
        _testDb.CreateUser(request.UserId, Guid.NewGuid());
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createMission", content);
        MissionRequest new_request = CreateUserMissionRequest(count_objectives);
        new_request.Mission.MissionId = request.Mission.MissionId;
        new_request.UserId = request.UserId;

        //Act
        client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        response = await client.PostAsync("/api/public/createMission", content);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UserMissionAlreadyExists));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 5)]
    [InlineData(2, 5)]
    [InlineData(3, 5)]
    [InlineData(4, 5)]
    public async Task CreateUserMission_DublicateObjectives(int number, int count_objectives)
    {
        // Arrange
        MissionRequest request = CreateUserMissionRequest(count_objectives);
        _testDb.CreateUser(request.UserId, Guid.NewGuid());
        var objectiveDTO = CreateObjectiveDTO();
        objectiveDTO.ObjectiveId = request.Mission.Objectives[number].ObjectiveId;
        request.Mission.Objectives.Add(objectiveDTO);

        // Act 
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createMission", content);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UserMissionHasDuplicatedObjective));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetUserMission_HappyPath(int count_objectives)
    {
        //Arrange
        MissionRequest request = CreateUserMissionRequest(count_objectives);
        _testDb.CreateUser(request.UserId, Guid.NewGuid());
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createMission", content);

        // Act
        client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        response = await client.GetAsync($"/api/public/getMissions?userId={request.UserId}");
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        var actual = JsonConvert.DeserializeObject<MissionsResponse>(responseContent);
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.Single(actual.Missions);
        Assert.Equal(request.Mission.MissionId, actual.Missions.First().MissionId);
        var dto = actual.Missions.First();
        Assert.Equal(request.Mission.Status, dto.Status);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Year, dto.ExpireTime?.Year);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Month, dto.ExpireTime?.Month);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Day, dto.ExpireTime?.Day);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Hour, dto.ExpireTime?.Hour);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Minute, dto.ExpireTime?.Minute);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Second, dto.ExpireTime?.Second);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Millisecond, dto.ExpireTime?.Millisecond);
        Assert.Equal(request.Mission.Objectives.Count(), dto.Objectives.Count());
        foreach (var obj in request.Mission.Objectives)
        {
            var dto_obj = dto.Objectives.FirstOrDefault(x => x.ObjectiveId == obj.ObjectiveId);
            Assert.NotNull(dto_obj);
            Assert.Equal(obj.Metadata, dto_obj.Metadata);
            Assert.Equal(obj.Status, dto_obj.Status);
        }
    }

    [Fact]
    public async Task GetUserMission_UserNotFound()
    {
        //Arrange
        var userId = Guid.NewGuid();

        // Act
        var client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var response = await client.GetAsync($"/api/public/getMissions?userId={userId}");
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UnknownUser));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task UpdateUserMisstion_HappyPath(int count_update)
    {
        //Arrange
        MissionRequest request = CreateUserMissionRequest(count_update);
        _testDb.CreateUser(request.UserId, Guid.NewGuid());
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createMission", content);

        //Act
        for (var i = 0; i < count_update; i++)
        {
            request.Mission.Objectives.Add(CreateObjectiveDTO());
            content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
            response = await client.PutAsync("/api/public/updateMission", content);
            response.EnsureSuccessStatusCode();
        }

        // Assert
        response = await client.GetAsync($"/api/public/getMissions?userId={request.UserId}");
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<MissionsResponse>(responseContent);
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.Single(actual.Missions);
        Assert.Equal(request.Mission.MissionId, actual.Missions.First().MissionId);
        var dto = actual.Missions.First();
        Assert.Equal(request.Mission.Status, dto.Status);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Year, dto.ExpireTime?.Year);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Month, dto.ExpireTime?.Month);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Day, dto.ExpireTime?.Day);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Hour, dto.ExpireTime?.Hour);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Minute, dto.ExpireTime?.Minute);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Second, dto.ExpireTime?.Second);
        Assert.Equal(request.Mission.ExpireTime?.UtcDateTime.Millisecond, dto.ExpireTime?.Millisecond);
        Assert.Equal(request.Mission.Objectives.Count(), dto.Objectives.Count());
        foreach (var obj in request.Mission.Objectives)
        {
            var dto_obj = dto.Objectives.FirstOrDefault(x => x.ObjectiveId == obj.ObjectiveId);
            Assert.NotNull(dto_obj);
            Assert.Equal(obj.Metadata, dto_obj.Metadata);
            Assert.Equal(obj.Status, dto_obj.Status);
        }
    }

    [Fact]
    public async Task UpdateUserMission_MissionNotExists_BadRequest()
    {
        //Arrange
        MissionRequest request = CreateUserMissionRequest();
        _testDb.CreateUser(request.UserId, Guid.NewGuid());

        //Act
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        request.Mission.Objectives.Add(CreateObjectiveDTO());
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/public/updateMission", content);
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UnknownUserMission));
    }

    [Fact]
    public async Task UpdateUserMission_DublicateObjectives_BadRequest()
    {
        //Arrange
        MissionRequest request = CreateUserMissionRequest();
        _testDb.CreateUser(request.UserId, Guid.NewGuid());
        HttpClient client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add(ServerSecretHeader, serverSecret);
        var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/public/createMission", content);

        //Act
        var objectiveDTO = CreateObjectiveDTO();
        objectiveDTO.ObjectiveId = request.Mission.Objectives[0].ObjectiveId;
        request.Mission.Objectives.Add(objectiveDTO);
        content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
        response = await client.PutAsync("/api/public/updateMission", content);
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(AssertUtils.HasErrorCode(responseContent, ErrorCode.UserMissionHasDuplicatedObjective));
    }

    private MissionRequest CreateUserMissionRequest(int count_objectives = 10)
    {
        Fixture fixture = new Fixture();
        var request = fixture.Create<MissionRequest>();
        request.Mission.Objectives = fixture.CreateMany<ObjectiveDTO>(count_objectives).ToList();
        return request;
    }

    private ObjectiveDTO CreateObjectiveDTO()
    {
        Fixture fixture = new Fixture();
        return fixture.Create<ObjectiveDTO>();
    }
}
