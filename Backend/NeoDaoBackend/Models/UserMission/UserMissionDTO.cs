using NeoDaoBackend.Validation.Attributes;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.Mission;

public class UserMissionDTO
{
    [JsonProperty("id")]
    [ValidNotEmptyString]
    public string MissionId { get; set; } = null!;
    public MissionStatus Status { get; set; }
    [JsonProperty("type")]
    public MissionType MissionType { get; set; }
    public DateTimeOffset? ExpireTime { get; set; }
    public List<ObjectiveDTO> Objectives { get; set; } = null!;
}
