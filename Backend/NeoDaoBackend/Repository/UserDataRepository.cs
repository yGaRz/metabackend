using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public class UserDataRepository : IUserDataRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserDataRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdOptional(Guid userId, CancellationToken ct)
    {
        return await _dbContext.Users.FindAsync(userId, ct);
    }

    public async Task<User> GetById(Guid userId, CancellationToken ct)
    {
        User? user = await GetByIdOptional(userId, ct);
        if (user == null)
        {
            throw new ApplicationException($"Couldn't load user {userId} by id");
        }
        return user;
    }

    public async Task<Dictionary<Guid, User>> GetUsersById(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        return await _dbContext.Users
            .Where(user => userIds.Contains(user.UserId))
            .ToDictionaryAsync(user => user.UserId, ct);
    }

    public void CreateUserNoSave(Guid userId, string userName, CancellationToken ct)
    {
        var user = new User
        {
            UserId = userId,
            UserName = userName,
            LastLoginAt = DateTimeOffset.UtcNow
        };
        _dbContext.Users.Add(user);
    }

    public async Task<bool> UserExists(Guid userId, CancellationToken ct)
    {
        User? user = await GetByIdOptional(userId, ct);
        return user != null;
    }

    public async Task DeleteUserData(Guid userId, Guid inventoryId, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dbContext.Items
                .Where(item => item.InventoryId == inventoryId)
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.Equipments
                .Where(item => item.InventoryId == inventoryId)
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.Inventories.Where(ui => ui.UserId == userId).ExecuteDeleteAsync(cancellationToken);

            await _dbContext.UserSessions.Where(us => us.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserAvatar.Where(ua => ua.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserGroups.Where(ug => ug.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.PlayerLocations.Where(pl => pl.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserBalances.Where(ub => ub.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserBalanceTransactions.Where(ubt => ubt.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserMissions.Where(um => um.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.MissionObjectives.Where(mo => mo.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserTransports.Where(ut => ut.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserStorePurchases.Where(usp => usp.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserEmotions.Where(ue => ue.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserActiveCustomizations.Where(uac => uac.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserAvailableCustomizations.Where(uac => uac.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.UserRelations.Where(ur => ur.UserId == userId || ur.RelationUserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.ChatMessages.Where(ur => ur.SenderId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.Channels.Where(ur => ur.UserAId == userId || ur.UserBId == userId).ExecuteDeleteAsync(cancellationToken);
            await _dbContext.AuctionLots.Where(ur => ur.UserId == userId).ExecuteDeleteAsync(cancellationToken);

            await _dbContext.Users.Where(u => u.UserId == userId).ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void UpdateIsOnlineStatus(Guid userId, bool onlineStatus)
    {
        _dbContext.Users.Where(x => x.UserId == userId)
            .ExecuteUpdate(setters => setters
                .SetProperty(x => x.IsOnline, onlineStatus)
                .SetProperty(x => x.LastLoginAt, DateTimeOffset.UtcNow));
    }

    public async Task<bool> IsUserOnline(Guid userId, CancellationToken ct)
    {
        User? user = await GetByIdOptional(userId, ct);
        return user?.IsOnline ?? false;
    }

    public async Task UpdateUserName(Guid userId, string userName, CancellationToken ct)
    {
        User user = await GetById(userId, ct);
        user.UserName = userName;
        _dbContext.Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }

    #region Promocodes

    public async Task AddPromocodes(List<string> promocodes, CancellationToken ct)
    {
        _dbContext.Promocodes.AddRange(promocodes.Select(x => new Promocode
        {
            Code = x,
            CountUse = 1
        }).ToList());
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<string>> GetActivePromocodes(CancellationToken ct)
    {
        return await _dbContext.Promocodes.Where(x => x.CountUse > 0).Select(x => x.Code).ToListAsync(ct);
    }

    public async Task<int> GetCountUsesCode(string code, CancellationToken ct)
    {
        var promocode = await _dbContext.Promocodes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code,ct);
        return promocode?.CountUse ?? 0;
    }

    public async Task UsePromocode(string code, CancellationToken ct)
    {
        var promocode = await _dbContext.Promocodes.Where(x => x.Code == code).FirstAsync(ct);
        promocode.CountUse--;
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearPromocodes(CancellationToken ct)
    {
        _dbContext.Promocodes.RemoveRange(_dbContext.Promocodes);
        await _dbContext.SaveChangesAsync(ct);
    }

    #endregion
}
