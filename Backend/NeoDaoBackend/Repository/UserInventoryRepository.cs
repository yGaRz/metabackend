using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public class UserInventoryRepository : IUserInventoryRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserInventoryRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Item>> GetInventoryItemsByUserId(Guid userId, CancellationToken ct)
    {
        return await _dbContext.Inventories
            .Where(i => i.UserId == userId)
            .SelectMany(i => i.Items)
            .ToListAsync(ct);
    }

    public async Task CreateItem(Guid userId, Item itemDTO, CancellationToken ct)
    {
        var inventory = await GetInventoryByUserId(userId, ct);
        
        var item = new Item
        {
            ItemId = itemDTO.ItemId,
            UnrealItemId = itemDTO.UnrealItemId,
            Quantity = itemDTO.Quantity,
            ExpirationDate = itemDTO.ExpirationDate,
            Properties = itemDTO.Properties,
            InventoryId = inventory.InventoryId
        };
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateItem(Item item, CancellationToken ct)
    {
        _dbContext.Items.Update(item);
        await _dbContext.SaveChangesAsync(ct);
    }

    public void UpdateItemNoSave(Item item, CancellationToken ct)
    {
        _dbContext.Items.Update(item);
    }

    public async Task DeleteItem(Item item, CancellationToken ct)
    {
        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task<Inventory> GetInventoryByUserId(Guid userId, CancellationToken ct)
    {
        return await _dbContext.Inventories
            .Where(i => i.UserId == userId)
            .FirstAsync(ct);
    }

    public async Task<Item?> GetItemByIdOptional(Guid itemId, CancellationToken ct)
    {
        return await _dbContext.Items
            .Where(i => i.ItemId == itemId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Item> GetItemById(Guid itemId, CancellationToken ct)
    {
        Item? item = await GetItemByIdOptional(itemId, ct);
        if (item == null)
        {
            throw new ApplicationException($"Item with id {itemId} not found");
        }
        return item;
    }
    
    public async Task<List<Item>> GetItemCollection(string unrealItemId, Guid inventoryId, CancellationToken ct)
    {
        return await _dbContext.Items
            .Where(i => i.UnrealItemId == unrealItemId && i.InventoryId == inventoryId)
            .ToListAsync(ct);
    }
    
    public async Task<Item?> GetItemByUnrealIdFromInventory(string unrealItemId, Guid inventoryId, CancellationToken ct)
    {
        return await _dbContext.Items
            .Where(i => i.UnrealItemId == unrealItemId && i.InventoryId == inventoryId)
            .FirstOrDefaultAsync(ct);
    }

    public void CreateInventoryNoSave(Guid userId, Guid inventoryId, CancellationToken ct)
    {
        var inventory = new Inventory { UserId = userId, InventoryId = inventoryId};
        _dbContext.Inventories.Add(inventory);
    }

    public async Task CreateItem(Item item, CancellationToken ct)
    {
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(ct);
    }

    public void CreateItemNoSave(Item item, CancellationToken ct)
    {
        _dbContext.Items.Add(item);
    }
}