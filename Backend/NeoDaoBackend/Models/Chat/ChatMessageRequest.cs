using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Chat;

public class ChatMessageRequest : IInputMessageData
{
    public string ChannelId { get; set; } = null!;
    public string Text { get; set; } = null!;
}
