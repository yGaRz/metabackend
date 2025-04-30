namespace NeoDaoBackend.Models.Mission;

public class MissionsResponse
{
    public IEnumerable<UserMissionDTO> Missions { get; set; } = null!;
}
