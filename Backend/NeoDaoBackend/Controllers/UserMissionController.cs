using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Mission;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserMissionController : BaseController
{
    private readonly UserMissionService _userMissionService;
    public UserMissionController(IValidationStorage validationStorage, UserMissionService userMissionService) : base(validationStorage)
    {
        _userMissionService = userMissionService;
    }

    [ServerAuthorized]
    [HttpGet("getMissions")]
    public async Task<IActionResult> GetMissions([FromQuery, ValidGuid] Guid userId,
                                                 [FromQuery, ValidNotEmptyString(IsOptional = true)] string? missionId,
                                                 [FromQuery] MissionStatus? status,
                                                 CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userMissionService.GetMissions(userId, missionId, status, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createMission")]
    public async Task<IActionResult> CreateUserMission([FromBody] MissionRequest userMission, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userMissionService.CreateMission(userMission, token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateMission")]
    public async Task<IActionResult> UpdateUserMission([FromBody] MissionRequest userMission, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userMissionService.UpdateMission(userMission, token), ct);
    }
}