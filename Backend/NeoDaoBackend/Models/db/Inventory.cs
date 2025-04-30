using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NeoDaoBackend.Models.db;

[Table("inventory")]
public class Inventory {
    [Key]
    [Required]
    [Column("inventory_id", TypeName = "uuid")]
    public Guid InventoryId { get; set; }

    [Required]
    [ForeignKey("User")]
    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }
    
    public virtual User User { get; set; }
    
    public virtual ICollection<Item> Items { get; set; }
    public virtual ICollection<Equipment> Equipments { get; set; }
}