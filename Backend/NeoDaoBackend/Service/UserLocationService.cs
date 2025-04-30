using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserLocation;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;

namespace NeoDaoBackend.Service;

public class UserLocationService
{
    private readonly ILogger<UserLocationService> _logger;
    private readonly IValidationStorage _validationStorage;
    private readonly IUserLocationRepository _userLocationRepository;
    private readonly IUserDataRepository _userDataRepository;

    public UserLocationService(ILogger<UserLocationService> logger, IValidationStorage validationStorage,
        IUserLocationRepository userLocationRepository, IUserDataRepository userDataRepository)
    {
        _logger = logger;
        _validationStorage = validationStorage;
        _userLocationRepository = userLocationRepository;
        _userDataRepository = userDataRepository;
    }

    #region Actions

    public async Task<PlayerLocationDTO?> GetPlayerLocation(Guid userId, CancellationToken ct)
    {
        bool isRequestValid = await ValidateGetPlayerLocation(userId, ct);
        if (!isRequestValid)
        {
            return null;
        }
        PlayerLocation? playerLocation = await _userLocationRepository.GetPlayerLocationByUserId(userId, ct);
        if (playerLocation == null)
        {
            return null;
        }
        return new PlayerLocationDTO {
            LevelName = playerLocation.LevelName,
            XCoordinate = playerLocation.XCoordinate,
            YCoordinate = playerLocation.YCoordinate,
            ZCoordinate = playerLocation.ZCoordinate,
            Tag = playerLocation.Tag
        };
    }

    public async Task<bool> CreateOrUpdatePlayerLocations(CreateOrUpdateUsersLocationsRequest request, CancellationToken ct)
    {
        bool isRequestValid = await ValidateCreateOrUpdatePlayerLocations(request, ct);
        if (!isRequestValid)
        {
            return false;
        }
        var playerLocations = request.PlayerLocations.Select(location => new PlayerLocation
        {
            UserId = location.UserId,
            LevelName = location.Location.LevelName,
            XCoordinate = location.Location.XCoordinate,
            YCoordinate = location.Location.YCoordinate,
            ZCoordinate = location.Location.ZCoordinate,
            Tag = location.Location.Tag
        }).ToList();

        await _userLocationRepository.CreateOrUpdatePlayerLocations(playerLocations, ct);
        return true;
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateGetPlayerLocation(Guid userId, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(userId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, userId);
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateCreateOrUpdatePlayerLocations(CreateOrUpdateUsersLocationsRequest request, CancellationToken ct)
    {
        List<Guid> userIds = request.PlayerLocations.Select(pl => pl.UserId).ToList();
        Dictionary<Guid, User> userById = await _userDataRepository.GetUsersById(userIds, ct);
        foreach (Guid userId in userIds)
        {
            if (!userById.ContainsKey(userId))
            {
                ValidationUtils.AddUnknownUserError(_validationStorage, userId);
            }
        }
        return _validationStorage.IsValid;
    }

    #endregion
}
