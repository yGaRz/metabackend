using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Chat;

public class ChatMessageResponse : IOutputMessageData
{
    public long MessageId { get; set; }
    public string? SenderName { get; set; }
    public Guid? SenderUserId { get; set; }
    public string Text { get; set; } = null!;
    public string ChannelId { get; set; } = null!;
    public string ChannelType {  get; set; }=null!;
    public DateTimeOffset Created { get; set; }
}
