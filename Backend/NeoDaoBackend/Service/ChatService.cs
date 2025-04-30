using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Chat;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.UserRelations;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Service;

public class ChatService : AbstractWebSocketService<ChatEndpointKind>
{
    private readonly UserRelationsService _userRelationsService;
    private readonly IChatRepository _chatServiceRepository;
    private readonly UserDataService _userDataService;

    private static ConcurrentDictionary<string, List<Guid>> ChatList { get; set; } = new ConcurrentDictionary<string, List<Guid>>();
    private static object ChatListLock = new object();
    private readonly ProfanityFilter.ProfanityFilter filter;

    public ChatService(IValidationStorage validationStorage,
        JsonSerializerSettings jsonSerializerSettings,
        ILogger<ChatService> logger,
        UserRelationsService userRelationsService,
        IChatRepository chatServiceRepository,
        UserDataService userDataService,
        NotificationService notificationService) :
        base(EndpointCategory.Chat, validationStorage, logger, jsonSerializerSettings, notificationService)
    {
        _chatServiceRepository = chatServiceRepository;
        _userDataService = userDataService;
        _userRelationsService = userRelationsService;
        filter = new ProfanityFilter.ProfanityFilter();
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, ChatEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case ChatEndpointKind.ConnectToChat:
                outputMessage = await ConnectToChat(user, ct);
                break;
            case ChatEndpointKind.ChatMessage:
                await ChatMessage(user, data!, ct);
                break;
            case ChatEndpointKind.GetChatMessages:
                outputMessage = await GetChatMessages(user, data!, ct);
                break;
            case ChatEndpointKind.MarkChannelAsRead:
                await MarkChannelAsRead(user, data!, ct);
                break;
            case ChatEndpointKind.ConnectToChannel:
                await ConnectToChannel(user, data!, ct);
                break;
            case ChatEndpointKind.DisconnectFromChannel:
                await DisconnectFromChannel(user, data!, ct);
                break;
        }
        return outputMessage;
    }

    #region Actions     

    #region ProcessMessage
    private async Task<IOutputMessageData?> GetChatMessages(NeoDaoUser user, string data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<GetMessagesRequest>(data!);
        ChannelType type = await ValidateGetMessageFromChannelStringAsync(request!.ChannelId, user.UserId!.Value, ct);
        if (!_validationStorage.IsValid)
        {
            _logger.LogWarning($"Validating error while getting chat messages. Data={data}");
            return null;
        }
        if (type == ChannelType.Private)
        {
            await _chatServiceRepository.UpdateChannelIsRead(request!.ChannelId, user.UserId!.Value, ct);
        }
        return new GetMessagesResponse()
        {
            Messages = await _chatServiceRepository.GetChatMessages(request!.ChannelId, type, request.Pagination, ct)
        };
    }

    private async Task ChatMessage(NeoDaoUser user, string data, CancellationToken ct)
    {
        ChatMessageRequest? chatMessageData = JsonConvert.DeserializeObject<ChatMessageRequest>(data!);
        bool isValid = await ValidateChatMessageAsync(chatMessageData, user.UserId!.Value, ct);
        if (!isValid)
        {
            _logger.LogWarning($"Validating error in processing chat message. Data={chatMessageData}");
            return;
        }
        _logger.LogInformation($"Received correct message from user {user.UserId}: {chatMessageData!.Text}");

        chatMessageData.Text = filter.CensorString(chatMessageData.Text);

        Guid? userId1;
        Guid? userId2;
        switch (GetChannelType(chatMessageData.ChannelId, out userId1, out userId2))
        {
            case ChannelType.Global:
                {
                    long messageId = await _chatServiceRepository.AddChatMessage(chatMessageData.ChannelId, user.UserId!.Value, chatMessageData.Text, ct);
                    ChatMessageResponse chatMessageResponse = new ChatMessageResponse()
                    {
                        SenderName = user.UserName!,
                        Created = DateTimeOffset.Now,
                        ChannelId = chatMessageData.ChannelId,
                        MessageId = messageId,
                        Text = chatMessageData.Text,
                        ChannelType = ChannelType.Global.ToString(),
                        SenderUserId = user.UserId!.Value
                    };
                    await _notificationService.AddNotificationToAllUsers(chatMessageResponse, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
                    break;
                }
            case ChannelType.Stream:
                {
                    long messageId = await _chatServiceRepository.AddChatMessage(chatMessageData.ChannelId, user.UserId!.Value, chatMessageData.Text, ct);
                    ChatMessageResponse chatMessageResponse = new ChatMessageResponse()
                    {
                        SenderName = user.UserName!,
                        Created = DateTimeOffset.Now,
                        ChannelId = chatMessageData.ChannelId,
                        MessageId = messageId,
                        Text = chatMessageData.Text,
                        ChannelType = ChannelType.Stream.ToString(),
                        SenderUserId = user.UserId!.Value
                    };
                    List<Guid> users = GetUsersFromChat(chatMessageData.ChannelId);
                    foreach (Guid userId in users)
                    {
                        await _notificationService.AddNotificationToSingleUser(userId, chatMessageResponse, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
                    }
                    break;
                }
            case ChannelType.Group:
                {
                    long messageId = await _chatServiceRepository.AddChatMessage(chatMessageData.ChannelId, user.UserId!.Value, chatMessageData.Text, ct);
                    ChatMessageResponse chatMessageResponse = new ChatMessageResponse()
                    {
                        SenderName = user.UserName!,
                        Created = DateTimeOffset.Now,
                        ChannelId = chatMessageData.ChannelId,
                        MessageId = messageId,
                        Text = chatMessageData.Text,
                        ChannelType = ChannelType.Group.ToString(),
                        SenderUserId = user.UserId!.Value
                    };
                    List<Guid> users = GetUsersFromChat(chatMessageData.ChannelId);
                    foreach (Guid userId in users)
                    {
                        await _notificationService.AddNotificationToSingleUser(userId, chatMessageResponse, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
                    }
                    break;
                }
            case ChannelType.Private:
                {
                    if (!await _chatServiceRepository.IsChannelExists(chatMessageData.ChannelId, CancellationToken.None))
                    {
                        if (userId1 < userId2)
                        {
                            await _chatServiceRepository.CreateChannel(userId1!.Value, userId2!.Value, chatMessageData.ChannelId, ct);
                        }
                        else
                        {
                            await _chatServiceRepository.CreateChannel(userId2!.Value, userId1!.Value, chatMessageData.ChannelId, ct);
                        }
                    }
                    Guid recipientId = user.UserId == userId1!.Value ? userId2!.Value : userId1!.Value;
                    Guid senderId = user.UserId.Value;
                    long messageId = await _chatServiceRepository.AddChatMessageWithUpdateChannel(chatMessageData.ChannelId, senderId, recipientId, chatMessageData.Text, ct);
                    ChatMessageResponse response = new ChatMessageResponse()
                    {
                        ChannelId = chatMessageData.ChannelId,
                        Text = chatMessageData.Text,
                        SenderUserId = senderId,
                        SenderName = user.UserName,
                        ChannelType = ChannelType.Private.ToString(),
                        Created = DateTimeOffset.Now,
                        MessageId = messageId
                    };
                    await _notificationService.AddNotificationToSingleUser(recipientId, response, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
                    await _notificationService.AddNotificationToSingleUser(user.UserId.Value, response, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
                    break;
                }
        }
    }

    private async Task<IOutputMessageData?> ConnectToChat(NeoDaoUser user, CancellationToken ct)
    {
        _logger.LogInformation($"Connecting to chat. ID={user.UserId}");
        ChatConnectionResponse chatConnectionResponse = new ChatConnectionResponse()
        {
            GreetingMessage = (await _chatServiceRepository.GetChatMessages(GreetingsChatId, ChannelType.Greeting, new PaginationModel() { Offset = 0, Count = 1 }, ct))[0],
            BlockedUserIds = await _userRelationsService.GetBlockListGuid(user.UserId!.Value, ct),
            GlobalMessages = await _chatServiceRepository.GetChatMessages(GlobalChatId, ChannelType.Global, new PaginationModel() { Offset = 0, Count = 15 }, ct),
            PrivateChatList = await _chatServiceRepository.GetPrivateChats(user.UserId!.Value, ct)
        };
        return chatConnectionResponse;
    }

    private async Task MarkChannelAsRead(NeoDaoUser user, string data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<MarkChannelAsReadRequest>(data!);
        ChannelType type = await ValidateGetMessageFromChannelStringAsync(request!.ChannelId, user.UserId!.Value, ct);
        if (!_validationStorage.IsValid)
        {
            _logger.LogWarning($"Validating error in processing chat message. Data={data}");
        }
        await _chatServiceRepository.UpdateChannelIsRead(request!.ChannelId, user.UserId!.Value, ct);
    }

    private async Task ConnectToChannel(NeoDaoUser user, string data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<ConnectToChannelRequest>(data!);
        //TODO: сделать проверку на принадлежность пользователя к чату в рамках Stream/Group
        bool isValid = await ValidateChannelStringAsync(request!.ChannelId, ct);
        if (!isValid)
        {
            _logger.LogWarning($"Validating error in processing chat message. Data={data}");
        }

        await AddUserToChat(request!.ChannelId, user.UserId!.Value, ct);
    }

    private async Task DisconnectFromChannel(NeoDaoUser user, string data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<ConnectToChannelRequest>(data!);
        //TODO: сделать проверку на принадлежность пользователя к чату в рамках Stream/Group
        bool isValid = await ValidateChannelStringAsync(request!.ChannelId, ct);
        if (!isValid)
        {
            _logger.LogWarning($"Validating error in processing chat message. Data={data}");
        }

        ChatList.TryGetValue(request.ChannelId, out List<Guid>? userList);
        if (userList != null)
        {
            lock (ChatListLock)
            {
                userList.Remove(user.UserId!.Value);
            }
        }
    }

    #endregion

    #region Stream/Group
    public async Task AddUserToChat(string channelId, Guid userId, CancellationToken ct)
    {
        //Признаю, что это порнография и костыль.
        await CreateChat(channelId, ct);
        List<Guid> userList = GetUsersFromChat(channelId);
        if (userList != null && !userList.Contains(userId))
        {
            lock (ChatListLock)
            {
                userList.Add(userId);
            }
        }
    }

    public bool IsChatExists(string channelId)
    {
        return ChatList.ContainsKey(channelId);
    }

    public void RemoveUserFromChat(string channelId, Guid userId)
    {
        List<Guid> userList = GetUsersFromChat(channelId);
        if (userList != null && userList.Contains(userId))
        {
            lock (ChatListLock)
            {
                userList.Remove(userId);
            }
        }
    }

    public async Task CreateChat(string channelId, CancellationToken ct)
    {
        //TODO: Логика такая что в базе может быть создан канал, но на беке его еще не создано, после рестарта и т.д.
        //Логику создания и удаления необходимо будет править после доработки функционала стримов
        //Сейчас это мвп версия для демо
        ChatList.TryAdd(channelId, new List<Guid>());
        if (!await _chatServiceRepository.IsChannelExists(channelId, ct))
        {
            await _chatServiceRepository.CreateChannel(null, null, channelId, ct);
        }
    }

    private List<Guid> GetUsersFromChat(string channelId)
    {
        List<Guid>? userList;
        ChatList.TryGetValue(channelId, out userList);
        return userList!;
    }
    #endregion

    public async Task<bool> SendAdminMessage(string text, CancellationToken ct)
    {
        bool isValid = ValidationBody(text);
        if (!isValid)
        {
            return false;
        }

        ChatMessageResponse chatMessage = new ChatMessageResponse()
        {
            Created = DateTimeOffset.UtcNow,
            Text = text,
            SenderUserId = null,
            ChannelId = AdminChatId,
            ChannelType = ChannelType.Admin.ToString(),
            MessageId = await _chatServiceRepository.AddChatMessage(AdminChatId, text, ct),
            SenderName = AdminChatId,
        };
        await _notificationService.AddNotificationToAllUsers(chatMessage, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
        return true;
    }

    public async Task<bool> SendMetaMessage(ChatMessageToUserRequest request, CancellationToken ct)
    {
        bool isValid = ValidationBody(request.Text) && await _userDataService.ValidateUser(request.UserId, ct);
        if (!isValid)
        {
            return false;
        }
        string channelId = $"meta:{request.UserId}";
        if (!await _chatServiceRepository.IsChannelExists(channelId, CancellationToken.None))
        {
            await _chatServiceRepository.CreateChannel(request.UserId, null, channelId, ct);
        }

        ChatMessageResponse chatMessage = new ChatMessageResponse()
        {
            Created = DateTimeOffset.UtcNow,
            Text = request.Text,
            ChannelId = channelId,
            ChannelType = ChannelType.Meta.ToString(),
            MessageId = await _chatServiceRepository.AddChatMessage(channelId, request.Text, ct),
            SenderName = ChannelType.Meta.ToString(),
        };
        await _notificationService.AddNotificationToAllUsers(chatMessage, _endpointCategory.ToString(), ChatEndpointKind.ChatMessage.ToString(), ct);
        return true;
    }

    public async Task<ChatMessageResponse?> GetGreetingMessage(CancellationToken ct)
    {
        return (await _chatServiceRepository.GetChatMessages(GreetingsChatId, ChannelType.Greeting,
            new PaginationModel { Offset = 0, Count = 1 }, ct))[0];
    }

    public async Task<bool> UpdateGreetingMessage(string message, CancellationToken ct)
    {
        bool isValid = ValidationBody(message);
        if (!isValid)
        {
            return false;
        }
        await _chatServiceRepository.UpdateGreetingMessage(message, ct);
        return true;
    }

    public void RemoveUserFromAllChat(NeoDaoUser user)
    {
        foreach (var chat in ChatList.Values)
        {
            if (chat.Contains(user.UserId!.Value))
            {
                lock (ChatListLock)
                {
                    chat.Remove(user.UserId!.Value);
                }
            }
        }
    }

    private ChannelType GetChannelType(string channelId, out Guid? id1, out Guid? id2)
    {
        id1 = null;
        id2 = null;
        if (channelId == GlobalChatId)
        {
            return ChannelType.Global;
        }
        if (channelId.StartsWith("stream:"))
        {
            return ChannelType.Stream;
        }
        if (channelId.StartsWith("group:"))
        {
            return ChannelType.Group;
        }
        if (channelId.StartsWith("meta:"))
        {
            var ids = channelId.Split(':');
            id1 = new Guid(ids[1]);
            return ChannelType.Meta;
        }
        Regex PrivateChannelIdRegex = new Regex(PrivateChannelIdRegexString);
        if (PrivateChannelIdRegex.IsMatch(channelId))
        {
            var ids = channelId.Split(':');
            id1 = new Guid(ids[0]);
            id2 = new Guid(ids[1]);
            return ChannelType.Private;
        }
        return ChannelType.Error;
    }
    #endregion

    #region Validation
    private async Task<bool> ValidateChatMessageAsync(ChatMessageRequest? chatMessageData, Guid senderId, CancellationToken ct)
    {
        if (chatMessageData == null)
        {
            ValidationUtils.AddEmptyDataError(_validationStorage);
            return false;
        }
        ValidationBody(chatMessageData.Text);
        Guid? userId1;
        Guid? userId2;
        var channelType = GetChannelType(chatMessageData.ChannelId, out userId1, out userId2);
        switch (channelType)
        {
            case ChannelType.Private:
                {
                    if (!await _chatServiceRepository.IsChannelExists(chatMessageData.ChannelId, ct))
                    {
                        await _userDataService.ValidateUser(userId1!.Value, ct);
                        await _userDataService.ValidateUser(userId2!.Value, ct);
                        if (userId1 >= userId2)
                        {
                            _validationStorage.AddError(ErrorCode.UnknownChat, $"Chat id is not correct, Id1>=Id2");
                        }
                    }
                    var recipientId = senderId == userId1 ? userId2 : userId1;
                    switch (await _userRelationsService.GetBlockStatusAsync(senderId, recipientId!.Value, ct))
                    {
                        case BlockStatus.IncomingBlock:
                        case BlockStatus.FullBlock:
                            _validationStorage.AddError(ErrorCode.UserBlockedYou, $"User has blocked private messages from you");
                            break;
                        case BlockStatus.OutgoingBlock:
                            _validationStorage.AddError(ErrorCode.YouBlockedUser, $"You has blocked user");
                            break;
                    }
                    if (senderId != userId1 && senderId != userId2)
                    {
                        _validationStorage.AddError(ErrorCode.WrongChat, $"The user is not a chat participant");
                    }
                    break;
                }
            case ChannelType.Stream:
                {
                    if (!ChatList.ContainsKey(chatMessageData.ChannelId))
                    {
                        await CreateChat(chatMessageData.ChannelId, ct);
                    }
                    if (!GetUsersFromChat(chatMessageData.ChannelId).Contains(senderId))
                    {
                        _validationStorage.AddError(ErrorCode.UnknownChat, $"User does not have access to this chat");
                    }
                    break;
                }
            case ChannelType.Error:
                _validationStorage.AddError(ErrorCode.UnknownChat, $"Chat with id {chatMessageData.ChannelId} does not exist");
                break;
        }
        //await ValidatePreviousMessage(senderId, chatMessageData.ChannelId, chatMessageData.Text, ct);

        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateChannelStringAsync(string channelId, CancellationToken ct)
    {
        if (!await _chatServiceRepository.IsChannelExists(channelId, ct))
        {
            _validationStorage.AddError(ErrorCode.UnknownChat, $"Chat with id {channelId} does not exist");
        }
        return _validationStorage.IsValid;
    }

    private async Task<ChannelType> ValidateGetMessageFromChannelStringAsync(string channelId,Guid userId, CancellationToken ct)
    {
        if (!await _chatServiceRepository.IsChannelExists(channelId, ct))
        {
            _validationStorage.AddError(ErrorCode.UnknownChat, $"Chat with id {channelId} does not exist");
        }
        ChannelType type = GetChannelType(channelId, out Guid? id1, out Guid? id2);
        switch (type)
        {
            case ChannelType.Private:
                if (!(id1 == userId || id2 == userId))
                {
                    _validationStorage.AddError(ErrorCode.WrongChat, $"The user is not a chat participant");
                }
                break;
            case ChannelType.Meta:
                if(id1!=userId)
                {
                    _validationStorage.AddError(ErrorCode.WrongChat, $"The user is not a chat participant");
                }
                break;
        }
        return type;
    }

    //Убираем проверку предыдущего сообщения.
    //private async Task ValidatePreviousMessage(Guid userId, string channelId, string text, CancellationToken ct)
    //{
    //    string? previousMessage = await _chatServiceRepository.GetPreviousUserMessage(userId, channelId, ct);
    //    if (text == previousMessage)
    //    {
    //        _validationStorage.AddError(ErrorCode.EqualPreviousMessage, $"The message: '{text}' is equal to the previous message.");
    //    }
    //}

    private bool ValidationBody(string message)
    {
        return true;
        //Возможно будем проверять тело сообщения на предмет наличия гиперссылок
        //Убираем валидацию языков, те которые поддерживаются будут корректно отображаться, остальные не будут отображаться и все.
        //HashSet<char> forbiddenCharacters = [];
        //foreach (char c in message)
        //{
        //    if (!char.IsAscii(c))
        //    {
        //        forbiddenCharacters.Add(c);
        //    }
        //}
        //if (forbiddenCharacters.Any())
        //{
        //    _validationStorage.AddError(ErrorCode.ForbiddenCharacters,
        //        $"Message contains forbidden characters: {FormatEnumerable(forbiddenCharacters, "")}");
        //}

        //return _validationStorage.IsValid;
    }
    #endregion
}