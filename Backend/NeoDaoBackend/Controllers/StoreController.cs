using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.StoreItems;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class StoreController : BaseController
{
    private readonly StoresService _storesService;

    public StoreController(IValidationStorage validationStorage, StoresService storesService) : base(validationStorage)
    {
        _storesService = storesService;
    }
    
    [ServerAuthorized]
    [HttpGet("getStoreItems")]
    public async Task<IActionResult> GetStoreItems([FromQuery] StoreType storeType, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.GetStoreItems(storeType, token), ct);
    }
    
    [ServerAuthorized]
    [HttpGet("getUserStoreItems")]
    public async Task<IActionResult> GetUserStoreItems([FromQuery, ValidGuid] Guid userId, [FromQuery, ValidUniqueStringList] List<string> unrealIds, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.GetUserStoreItems(userId, unrealIds, token), ct);
    }
    
    [ServerAuthorized]
    [HttpPost("buyStoreItem")]
    public async Task<IActionResult> BuyStoreItem([FromBody] BuyStoreItemRequest buyStoreItemRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.BuyStoreItem(buyStoreItemRequest, token), ct);
    }
    
    [ServerAuthorized]
    [HttpPost("sellStoreItem")]
    public async Task<IActionResult> SellStoreItem([FromBody] BuyStoreItemRequest sellStoreItemRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.SellStoreItem(sellStoreItemRequest, token), ct);
    }
    
    [ServerAuthorized]
    [HttpPost("createStoreItem")]
    public async Task<IActionResult> CreateStoreItem([FromBody] CreateStoreItemRequest createStoreItemRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.CreateStoreItem(createStoreItemRequest, token), ct);
    }
    
    [ServerAuthorized]
    [HttpPut("updateStoreItem")]
    public async Task<IActionResult> UpdateStoreItem([FromBody] UpdateStoreItemRequest updateStoreItemRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.UpdateStoreItem(updateStoreItemRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpDelete("deleteStoreItem")]
    public async Task<IActionResult> DeleteStoreItem([FromQuery, ValidGuid] Guid internalId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _storesService.DeleteStoreItem(internalId, token), ct);
    }
}