using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoDaoBackend.Models.Matchmaking;

public class GetTicketAssignmentRequestData : IInputMessageData
{
    [JsonProperty("serverType", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public GameServerType ServerType { get; set; }

    public string TicketId { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
