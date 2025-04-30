using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL;
using static NeoDaoBackend.Models.db.SessionStatusMethods;

namespace NeoDaoBackend.Repository;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserSessionRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Session?> GetByExternalId(Guid externalSessionId, CancellationToken ct)
    {
        return await _dbContext.UserSessions
            .Where(x => x.ExternalSessionId == externalSessionId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Session?> GetByInternalId(Guid internalSessionId, CancellationToken ct)
    {
        return await _dbContext.UserSessions
            .Where(x => x.InternalSessionId == internalSessionId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Session?> GetConfirmedSessionByUserId(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserSessions
            .Where(x => x.UserId == userId && x.Status == SessionStatus.CONFIRMED)
            .FirstOrDefaultAsync(ct);
    }

    public async Task CreateUserSession(UserSession userSession, CancellationToken ct)
    {
        Session session = new()
        {
            ExternalSessionId = userSession.Id,
            Status = FromGraphQL(userSession.Status),
            ExpiredAt = userSession.ExpiredAt
        };
        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ConfirmSession(Session session, bool isSuccess, string? token, CancellationToken ct)
    {
        session.Status = isSuccess ? SessionStatus.CONFIRMED : SessionStatus.REMOVED;
        if (session.ConfirmedAt == null && isSuccess)
        {
            session.ConfirmedAt = DateTimeOffset.UtcNow;
            session.Status = SessionStatus.CONFIRMED;
            session.Token = token!;
        }
        if (session.RemovedAt == null && !isSuccess)
        {
            session.RemovedAt = DateTimeOffset.UtcNow;
            session.Status = SessionStatus.REMOVED;
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> RemoveStaleUserSessions(CancellationToken ct)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<Session> sessionsToUpdate = await
            _dbContext.UserSessions.Where(us => us.RemovedAt == null && us.ExpiredAt.CompareTo(now) <= 0).ToListAsync(ct);
        foreach (Session session in sessionsToUpdate)
        {
            session.RemovedAt = now;
            session.Status = SessionStatus.REMOVED;
        }
        await _dbContext.SaveChangesAsync(ct);
        return sessionsToUpdate.Count;
    }

    public async Task RemoveUserSession(Session session, CancellationToken ct)
    {
        session.Status = SessionStatus.REMOVED;
        session.RemovedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateUserId(Guid internalSessionId, Guid userId, CancellationToken ct)
    {
        Session? session = await GetByInternalId(internalSessionId, ct);
        if (session == null)
        {
            throw new ApplicationException($"Couldn't find the session with internal id {internalSessionId} for updateUserId");
        }
        session.UserId = userId;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateSession(Session session, CancellationToken ct)
    {
        _dbContext.UserSessions.Update(session);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    //Use it only on development stage
    public async Task SaveDevelopmentSession(Session session, CancellationToken ct)
    {
        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(ct);
    }
}
