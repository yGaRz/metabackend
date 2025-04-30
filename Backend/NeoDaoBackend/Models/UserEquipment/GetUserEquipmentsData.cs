using NeoDaoBackend.Models.InventoryItems;
using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoDaoBackend.Models.UserEquipment;

public class GetUserEquipmentsData
{
    public Guid ItemId { get; set; }
    public SlotType SlotType { get; set; }
}