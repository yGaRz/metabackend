using NeoDaoBackend.Validation.Attributes;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.Mission;

public class ObjectiveDTO
{
    [JsonProperty("id")]
    [ValidNotEmptyString]
    public string ObjectiveId { get; set; } = null!;
    public MissionStatus Status { get; set; }
    [ValidNotEmptyString]
    public string Metadata { get; set; } = null!;
}
