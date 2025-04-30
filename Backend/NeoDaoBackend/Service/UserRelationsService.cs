using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserRelations;
using NeoDaoBackend.Models.UserRelations.Search;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;

namespace NeoDaoBackend.Service;

public class UserRelationsService : AbstractWebSocketService<UserRelationEndpointKind>
{
    private readonly IUserRelationsRepository _userRelationsRepository;
    private readonly IUserDataRepository _userDataRepository;

    public UserRelationsService(
        IValidationStorage validationStorage,
        ILogger<UserRelationsService> logger,
        IUserRelationsRepository userRelationsRepository,
        IUserDataRepository userDataRepository,
        JsonSerializerSettings jsonSerializerSettings,
        NotificationService notificationService
        ) : base(EndpointCategory.UserRelations, validationStorage,  logger, jsonSerializerSettings, notificationService)
    {
        _userRelationsRepository = userRelationsRepository;
        _userDataRepository = userDataRepository;
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, UserRelationEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case UserRelationEndpointKind.SendFriendInvite:
                outputMessage = await SendFriendInvite(user, data, ct);
                break;
            case UserRelationEndpointKind.SendConfirmFriendInvite:
                outputMessage = await SendConfirmFriendInvite(user, data, ct);
                break;
            case UserRelationEndpointKind.DeleteFriend:
                outputMessage = await DeleteFriend(user, data, ct);
                break;
            case UserRelationEndpointKind.DeleteFriendInvite:
                outputMessage = await DeleteFriendInvite(user, data, ct);
                break;
            case UserRelationEndpointKind.RemoveFriendRequest:
                outputMessage = await RemoveFriendRequest(user, data, ct);
                break;
            case UserRelationEndpointKind.SendIgnoreUser:
                outputMessage = await SendIgnoreUser(user, data, ct);
                break;
            case UserRelationEndpointKind.DeleteIgnoreUser:
                outputMessage = await DeleteIgnoreUser(user, data, ct);
                break;
            case UserRelationEndpointKind.GetBlockList:
                outputMessage = await GetBlockList(user, ct);
                break;
            case UserRelationEndpointKind.SearchUsers:
                outputMessage = await SearchUsers(user, data, ct);
                break;
            case UserRelationEndpointKind.GetFriends:
                outputMessage = await GetFriends(user, ct);
                break;
        }
        return outputMessage;
    }

    #region Actions
    public async Task<bool> IsFriend(Guid userA, Guid userB, CancellationToken ct)
    {
        var result = await _userRelationsRepository.GetRelationsStatus(userA, userB, ct);
        return result != null && result.FriendStatus == FriendStatus.Friend;
    }

    public async Task<BlockStatus> GetBlockStatusAsync(Guid senderId, Guid recipientId, CancellationToken ct)
    {
        var result = await _userRelationsRepository.GetRelationsStatus(senderId, recipientId, ct);
        return result != null ? result.BlockStatus : BlockStatus.None;
    }

    public async Task<List<UserIdName>> GetBlockListGuid(Guid user, CancellationToken ct)
    {
        return await _userRelationsRepository.GetBlockList(user, ct);
    }

    private async Task<IOutputMessageData> SendFriendInvite(NeoDaoUser user, string? data, CancellationToken ct)
    {
        UserIdRequest? request = JsonConvert.DeserializeObject<UserIdRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        UserRelation? userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation == null)
        {
            await _userRelationsRepository.CreateRelationsStatus(userIdA, userIdB, FriendStatus.Outgoing, BlockStatus.None, ct);
            await SendNotification(userIdA, userIdB, UserRelationEndpointKind.SendFriendInvite, ct);
            return response;
        }
        switch (userRelation!.FriendStatus)
        {
            case FriendStatus.Outgoing:
                _validationStorage.AddError(ErrorCode.FriendsInviteAlreadyExists, "Friends invite already exists");
                return response;
            case FriendStatus.Incoming:
                _validationStorage.AddError(ErrorCode.IncomingFriendsInviteExists, "The user sent you a friend invite");
                return response;
            case FriendStatus.Friend:
                _validationStorage.AddError(ErrorCode.AlreadyYourFriend, "User is already friend");
                return response;
        }
        if (userRelation.BlockStatus != BlockStatus.None)
        {
            AddBlockedError(userRelation.BlockStatus);
        }

        await _userRelationsRepository.SetFriendStatus(userIdA, userIdB, FriendStatus.Outgoing, ct);
        await SendNotification(userIdA, userIdB, UserRelationEndpointKind.SendFriendInvite, ct);
        _logger.LogWarning($"Found empty user relation row between userId {userIdA} and userId {userIdB}");
        return response;
    }

    private async Task<IOutputMessageData> SendConfirmFriendInvite(NeoDaoUser user, string? data, CancellationToken ct)
    {
        UserIdRequest? request = JsonConvert.DeserializeObject<UserIdRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        UserRelation? userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation == null)
        {
            _validationStorage.AddError(ErrorCode.IncomingInviteDoesNotExist, "Incoming invite does not exist");
            return response;
        }
        if (userRelation.BlockStatus != BlockStatus.None)
        {
            AddBlockedError(userRelation.BlockStatus);
            return response;
        }
        switch (userRelation.FriendStatus)
        {
            case FriendStatus.Friend:
                _validationStorage.AddError(ErrorCode.AlreadyYourFriend, "User is already your friend");
                return response;
            case FriendStatus.Incoming:
                await _userRelationsRepository.SetFriendStatus(userIdA, userIdB, FriendStatus.Friend, ct);
                await SendNotification(userIdA, userIdB, UserRelationEndpointKind.SendConfirmFriendInvite, ct);
                return response;
            default:
                _validationStorage.AddError(ErrorCode.IncomingInviteDoesNotExist, "Incoming invite does not exist");
                return response;
        }
    }

    private async Task<IOutputMessageData> DeleteFriend(NeoDaoUser user, string? data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<UserIdRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        UserRelation? userRelation;
        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation == null)
        {
            _validationStorage.AddError(ErrorCode.UserIsNotYourFriend, "User is not your friend");
            return response;
        }
        if (userRelation.BlockStatus != BlockStatus.None)
        {
            AddBlockedError(userRelation.BlockStatus);
            return response;
        }
        switch (userRelation.FriendStatus)
        {
            case FriendStatus.Friend:
                await _userRelationsRepository.DeleteRelations(userIdA, userIdB, ct);
                return response;
            default:
                _validationStorage.AddError(ErrorCode.UserIsNotYourFriend, "User is not your friend");
                return response;
        }
    }

    private async Task<IOutputMessageData> DeleteFriendInvite(NeoDaoUser user, string? data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<UserIdRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        UserRelation? userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation == null)
        {
            _validationStorage.AddError(ErrorCode.IncomingInviteDoesNotExist, "Friends invite does not exist");
            return response;
        }
        if (userRelation.BlockStatus != BlockStatus.None)
        {
            AddBlockedError(userRelation.BlockStatus);
            return response;
        }
        switch (userRelation.FriendStatus)
        {
            case FriendStatus.Friend:
                _validationStorage.AddError(ErrorCode.UserIsYourFriend, "User is your friend");
                return response;
            case FriendStatus.Incoming:
            case FriendStatus.None:
                await _userRelationsRepository.DeleteRelations(userIdA, userIdB, ct);
                return response;
            default:
                _validationStorage.AddError(ErrorCode.IncomingInviteDoesNotExist, "Friends invite does not exist");
                return response;
        }
    }

    private async Task<IOutputMessageData> RemoveFriendRequest(NeoDaoUser user, string? data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<UserIdRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        UserRelation? userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation == null)
        {
            _validationStorage.AddError(ErrorCode.OutgoingInviteDoesNotExist, "Friends invite does not exist");
            return response;
        }
        if (userRelation.BlockStatus != BlockStatus.None)
        {
            AddBlockedError(userRelation.BlockStatus);
            return response;
        }
        switch (userRelation.FriendStatus)
        {
            case FriendStatus.Outgoing:
            case FriendStatus.None:
                await _userRelationsRepository.DeleteRelations(userIdA, userIdB, ct);
                return response;
            case FriendStatus.Friend:
                _validationStorage.AddError(ErrorCode.UserIsYourFriend, "User is your friend");
                return response;
            default:
                _validationStorage.AddError(ErrorCode.OutgoingInviteDoesNotExist, "Friends invite does not exist");
                return response;
        }
    }

    private async Task<IOutputMessageData> SendIgnoreUser(NeoDaoUser user, string? data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<UserIdWithStatusRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        UserRelation? userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation == null)
        {
            await _userRelationsRepository.CreateRelationsStatus(userIdA, userIdB, FriendStatus.None, BlockStatus.OutgoingBlock, ct);
            return response;
        }
        if (userRelation.FriendStatus == FriendStatus.Friend && !request.isFriendDeletionEnabled)
        {
            _validationStorage.AddError(ErrorCode.UserIsYourFriend, "This user is your friend");
            return response;
        }
        switch (userRelation.BlockStatus)
        {
            case BlockStatus.None:
                await _userRelationsRepository.SetBlockStatus(userIdA, userIdB, BlockStatus.OutgoingBlock, ct);
                return response;
            case BlockStatus.IncomingBlock:
                await _userRelationsRepository.SetBlockStatus(userIdA, userIdB, BlockStatus.FullBlock, ct);
                return response;
            default:
                _validationStorage.AddError(ErrorCode.OutgoingBlockAlreadyExists, "You already blocked user");
                return response;
        }
    }

    private async Task<IOutputMessageData> DeleteIgnoreUser(NeoDaoUser user, string? data, CancellationToken ct)
    {
        var request = JsonConvert.DeserializeObject<UserIdRequest>(data!, _jsonSerializerSettings);
        UserIdResponse response = new UserIdResponse() { UserId = request!.UserId };
        bool isValid = await ValidateUserIdRequest(request, ct);
        if (!isValid)
        {
            return response;
        }

        Guid userIdA = user.UserId!.Value;
        Guid userIdB = request!.UserId;
        UserRelation? userRelation = await _userRelationsRepository.GetRelationsStatus(userIdA, userIdB, ct);
        if (userRelation != null)
        {
            if (userRelation.BlockStatus == BlockStatus.OutgoingBlock)
            {
                await _userRelationsRepository.DeleteRelations(userIdA, userIdB, ct);
                return response;
            }
            if (userRelation.BlockStatus == BlockStatus.FullBlock)
            {
                await _userRelationsRepository.SetBlockStatus(userIdA, userIdB, BlockStatus.IncomingBlock, ct);
                return response;
            }
        }
        _validationStorage.AddError(ErrorCode.OutgoingBlockDoesNotExist, "User is not in the ignore list");
        return response;
    }

    private async Task<IOutputMessageData?> GetBlockList(NeoDaoUser user, CancellationToken ct)
    {
        var blockList = await _userRelationsRepository.GetBlockList(user.UserId!.Value, ct);
        UserListResponse response = new UserListResponse() { Users = blockList };
        return response;
    }

    private async Task<IOutputMessageData?> SearchUsers(NeoDaoUser user, string? data, CancellationToken ct)
    {
        UserRelationsSearchRequest request = JsonConvert.DeserializeObject<UserRelationsSearchRequest>(data!, _jsonSerializerSettings)!;
        bool isValid = ValidationUtils.ValidatePaginationModel(request.Pagination, _validationStorage);
        if (!isValid)
        {
            return null;
        }
        List<UserIdName> result = await _userRelationsRepository.SearchUsers(user.UserId!.Value, request.Filter, request.Pagination, request.SortFields, ct);
        return new UserListResponse() { Users = result };
    }

    private async Task<IOutputMessageData?> GetFriends(NeoDaoUser user, CancellationToken ct)
    {
        List<UserIdName> result = await _userRelationsRepository.GetFriends(user.UserId!.Value, ct);
        return new UserListResponse() { Users = result };
    }

    private async Task SendNotification(Guid senderId, Guid recipientId, UserRelationEndpointKind endpointKind, CancellationToken ct)
    {
        RelationWSResponse relationWSResponse = new RelationWSResponse()
        {
            UserId = senderId,
        };
        await _notificationService.AddNotificationToSingleUser(recipientId, relationWSResponse, _endpointCategory.ToString(), endpointKind.ToString(), ct);
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateUserIdRequest(UserIdRequest? request, CancellationToken ct)
    {
        bool isValid = ValidationUtils.BasicWsValidation(request, _validationStorage);
        if (!isValid)
        {
            return false;
        }
        if (!await _userDataRepository.UserExists(request!.UserId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, request.UserId);
        }
        return _validationStorage.IsValid;
    }

    private void AddBlockedError(BlockStatus blockStatus)
    {
        switch (blockStatus)
        {
            case BlockStatus.OutgoingBlock:
                _validationStorage.AddError(ErrorCode.YouBlockedUser, "You blocked this user");
                break;
            case BlockStatus.IncomingBlock:
            case BlockStatus.FullBlock:
                _validationStorage.AddError(ErrorCode.UserBlockedYou, "The user has blocked communication with you");
                break;
        }
    }

    #endregion
}
