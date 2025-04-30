using System.Text.Json.Serialization;
using NeoDaoBackend.Models.Customization;

namespace NeoDaoBackend.Models.db;

public class UserActiveCustomization
{
    public Guid UserId { get; set; }
    public string UnrealId { get; set; }
    public CustomizationSlotType SlotType { get; set; }
    
    
    public virtual User User { get; set; } = null!;
    public virtual UserAvailableCustomization AvailableCustomization { get; set; }
}

