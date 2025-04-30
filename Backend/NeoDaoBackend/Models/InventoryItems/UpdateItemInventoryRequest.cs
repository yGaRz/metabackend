using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.InventoryItems;

public class UpdateItemInventoryRequest
{
    [ValidGuid]
    public Guid ItemId { get; set; }
    [ValidPositiveInteger]
    public int Quantity { get; set; }
    [ValidJson]
    public string Properties { get; set; }
}