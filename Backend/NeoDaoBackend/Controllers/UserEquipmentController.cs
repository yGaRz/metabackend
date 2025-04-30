using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.UserEquipment;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserEquipmentController : BaseController
{
    private readonly UserEquipmentService _userEquipmentService;

    public UserEquipmentController(UserEquipmentService userEquipmentService, IValidationStorage validationStorage) 
        : base(validationStorage)
    {
        _userEquipmentService = userEquipmentService;
    }

    [ServerAuthorized]
    [HttpGet("getEquipment")]
    public async Task<IActionResult> GetEquipment([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userEquipmentService.GetEquipments(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createEquipment")]
    public async Task<IActionResult> CreateEquipment([FromBody] CreateUserEquipmentData equipmentData, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userEquipmentService.CreateEquipment(equipmentData, token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateEquipment")]
    public async Task<IActionResult> UpdateEquipment([FromBody] CreateUserEquipmentData equipmentData, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userEquipmentService.UpdateEquipment(equipmentData, token), ct);
    }

    [ServerAuthorized]
    [HttpDelete("deleteEquipment")]
    public async Task<IActionResult> DeleteEquipment([FromBody] DeleteEquipmentData equipmentData, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userEquipmentService.DeleteEquipment(equipmentData, token), ct);
    }
}
