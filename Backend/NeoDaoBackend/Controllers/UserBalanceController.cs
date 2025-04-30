using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Balance;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserBalanceController : BaseController
{
    private readonly UserBalanceService _userBalanceService;

    public UserBalanceController(IValidationStorage validationStorage, UserBalanceService userBalanceService) : base(validationStorage)
    {
        _userBalanceService = userBalanceService;
    }

    [ServerAuthorized]
    [HttpGet("getBalance")]
    public async Task<IActionResult> GetBalance([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userBalanceService.GetUserBalance(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("addCoinsToUser")]
    public async Task<IActionResult> AddCoinsToUserBalance([FromBody] AddUserBalanceRequest addUserBalanceRequest, CancellationToken ct)
    {
        if (addUserBalanceRequest.CoinType == CoinType.HardCoin || addUserBalanceRequest.CoinType == CoinType.BitForceCoin)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return NotFound();
            }
        }
        return await HandleRequestAsync(async token => await _userBalanceService.AddCoinsToUserBalance(addUserBalanceRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getBalanceTransactions")]
    public async Task<IActionResult> GetBalanceTransactions([FromQuery] GetBalanceTransactionRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userBalanceService.GetUserBalanceTransaction(request, token), ct);
    }
}