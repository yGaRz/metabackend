using NeoDaoBackend.Models.Chat;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository.Interface;

public interface IChatRepository
{
    Task<long> AddChatMessage(string channelId, string message, CancellationToken ct);
    Task<long> AddChatMessage(string channelId, Guid? senderId, string message, CancellationToken ct);
    Task<long> AddChatMessageWithUpdateChannel(string channelId, Guid senderId, Guid recipientId, string message, CancellationToken ct);
    Task<List<ChatMessageResponse>> GetChatMessages(string channelId, ChannelType type, PaginationModel pagination, CancellationToken ct);
    Task<bool> IsChannelExists(string channelId, CancellationToken ct);
    Task UpdateGreetingMessage(string message, CancellationToken ct);
    Task CreateChannel(Guid? userA, Guid? userB, string channelId, CancellationToken ct);
    Task<List<ChannelResponse>> GetPrivateChats(Guid userId, CancellationToken ct);
    Task UpdateChannelIsRead(string channelId, Guid senderId, CancellationToken ct);
    Task<string?> GetPreviousUserMessage(Guid userId, string channelId, CancellationToken ct);
}
