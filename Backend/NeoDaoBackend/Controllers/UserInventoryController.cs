using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.InventoryItems;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserInventoryController : BaseController
{
    private readonly UserInventoryService _userInventoryService;

    public UserInventoryController(UserInventoryService userInventoryService, IValidationStorage validationStorage)
        : base(validationStorage)
    {
        _userInventoryService = userInventoryService;
    }

    [ServerAuthorized]
    [HttpGet("getItems")]
    public async Task<IActionResult> GetItems([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userInventoryService.GetItems(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createItem")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemPlayerInventoryRequest itemRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userInventoryService.CreateItem(itemRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateItem")]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateItemInventoryRequest itemRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userInventoryService.UpdateItem(itemRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpDelete("deleteItem")]
    public async Task<IActionResult> DeleteItem([FromBody] DeleteItemInventoryRequest itemData, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userInventoryService.DeleteItem(itemData, token), ct);
    }
}