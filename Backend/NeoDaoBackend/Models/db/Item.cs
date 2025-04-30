using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoDaoBackend.Models.db;

[Table("items")]
public class Item
{
    public Guid ItemId { get; set; }
    public string UnrealItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTimeOffset? ExpirationDate { get; set; }
    public string Properties { get; set; }
    public Guid InventoryId { get; set; }
    public string? MetaforceNftId { get; set; }
    public string? NftDescription { get; set; }
    
    public virtual Inventory Inventory { get; set; }
}