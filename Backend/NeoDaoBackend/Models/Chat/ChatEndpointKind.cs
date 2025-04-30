namespace NeoDaoBackend.Models.Chat;

public enum ChatEndpointKind
{
    ConnectToChat,
    ChatMessage,
    GetChatMessages,
    MarkChannelAsRead,
    ConnectToChannel,
    DisconnectFromChannel
}
