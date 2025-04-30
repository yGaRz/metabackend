using System.Text.Json.Serialization;

namespace NeoDaoBackend.Models.db;

public class UserEmotion
{
    public Guid UserId { get; set; }
    public string UnrealId { get; set; }
    public DateTimeOffset Created { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}