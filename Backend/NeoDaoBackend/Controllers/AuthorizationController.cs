using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Service;
using NeoDaoBackend.Service.GraphQL;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;
using static NeoDaoBackend.Util.AuthUtils;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class AuthorizationController : BaseController
{
    private readonly IBridgeService _bridgeService;
    private readonly AuthorizationService _authorizationService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly UserDataService _userDataService;

    public AuthorizationController(IBridgeService bridgeService, AuthorizationService authorizationService,
        IUserSessionRepository userSessionRepository, UserDataService userDataService, IValidationStorage validationStorage)
        : base(validationStorage)
    {
        _bridgeService = bridgeService;
        _authorizationService = authorizationService;
        _userSessionRepository = userSessionRepository;
        _userDataService = userDataService;
    }

    // This endpoint is intentionally not authorized cause it's the first step of the actual authorization in the game
    [HttpPost("createSessionRequest")]
    public async Task<IActionResult> CreateSessionRequest(CancellationToken ct)
    {
        IHeaderDictionary headers = HttpContext.Request.Headers;
        return await HandleRequestAsync(async token => await _authorizationService.CreateSessionRequest(headers, token), ct);
    }
    
    // Used by CMS, checking auth
    [ServerAuthorized]
    [HttpGet("checkCmsAuth")]
    public IActionResult CheckCmsAuth()
    {
        return Ok();
    }
    
    [AuthorizedOnSessionCreated]
    [HttpGet("getSessionStatus")]
    public IActionResult GetSessionStatus()
    {
        NeoDaoUser user = GetUserFromContext(HttpContext)!;
        SessionStatusResponse response = new()
        {
            Status = user!.Status!.Value
        };
        return Ok(response);
    }

    [ServerAuthorized]
    [HttpPost("confirmSession")]
    public async Task<IActionResult> ConfirmSession([FromBody] WebhookPayload payload, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _authorizationService.ConfirmSession(payload, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("pushSessionConfirmation")]
    public async Task<IActionResult> PushSessionConfirmation([FromBody] PushSessionConfirmationRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _authorizationService.PushSessionConfirmation(request, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("authorizeFakeUser")]
    [DisallowInProduction]
    public async Task<IActionResult> AuthorizeFakeUser([FromBody] AuthorizeFakeUserRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.AuthorizeFakeUser(request.userId, request.userName, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("generatePromocode")]
    [DisallowInProduction]
    public async Task<IActionResult> GeneratePromocode([FromQuery] int count, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.GeneratePromocode(count, token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getPromocodeList")]
    [DisallowInProduction]
    public async Task<IActionResult> GetPromocodeList(CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.GetPromocodeList(token), ct);
    }

    [ServerAuthorized]
    [HttpPut("activatePromocode")]
    [DisallowInProduction]
    public async Task<IActionResult> ActivatePromocode([FromQuery] string code, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.ActivatePromocode(code,token), ct);
    }

    [ServerAuthorized]
    [HttpDelete("removeAllPromocode")]
    [DisallowInProduction]
    public async Task<IActionResult> RemoveAllPromocode(CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.RemoveAllPromocode(token), ct);
    }
}
