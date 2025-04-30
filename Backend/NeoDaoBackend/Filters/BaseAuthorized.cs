using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NeoDaoBackend.Models.Auth;
using System.Net;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Filters;

public abstract class BaseAuthorized : ActionFilterAttribute {
    protected readonly List<UserRole> _allowedRoles;

    public BaseAuthorized(List<UserRole> allowedRoles) {
        _allowedRoles = allowedRoles;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
        object? user = context.HttpContext.Items[UserKey];
        if (user == null || !(user is NeoDaoUser)) {
            context.Result = new ContentResult {
                Content = "Authorization handling error",
                StatusCode = (int) HttpStatusCode.InternalServerError
            };
            return;
        }
        NeoDaoUser neoDaoUser = (NeoDaoUser) user;
        if (neoDaoUser.Role == UserRole.NOT_AUTHENTICATED) {
            context.Result = new ContentResult {
                Content = null,
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
            return;
        }
        if (!_allowedRoles.Contains(neoDaoUser.Role)) {
            context.Result = new ContentResult {
                Content = null,
                StatusCode = (int) HttpStatusCode.Forbidden
            };
            return;
        }
        base.OnActionExecuting(context);
    }
}
