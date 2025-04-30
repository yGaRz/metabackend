using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Service;
using static NeoDaoBackend.Util.AuthUtils;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class WebSocketConnectionController : Controller
{
    private readonly ILogger<WebSocketConnectionController> _logger;
    private readonly WebSocketHandler _webSocketHandler;

    public WebSocketConnectionController(ILogger<WebSocketConnectionController> logger, WebSocketHandler webSocketHandler)
    {
        _logger = logger;
        _webSocketHandler = webSocketHandler;
    }

    [Authorized]
    [Route("connect")]
    [HttpGet]
    public async Task<ActionResult> Connect(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            _logger.LogWarning("connect: the request is not WebSocket!");
            return BadRequest();
        }
        NeoDaoUser user = GetUserFromContext(HttpContext)!;
        _logger.LogInformation($"User with internalSessionId {user.InternalSessionId} requested a connection");
        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await _webSocketHandler.HandleWebSocket(ws, user, ct);
        return new EmptyResult();
    }
}