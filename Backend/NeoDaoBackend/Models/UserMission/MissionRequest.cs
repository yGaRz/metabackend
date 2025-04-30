using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Mission;

public class MissionRequest
{
   [ValidGuid]
    public Guid UserId { get; set; }
    public UserMissionDTO Mission { get; set; } = null!;
}