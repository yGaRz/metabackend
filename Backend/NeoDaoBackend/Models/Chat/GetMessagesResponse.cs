using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Chat;

public class GetMessagesResponse:IOutputMessageData
{
    public List<ChatMessageResponse> Messages = null!;
}
