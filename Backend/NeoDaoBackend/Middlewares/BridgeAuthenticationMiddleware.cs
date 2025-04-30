using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Service;
using Serilog.Context;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Handlers;

public class BridgeAuthenticationMiddleware {
    private readonly ILogger<BridgeAuthenticationMiddleware> _logger;
    private readonly RequestDelegate _next;

    public BridgeAuthenticationMiddleware(ILogger<BridgeAuthenticationMiddleware> logger, RequestDelegate next) {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuthorizationService authorizationService) {
        // get the header and validate
        string? rawSessionId = context.Request.Headers[SessionIdHeader];
        Guid? externalSessionId = null;
        if (rawSessionId != null) {
            try {
                externalSessionId = Guid.Parse(rawSessionId);
            } catch (FormatException) {
                _logger.LogWarning("External session id {sessionId} is incorrect", rawSessionId);
            }
        }

        NeoDaoUser user = await authorizationService.GetUser(externalSessionId);
        context.Items.Add(UserKey, user);
        using (LogContext.PushProperty("InternalSessionId", user.InternalSessionId))
        using (LogContext.PushProperty("UserId", user.UserId)) {
            await _next(context);
        }
    }

}