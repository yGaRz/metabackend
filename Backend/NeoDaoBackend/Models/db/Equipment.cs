using NeoDaoBackend.Models.InventoryItems;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoDaoBackend.Models.db;

public class Equipment
{
    [Required]
    public Guid InventoryId { get; set; }
    
    [Required]
    public SlotType SlotType { get; set; }
    
    [Required]
    public Guid ItemId { get; set; }
    
    [ForeignKey("InventoryId")]
    public virtual Inventory Inventory { get; set; }
    
    [ForeignKey("ItemId")]
    public virtual Item Item { get; set; }
}
