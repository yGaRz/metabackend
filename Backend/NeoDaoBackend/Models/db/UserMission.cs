using NeoDaoBackend.Models.Mission;

namespace NeoDaoBackend.Models.db;

public class UserMission
{
    public Guid UserId { get; set; }
    public string MissionId { get; set; } = null!;
    public MissionStatus Status { get; set; }
    public MissionType MissionType { get; set; }
    public DateTimeOffset? ExpireTime { get; set; }


    public virtual ICollection<MissionObjectives> Objectives { get; set; } = new List<MissionObjectives>();
    public virtual User User { get; set; } = null!;
}