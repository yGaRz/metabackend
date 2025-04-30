using NeoDaoBackend.Models.Mission;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;

namespace NeoDaoBackend.Service;

public class UserMissionService
{
    private readonly ILogger<UserMissionService> _logger;
    private readonly IValidationStorage _validationStorage;
    private readonly IUserMissionRepository _userMissionRepository;
    private readonly IUserDataRepository _userDataRepository;

    public UserMissionService(ILogger<UserMissionService> logger, IValidationStorage validationStorage,
        IUserMissionRepository userMissionRepository, IUserDataRepository userDataRepository)
    {
        _logger = logger;
        _validationStorage = validationStorage;
        _userMissionRepository = userMissionRepository;
        _userDataRepository = userDataRepository;
    }

    #region Actions

    public async Task<MissionsResponse> GetMissions(Guid userId, string? missionId, MissionStatus? status, CancellationToken ct)
    {
        bool isRequestValid = await ValidateGetMissions(userId, ct);
        if (!isRequestValid)
        {
            return null!;
        }
        return new MissionsResponse()
        {
            Missions = await _userMissionRepository.GetUserMissions(userId, missionId, status, ct)
        };
    }

    public async Task<bool> CreateMission(MissionRequest missionData, CancellationToken ct)
    {
        bool isRequestValid = await ValidateCreateMission(missionData, ct);
        if (!isRequestValid)
        {
            return false;
        }


        await _userMissionRepository.CreateUserMission(missionData.UserId, missionData.Mission, ct);
        return true;
    }

    public async Task<bool> UpdateMission(MissionRequest updateMissionRequest, CancellationToken ct)
    {
        bool isRequestValid = await ValidateUpdateMission(updateMissionRequest, ct);
        if (!isRequestValid)
        {
            return false;
        }

        await _userMissionRepository.UpdateUserMission(updateMissionRequest.UserId, updateMissionRequest.Mission, ct);
        return true;
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateGetMissions(Guid userId, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(userId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, userId);
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateCreateMission(MissionRequest request, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(request.UserId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, request.UserId);
        }
        if (await _userMissionRepository.MissionExists(request.UserId, request.Mission.MissionId, ct))
        {
            AddUserMissionAlreadyExistsError(request.UserId, request.Mission.MissionId);
        }
        if (request.Mission.Objectives.Select(x => x.ObjectiveId).Distinct().Count() != request.Mission.Objectives.Count())
        {
            AddObjectiveIdIsDuplicatedError(request.Mission.MissionId);
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateUpdateMission(MissionRequest request, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(request.UserId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, request.UserId);
        }
        if (!await _userMissionRepository.MissionExists(request.UserId, request.Mission.MissionId, ct))
        {
            AddUnknownUserMissionError(request.UserId, request.Mission.MissionId);
        }
        if (request.Mission.Objectives.Select(x => x.ObjectiveId).Distinct().Count() != request.Mission.Objectives.Count())
        {
            AddObjectiveIdIsDuplicatedError(request.Mission.MissionId);
        }
        return _validationStorage.IsValid;
    }

    private void AddObjectiveIdIsDuplicatedError(string missionId)
    {
        _validationStorage.AddError(ErrorCode.UserMissionHasDuplicatedObjective, $"Objectives is duplicated in mission {missionId}");
    }

    private void AddUserMissionAlreadyExistsError(Guid userId, string missionId)
    {
        _validationStorage.AddError(ErrorCode.UserMissionAlreadyExists, $"User with Id {userId} already has mission {missionId}");
    }

    private void AddUnknownUserMissionError(Guid userId, string missionId)
    {
        _validationStorage.AddError(ErrorCode.UnknownUserMission, $"User with Id {userId} does not have mission {missionId}");
    }

    #endregion
}