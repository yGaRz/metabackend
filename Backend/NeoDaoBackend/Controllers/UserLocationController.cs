using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.UserLocation;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserLocationController : BaseController
{
    private readonly UserLocationService _playerLocationService;

    public UserLocationController(IValidationStorage validationStorage, UserLocationService playerLocationService) : base(validationStorage)
    {
        _playerLocationService = playerLocationService;
    }

    [ServerAuthorized]
    [HttpGet("getUserLocation")]
    public async Task<IActionResult> GetUserLocation([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _playerLocationService.GetPlayerLocation(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createOrUpdateUsersLocations")]
    public async Task<IActionResult> CreateOrUpdateUserLocations([FromBody] CreateOrUpdateUsersLocationsRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _playerLocationService.CreateOrUpdatePlayerLocations(request, token), ct);
    }
}