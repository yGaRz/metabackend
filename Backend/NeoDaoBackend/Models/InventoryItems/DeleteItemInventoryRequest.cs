using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.InventoryItems;

public class DeleteItemInventoryRequest
{
    
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidGuid]
    public Guid ItemId { get; set; }
}