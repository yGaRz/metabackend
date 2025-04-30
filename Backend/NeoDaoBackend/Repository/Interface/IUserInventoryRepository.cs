using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public interface IUserInventoryRepository
{
    Task<IEnumerable<Item>> GetInventoryItemsByUserId(Guid userId, CancellationToken ct);
    Task CreateItem(Guid userId, Item items, CancellationToken ct);

    Task UpdateItem(Item item, CancellationToken ct);
    void UpdateItemNoSave(Item item, CancellationToken ct);

    Task DeleteItem(Item item, CancellationToken ct);
    
    Task<Inventory> GetInventoryByUserId(Guid userId, CancellationToken ct);
    Task<Item?> GetItemByIdOptional(Guid itemId, CancellationToken ct);
    Task<Item> GetItemById(Guid itemId, CancellationToken ct);
    Task<Item?> GetItemByUnrealIdFromInventory(string unrealItemId, Guid inventoryId, CancellationToken ct);
    Task<List<Item>> GetItemCollection(string unrealItemId, Guid inventoryId, CancellationToken ct);
    void CreateInventoryNoSave(Guid userId, Guid inventoryId, CancellationToken ct);
    Task CreateItem(Item item, CancellationToken ct);
    void CreateItemNoSave(Item item, CancellationToken ct);
}