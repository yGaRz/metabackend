using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Repository;

public class StoreRepository : IStoreRepository
{
    private readonly NeoDaoDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public StoreRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StoreItem?> GetStoreItemByInternalId(Guid internalId, CancellationToken ct)
    {
        return await _dbContext.StoreItems
            .Where(e => e.InternalId == internalId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StoreItem?> GetStoreItemByUnrealId(string unrealId, CancellationToken ct)
    {
        return await _dbContext.StoreItems
            .Where(e => e.UnrealId == unrealId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsStoreItemOwned(Guid internalId, CancellationToken ct)
    {
        return await _dbContext.UserStorePurchases
            .Where(e => e.StoreItemId == internalId)
            .AnyAsync(ct);
    }

    public void AddUserStoreItemPurchaseNoSave(UserStorePurchase userStorePurchase)
    {
        _dbContext.UserStorePurchases.Add(userStorePurchase);
    }

    public async Task<bool> HasUserAlreadyPurchased(Guid userId, Guid internalId, CancellationToken ct)
    {
        return await _dbContext.UserStorePurchases
            .AnyAsync(purchase => purchase.UserId == userId && purchase.StoreItemId == internalId, ct);
    }
    
    public async Task<UserStorePurchase?> GetUserPurchased(Guid internalId,  Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserStorePurchases
            .Where(purchase => purchase.StoreItemId == internalId && purchase.UserId == userId)
            .FirstOrDefaultAsync(ct);
    }
    
    public void DeleteItemPurchased(UserStorePurchase userStorePurchase)
    {
        _dbContext.UserStorePurchases.Remove(userStorePurchase);
    }

    public async Task<IEnumerable<StoreItem>> GetStoreItems(List<string> unrealIds, CancellationToken ct)
    {
        var query = _dbContext.StoreItems.AsQueryable();
        if (unrealIds.Count > 0)
        {
            query = query.Where(item => unrealIds.Contains(item.UnrealId));
        }
        return await query.ToListAsync(ct);
    }

    public async Task<IEnumerable<StoreItem>> GetStoreItemsByType(StoreType storeType, CancellationToken ct)
    {
        return await _dbContext.StoreItems
            .Where(item => item.StoreType == storeType)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetUserStorePurchases(Guid userId, List<string> unrealIds, CancellationToken ct)
    {
        return await _dbContext.UserStorePurchases
            .Where(purchase => purchase.UserId == userId && unrealIds.Contains(purchase.StoreItem.UnrealId))
            .Select(purchase => purchase.StoreItem.UnrealId)
            .ToListAsync(ct);
    }

    public async Task CreateStoreItem(StoreItem storeItem, CancellationToken ct)
    {
        _dbContext.StoreItems.Add(storeItem);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateStoreItem(StoreItem storeItem, CancellationToken ct)
    {
        _dbContext.StoreItems.Update(storeItem);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteStoreItem(StoreItem storeItem, CancellationToken ct)
    {
        _dbContext.StoreItems.Remove(storeItem);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task BeginTransaction(CancellationToken ct)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransaction(CancellationToken ct)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransaction(CancellationToken ct)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            _transaction.Dispose();
            _transaction = null;
        }
    }

}