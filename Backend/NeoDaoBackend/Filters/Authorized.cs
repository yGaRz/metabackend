using NeoDaoBackend.Models.Auth;

namespace NeoDaoBackend.Filters;

public class Authorized : BaseAuthorized {
    public Authorized() : base([UserRole.AUTHENTICATED]) { }
}
