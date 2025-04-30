using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.UserTransport;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserTransportController : BaseController
{
    private readonly UserTransportService _userTransportService;

    public UserTransportController(IValidationStorage validationStorage, UserTransportService userTransportService) : base(validationStorage)
    {
        _userTransportService = userTransportService;
    }

    [ServerAuthorized]
    [HttpGet("getUserTransport")]
    public async Task<IActionResult> GetUserTransport([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userTransportService.GetUserTransport(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("useTransport")]
    public async Task<IActionResult> UseTransport([FromBody] UseTransportRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userTransportService.UseTransport(request, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("useTaxi")]
    public async Task<IActionResult> UseTaxi([FromBody] UseTaxiRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userTransportService.UseTaxi(request, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("addTransportToUser")]
    public async Task<IActionResult> AddTransportToUser([FromBody] AddTransportToUserRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userTransportService.AddTransportToUser(request, token), ct);
    }
}