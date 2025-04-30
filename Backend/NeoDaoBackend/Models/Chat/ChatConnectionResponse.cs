using NeoDaoBackend.Models.UserRelations.Search;
using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Chat;

public class ChatConnectionResponse : IOutputMessageData
{
    public ChatMessageResponse GreetingMessage { get; set; } = null!;
    public List<ChatMessageResponse> GlobalMessages { get; set; } = null!;
    public List<ChannelResponse> PrivateChatList { get; set; } = null!;
    public List<UserIdName> BlockedUserIds { get; set; } = null!;
}
