using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoDaoBackend.Models.UserData;

public class GetAvatarSettingsData : IOutputMessageData
{
    [JsonProperty("gender")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AvatarGender? Gender { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
