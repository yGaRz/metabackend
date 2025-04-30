using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Chat;

public class MarkChannelAsReadRequest : IInputMessageData
{
    public string ChannelId { get; set; } = null!;
    public long MessageId { get; set; }
}
