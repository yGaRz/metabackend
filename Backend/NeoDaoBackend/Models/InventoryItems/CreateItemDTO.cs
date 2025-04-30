using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.InventoryItems;

public class CreateItemDTO
{
    [ValidNotEmptyString]
    public string UnrealItemId { get; set; }

    [ValidPositiveInteger]
    public int Quantity { get; set; }
    
    public DateTimeOffset? ExpirationDate { get; set; }
    [ValidJson]
    public string Properties { get; set; }
    
    public string? MetaforceNftId { get; set; }
    
    public string? NftDescription { get; set; }
}