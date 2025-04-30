using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Customization;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserCustomizationController : BaseController
{
    private readonly UserCustomizationService _customizationService;

    public UserCustomizationController(IValidationStorage validationStorage, UserCustomizationService customizationService) : base(validationStorage)
    {
        _customizationService = customizationService;
    }

    [ServerAuthorized]
    [HttpGet("getUserAvailableCustomizations")]
    public async Task<IActionResult> GetUserAvailableCustomizations([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _customizationService.GetUserAvailableCustomizations(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createUserAvailableCustomization")]
    public async Task<IActionResult> CreateUserAvailableCustomization([FromBody] CreateAvailableCustomizationRequest dto, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _customizationService.CreateUserAvailableCustomization(dto, token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getUserActiveCustomizations")]
    public async Task<IActionResult> GetUserActiveCustomizations([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _customizationService.GetUserActiveCustomizations(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createActiveUserCustomization")]
    public async Task<IActionResult> CreateActiveUserCustomization([FromBody] ActiveUserCustomizationRequest dto, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _customizationService.CreateActiveUserCustomization(dto, token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateActiveUserCustomization")]
    public async Task<IActionResult> UpdateActiveUserCustomization([FromBody] ActiveUserCustomizationRequest dto,
        CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _customizationService.UpdateActiveUserCustomization(dto, token), ct);
    }
}