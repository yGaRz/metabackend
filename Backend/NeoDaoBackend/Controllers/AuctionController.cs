using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.NFTAuction;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class AuctionController : BaseController
{
    private readonly AuctionService _auctionService;

    public AuctionController(IValidationStorage validationStorage, AuctionService auctionService)
        : base(validationStorage)
    {
        _auctionService = auctionService;
    }

    [ServerAuthorized]
    [HttpGet("getAuctionLots")]
    public async Task<IActionResult> GetAuctionLots(CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.GetAuctionLots(token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getUserAuctionLots")]
    public async Task<IActionResult> GetUserAuctionLots([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.GetUserAuctionLots(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createAuctionLot")]
    public async Task<IActionResult> CreateAuctionLot([FromBody] CreateAuctionLotRequest createAuctionLotRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.CreateAuctionLotItem(createAuctionLotRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createAuction")]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.CreateAuction(request, token), ct);
    }
    
    [ServerAuthorized]
    [HttpPost("createAuctionBids")]
    public async Task<IActionResult> CreateAuctionBids([FromBody] CreateAuctionBidsRequest auctionBidsRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.CreateAuctionBids(auctionBidsRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getCurrentAuction")]
    public async Task<IActionResult> GetCurrentAuction(CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.GetCurrentAuction(token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getAllAuction")]
    public async Task<IActionResult> GetAllAuction(CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.GetAllAuction(token), ct);
    }

    [ServerAuthorized]
    [HttpDelete("deleteAuction")]
    public async Task<IActionResult> DeleteAuction([FromQuery, ValidGuid] Guid auctionId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _auctionService.DeleteAuction(auctionId, token), ct);
    }
}