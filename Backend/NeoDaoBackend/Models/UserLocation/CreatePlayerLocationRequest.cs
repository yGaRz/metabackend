using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.UserLocation;

public class CreatePlayerLocationRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }

    public PlayerLocationDTO Location { get; set; } = null!;
}