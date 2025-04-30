using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository.Interface;

public interface IStoreRepository
{
    Task<StoreItem?> GetStoreItemByInternalId(Guid internalId, CancellationToken ct);
    Task<StoreItem?> GetStoreItemByUnrealId(string unrealId, CancellationToken ct);
    Task<bool> IsStoreItemOwned(Guid internalId, CancellationToken ct);
    void AddUserStoreItemPurchaseNoSave(UserStorePurchase userStorePurchase);
    Task<bool> HasUserAlreadyPurchased(Guid userId, Guid internalId, CancellationToken ct);
    Task<UserStorePurchase?> GetUserPurchased(Guid internalId, Guid userId, CancellationToken ct);
    void DeleteItemPurchased(UserStorePurchase userStorePurchase);
    Task<IEnumerable<StoreItem>> GetStoreItems(List<string> unrealIds, CancellationToken ct);
    Task<IEnumerable<StoreItem>> GetStoreItemsByType(StoreType storeType, CancellationToken ct);
    Task<List<string>> GetUserStorePurchases(Guid userId, List<string> unrealIds, CancellationToken ct);
    
    Task CreateStoreItem(StoreItem storeItem, CancellationToken ct);
    Task UpdateStoreItem(StoreItem storeItem, CancellationToken ct);
    Task DeleteStoreItem(StoreItem storeItem, CancellationToken ct);
    
    Task BeginTransaction(CancellationToken ct);
    Task CommitTransaction(CancellationToken ct);
    Task RollbackTransaction(CancellationToken ct);
}