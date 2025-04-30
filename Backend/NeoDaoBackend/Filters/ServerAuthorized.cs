using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Net;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ServerAuthorized : Attribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        string expectedSecret = Environment.GetEnvironmentVariable("SERVER_SECRET") ??
                throw new ApplicationException("Environment variable SERVER_SECRET is not set!");
        context.HttpContext.Request.Headers.TryGetValue(ServerSecretHeader, out StringValues secret);
        if (secret != expectedSecret) {
            context.Result = new StatusCodeResult((int) HttpStatusCode.Unauthorized);
        }
    }
}
