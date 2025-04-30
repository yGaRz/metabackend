using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.InventoryItems;

public class CreateItemPlayerInventoryRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }

    public CreateItemDTO item { get; set; }
}