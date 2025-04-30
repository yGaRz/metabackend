using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.InventoryItems;

namespace NeoDaoBackend.Repository;

public class UserEquipmentRepository : IUserEquipmentRepository
{    
    private readonly NeoDaoDbContext _dbContext;

    public UserEquipmentRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateEquipment(Equipment equipment, CancellationToken ct)
    {
        _dbContext.Equipments.Add(equipment);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Inventory> GetInventory(Guid userId, CancellationToken ct)
    {
        return await _dbContext.Inventories
            .Where(e => e.UserId == userId)
            .FirstAsync(ct);
    }

    public async Task<IEnumerable<Equipment>> GetEquipments(Guid inventoryId, CancellationToken ct)
    {
        return await _dbContext.Equipments
            .Where(i => i.InventoryId == inventoryId)
            .ToListAsync(ct);
    }

    public async Task<Equipment?> GetEquipmentBySlotType(Guid inventoryId, SlotType slotType, CancellationToken ct)
    {
        return await _dbContext.Equipments
            .Where(e => e.InventoryId == inventoryId && e.SlotType == slotType)
            .FirstOrDefaultAsync(ct);
    }

    public async Task DeleteEquipment(Guid inventoryId, SlotType slotType, CancellationToken ct)
    {
        var equipment = await _dbContext.Equipments
            .Where(e => e.InventoryId == inventoryId && e.SlotType == slotType)
            .FirstOrDefaultAsync(ct);

        _dbContext.Equipments.Remove(equipment);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateEquipment(Equipment equipment, CancellationToken ct)
    {
        _dbContext.Equipments.Update(equipment);
        await _dbContext.SaveChangesAsync(ct);
    }
}