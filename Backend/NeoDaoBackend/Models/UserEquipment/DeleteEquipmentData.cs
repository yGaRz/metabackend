using NeoDaoBackend.Models.InventoryItems;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoDaoBackend.Models.UserEquipment;

public class DeleteEquipmentData
{
    [ValidGuid]
    public Guid UserId { get; set; }
    public SlotType SlotType { get; set; }
}