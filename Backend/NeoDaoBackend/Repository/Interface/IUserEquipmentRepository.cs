using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.InventoryItems;

namespace NeoDaoBackend.Repository;

public interface IUserEquipmentRepository
{
    Task CreateEquipment(Equipment equipment, CancellationToken ct);
    Task<Inventory> GetInventory(Guid userId, CancellationToken ct);
    Task<IEnumerable<Equipment>> GetEquipments(Guid inventoryId, CancellationToken ct);
    Task<Equipment?> GetEquipmentBySlotType(Guid inventoryId, SlotType slotType, CancellationToken ct);
    Task DeleteEquipment(Guid inventoryId, SlotType slotType, CancellationToken ct);
    Task UpdateEquipment(Equipment equipment, CancellationToken ct);
}