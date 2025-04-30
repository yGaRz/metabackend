namespace NeoDaoBackend.Models.db;

public class ChatMessage
{
    public long MessageId {  get; set; }
    public string ChannelId { get; set; } = null!;
    public Guid? SenderId { get; set; }
    public string Message {  get; set; }=null!;
    public DateTimeOffset Created { get; set; }

    public virtual User Sender { get; set; } = null!;
    public virtual Channel Channel { get; set; } = null!;
}
