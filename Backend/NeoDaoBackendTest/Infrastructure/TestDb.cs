using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackendTest.Infrastructure;

public class TestDb(NeoDaoDbContext dbContext)
{
    private readonly NeoDaoDbContext _dbContext = dbContext;

    #region Create

    public (Guid, Guid) CreateAuthenticatedUser()
    {
        Guid userId = Guid.NewGuid();
        Guid inventoryId= Guid.NewGuid();   
        CreateUser(userId, inventoryId);
        Guid externalSessionId = CreateUserSession(SessionStatus.CONFIRMED, userId);
        return (userId, externalSessionId);
    }

    public void CreateUser(Guid userId, Guid inventoryId, int softAmount = 0)
    {
        User user = new()
        {
            UserId = userId,
            UserName = Guid.NewGuid().ToString(),
        };
        _dbContext.Users.Add(user);
        var inventory = new Inventory { UserId = userId, InventoryId = inventoryId };
        _dbContext.Inventories.Add(inventory);
        var balance = new UserBalance { UserId = userId, SoftAmount = softAmount };
        _dbContext.UserBalances.Add(balance);
        _dbContext.SaveChanges();
    }

    public Guid CreateUserSession(SessionStatus status)
    {
        return CreateUserSession(status, null);
    }

    public Guid CreateUserSession(SessionStatus status, Guid? userId)
    {
        Guid externalSessionId = Guid.NewGuid();
        Session session = new()
        {
            InternalSessionId = Guid.NewGuid(),
            ExternalSessionId = externalSessionId,
            Status = status,
            Token = status == SessionStatus.CREATED ? null : "whatever",
            ConfirmedAt = status == SessionStatus.CREATED ? null : DateTimeOffset.UtcNow,
            ExpiredAt = DateTimeOffset.UtcNow.AddHours(1),
            RemovedAt = status == SessionStatus.REMOVED ? DateTimeOffset.UtcNow : null,
            UserId = userId
        };
        _dbContext.UserSessions.Add(session);
        _dbContext.SaveChanges();
        return externalSessionId;
    }

    public void CreateUserAvatar(Guid userId, AvatarGender gender)
    {
        UserAvatar userAvatar = new()
        {
            UserId = userId,
            Gender = gender,
        };
        _dbContext.UserAvatar.Add(userAvatar);
        _dbContext.SaveChanges(); ;
    }

    public void CreateTransportToUser(Guid userId, string transportId)
    {
        _dbContext.UserTransports.Add(new UserTransport() { UserId = userId, TransportUnrealId = transportId });
        _dbContext.SaveChanges();
    }

    #endregion

    #region Get

    public Session? GetSession(Guid externalSessionId)
    {
        return _dbContext.UserSessions
            .Where(us => us.ExternalSessionId == externalSessionId)
            .FirstOrDefault();
    }

    public User? GetUser(Guid userId)
    {
        return _dbContext.Users
            .Where(u => u.UserId == userId)
            .FirstOrDefault();
    }

    public UserAvatar? GetUserAvatar(Guid userId)
    {
        return _dbContext.UserAvatar
            .Where(u => u.UserId == userId)
            .FirstOrDefault();
    }

    public Inventory? GetInventory(Guid userId)
    {
        return _dbContext.Inventories
            .Where(i => i.UserId == userId)
            .FirstOrDefault();
    }

    public UserBalance? GetUserBalance(Guid userId)
    {
        return _dbContext.UserBalances
            .Where(i => i.UserId == userId)
            .FirstOrDefault();
    }

    #endregion

    public void ClearEFChanges()
    {
        _dbContext.ChangeTracker.Clear();
    }
}
