using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Notification;

public class Notification
{
    public Guid? RecipientId { get; set; }
    public IOutputMessageData? Message { get; set; }
    public string EventCategory { get; set; } =null!;
    public string EventType { get; set; } = null!;
    public bool ToAllUser {  get; set; } = false;
}
