using NeoDaoBackend.Models.Mission;

namespace NeoDaoBackend.Models.db;

public class MissionObjectives
{
    public Guid UserId { get; set; }
    public string MissionId { get; set; } = null!;
    public string ObjectiveId { get; set; } = null!;
    public MissionStatus Status { get; set; }
    public string Metadata { get; set; } = null!;

    public virtual UserMission UserMission { get; set; } = null!;
}
