using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.Chat;

public class ConnectToChannelRequest : IInputMessageData
{
    [JsonProperty("id")]
    public string ChannelId { get; set; } = "";
}
