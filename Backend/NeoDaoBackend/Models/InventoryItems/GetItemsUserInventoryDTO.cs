using Newtonsoft.Json;

namespace NeoDaoBackend.Models.InventoryItems;

public class GetItemsUserInventoryDTO
{
    public Guid ItemId { get; set; }
    public string UnrealItemId { get; set; }
    public string Properties { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public string? MetaforceNftId { get; set; }
    public string? NftDescription { get; set; }
    public bool? IsReservedForAuction { get; set; }
    public int Quantity { get; set; }
}