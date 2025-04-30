namespace NeoDaoBackend.Models.Chat;

public class ChannelResponse
{
    public string ChannelId { get; set; } = null!;
    public string ChannelType {  get; set; }=null!;
    public string SenderName { get; set; } = null!;
    public string SenderId { get; set; }=null!;
    public string InterlocutorName {  get; set; } = null!;
    public bool IsRead { get; set; }
    public string Message { get; set; } = null!;
    public long? MessageId {  get; set; }
    public DateTimeOffset? MessageCreated { get; set; }
}
