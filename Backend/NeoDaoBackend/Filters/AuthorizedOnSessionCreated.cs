using NeoDaoBackend.Models.Auth;

namespace NeoDaoBackend.Filters;

public class AuthorizedOnSessionCreated : BaseAuthorized {
    public AuthorizedOnSessionCreated() : base([UserRole.SESSION_CREATED, UserRole.AUTHENTICATED]) { }
}
