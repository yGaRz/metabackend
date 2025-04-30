using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL;

namespace NeoDaoBackend.Repository;

public interface IUserSessionRepository {
    public Task<Session?> GetByExternalId(Guid externalSessionId, CancellationToken ct);
    public Task<Session?> GetByInternalId(Guid internalSessionId, CancellationToken ct);
    public Task<Session?> GetConfirmedSessionByUserId(Guid userId, CancellationToken ct);
    public Task CreateUserSession(UserSession userSession, CancellationToken ct);
    public Task UpdateSession(Session session, CancellationToken ct);
    public Task SaveDevelopmentSession(Session session, CancellationToken ct);
    public Task ConfirmSession(Session session, bool isSuccess, string? token, CancellationToken ct);
    public Task<int> RemoveStaleUserSessions(CancellationToken ct);
    public Task UpdateUserId(Guid internalSessionId, Guid userId, CancellationToken ct);
    public Task RemoveUserSession(Session session, CancellationToken ct);
}
