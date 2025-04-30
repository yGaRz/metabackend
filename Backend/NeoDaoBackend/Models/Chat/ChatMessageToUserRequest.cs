using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Chat;

public class ChatMessageToUserRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    public string Text { get; set; } = null!;
}
