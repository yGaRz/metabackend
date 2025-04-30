using System.Text.Json.Serialization;

namespace NeoDaoBackend.Models.db;

public class UserAvailableCustomization
{
    public Guid UserId { get; set; }
    public string UnrealId { get; set; }
    public string Properties { get; set; }
    public DateTimeOffset Created { get; set; }
    
    public virtual User User { get; set; } = null!;
}