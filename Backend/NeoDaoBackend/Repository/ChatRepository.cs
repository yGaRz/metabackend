using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.Chat;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Repository.Interface;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Repository;

public class ChatRepository : IChatRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public ChatRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<long> AddChatMessage(string channelId, string message, CancellationToken ct)
    {
        return await AddChatMessage(channelId, null, message, ct);
    }

    public async Task<long> AddChatMessage(string channelId, Guid? senderId, string message, CancellationToken ct)
    {
        var chatMessage = new ChatMessage()
        {
            ChannelId = channelId,
            SenderId = senderId,
            Message = message
        };
        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync(ct);
        return chatMessage.MessageId;
    }

    public async Task<long> AddChatMessageWithUpdateChannel(string channelId, Guid senderId, Guid recipientId, string message, CancellationToken ct)
    {
        var chatMessage = new ChatMessage()
        {
            ChannelId = channelId,
            SenderId = senderId,
            Message = message
        };
        _dbContext.ChatMessages.Add(chatMessage);
        Channel editChannel = await _dbContext.Channels.Where(c => c.ChannelId == channelId).FirstAsync(ct);
        bool isARecipient = recipientId == editChannel.UserAId;
        editChannel.isReadB = isARecipient;
        editChannel.isReadA = !isARecipient;
        editChannel.Updated = DateTimeOffset.UtcNow;
        _dbContext.Channels.Update(editChannel);
        await _dbContext.SaveChangesAsync(ct);
        return chatMessage.MessageId;
    }

    public async Task CreateChannel(Guid? userA, Guid? userB, string channelId, CancellationToken ct)
    {
        Channel channel = new Channel()
        {
            ChannelId = channelId,
            UserAId = userA,
            UserBId = userB
        };
        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateChannelIsRead(string channelId, Guid userId, CancellationToken ct)
    {
        Channel editChannel = await _dbContext.Channels.Where(c => c.ChannelId == channelId).FirstAsync(ct);
        if (userId == editChannel.UserAId)
        {
            editChannel.isReadA = true;
        }
        else
        {
            editChannel.isReadB = true;
        }
        editChannel.Updated = DateTimeOffset.UtcNow;
        _dbContext.Channels.Update(editChannel);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> IsChannelExists(string channelId, CancellationToken ct)
    {
        return await _dbContext.Channels.Where(x => x.ChannelId == channelId).AnyAsync(ct);
    }

    public async Task UpdateGreetingMessage(string text, CancellationToken ct)
    {
        ChatMessage message = await _dbContext.ChatMessages.Where(x => x.ChannelId == GreetingsChatId).FirstAsync(ct);
        message.Message = text;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<ChatMessageResponse>> GetChatMessages(string channelId, ChannelType type, PaginationModel pagination,  CancellationToken ct)
    {
        return await _dbContext.ChatMessages.Where(x => x.ChannelId == channelId)
            .Include(x => x.Sender)
            .OrderByDescending(x => x.MessageId)
            .Skip(pagination.Offset)
            .Take(pagination.Count)
            .Select(x => new ChatMessageResponse()
            {
                ChannelId = x.ChannelId,
                Created = x.Created,
                MessageId = x.MessageId,
                Text = x.Message,
                SenderUserId = x.SenderId,
                ChannelType = type.ToString(),
                SenderName = x.Sender.UserName
            })
            .ToListAsync(ct);
    }

    public async Task<List<ChannelResponse>> GetPrivateChats(Guid userId, CancellationToken ct)
    {
        //Чат является приватным если у него обоих собеседников не нулевой userId
        return await _dbContext.Channels.Where(x => (x.UserAId == userId || x.UserBId == userId) && x.UserAId != null && x.UserBId != null)
            .Join(_dbContext.ChatMessages,
                channel => channel.ChannelId,
                message => message.ChannelId,
                (channel, message) => new { channel, message })
            .Select(
            x => new ChannelResponse()
            {
                ChannelId = x.channel.ChannelId,
                IsRead = x.channel.UserAId == userId ? x.channel.isReadA : x.channel.isReadB,
                Message = x.message.Message,
                SenderName = x.message.Sender.UserName,
                SenderId = x.message.SenderId!.Value.ToString(),
                ChannelType = ChannelType.Private.ToString(),
                InterlocutorName = x.channel.UserAId == userId ? x.channel.UserB!.UserName : x.channel.UserA!.UserName,
                MessageId = x.message.MessageId,
                MessageCreated = x.message.Created
            }).GroupBy(x => x.ChannelId).Select(g => g.OrderByDescending(l => l.MessageId).First()).ToListAsync(ct);
    }

    public async Task<string?> GetPreviousUserMessage(Guid userId, string channelId, CancellationToken ct)
    {
        var message = await _dbContext.ChatMessages
            .Where(x => x.SenderId == userId && x.ChannelId == channelId)
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync(ct);
        return message?.Message;
    }
}
