using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Group;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Service;

public class GroupService : AbstractWebSocketService<GroupEndpointKind>
{
    private readonly UserDataService _userDataService;
    private readonly UserRelationsService _userRelationsService;
    private readonly IGroupRepository _groupRepository;
    private readonly ChatService _chatService;

    public GroupService(ILogger<GroupService> logger,
        JsonSerializerSettings jsonSerializerSettings,
        IValidationStorage validationStorage,
        IGroupRepository groupRepository,
        UserRelationsService userRelationsService,
        UserDataService userDataService,
        NotificationService notificationService,
        ChatService chatService) :
        base(EndpointCategory.Group, validationStorage, logger, jsonSerializerSettings, notificationService)
    {
        _groupRepository = groupRepository;
        _userRelationsService = userRelationsService;
        _userDataService = userDataService;
        _chatService = chatService;
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, GroupEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case GroupEndpointKind.GetGroupData:
                outputMessage = await GetGroupData(user.UserId!.Value, ct);
                break;
            case GroupEndpointKind.CreateGroup:
                outputMessage = await CreateGroup(user.UserId!.Value, ct);
                break;
            case GroupEndpointKind.ExitGroup:
                outputMessage = await ExitGroup(user, ct);
                break;
            case GroupEndpointKind.InviteFriendToGroup:
                await InviteFriendToGroup(user, data, ct);
                break;
            case GroupEndpointKind.AcceptInvitationToGroup:
                await AcceptInvitationToGroup(user, data, ct);
                break;
            case GroupEndpointKind.JoinFriendGroup:
                await JoinFriendGroup(user, data, ct);
                break;
            case GroupEndpointKind.AcceptUserJoiningGroup:
                await AcceptUserJoiningGroup(user.UserId!.Value, data, ct);
                break;
        }
        return outputMessage;
    }

    #region Actions
    public async Task FinalizeUserGroup(Guid userId, string userName, CancellationToken ct)
    {
        UserGroup? activeGroup = await _groupRepository.GetActiveUserGroup(userId, ct);
        if (activeGroup != null)
        {
            _logger.LogInformation($"Active group {activeGroup.GroupId} found for user {userId}. Removing user from the group");
            await DoExitGroup(userId, userName, ct);
        }
        int deletedUserGroupsCount = await _groupRepository.DeleteUserGroupBatch(userId, ct);
        _logger.LogInformation($"Deleted {deletedUserGroupsCount} additional userGroups where user {userId} was not active");
    }

    private async Task<IOutputMessageData?> GetGroupData(Guid userId, CancellationToken ct)
    {
        IEnumerable<Guid> userGroupIds = await _groupRepository.GetAllGroupIdsForUser(userId, ct);
        var usersByGroupId = await _groupRepository.GetUsersByGroups(userGroupIds, ct);
        List<UserGroupDTO> userGroups = usersByGroupId.Select(entry => new UserGroupDTO()
        {
            GroupId = entry.Key,
            Users = [.. entry.Value],
        }).ToList();
        return new UserGroupData { userGroups = userGroups };
    }

    private async Task<IOutputMessageData?> CreateGroup(Guid userId, CancellationToken ct)
    {
        bool isValid = await ValidateCreateGroup(userId, ct);
        if (!isValid)
        {
            return null;
        }

        Guid groupId = await _groupRepository.CreateGroup(userId, ct);
        await _chatService.CreateChat($"group:{groupId}", ct);
        await _chatService.AddUserToChat($"group:{groupId}", userId, ct);
        return new CreateGroupResponse { GroupId = groupId };
    }

    private async Task<IOutputMessageData?> ExitGroup(NeoDaoUser user, CancellationToken ct)
    {
        bool isValid = await ValidateExitGroup(user.UserId!.Value, ct);
        if (!isValid)
        {
            return null;
        }
        return await DoExitGroup(user.UserId!.Value, user.UserName!, ct);
    }

    private async Task<IOutputMessageData?> DoExitGroup(Guid userId, string userName, CancellationToken ct)
    {
        UserGroup activeUserGroup = (await _groupRepository.GetActiveUserGroup(userId, ct))!;
        Group group = await _groupRepository.GetById(activeUserGroup.GroupId, ct);
        var response = new YouLeavedGroupResponse { GroupId = group.GroupId };
        if (group.LeaderUserId.Equals(userId))
        {
            _groupRepository.DeleteUserGroupNoSave(activeUserGroup);
            List<UserGroup> usersInGroup = await _groupRepository.GetActiveGroupMembers(group.GroupId, ct);
            if (usersInGroup.Count <= 1)
            {
                _logger.LogInformation($"User {userId} was the only active member of the group {group.GroupId}. Deleting the whole group");
                await _groupRepository.DeleteGroup(group, ct);
                response.Deleted = true;
            }
            else
            {
                Guid newLeaderId = usersInGroup
                    .Where(ug => !userId.Equals(ug.UserId))
                    .Select(ug => ug.UserId)
                    .First();
                group.LeaderUserId = newLeaderId;
                _logger.LogInformation($"User {userId} was the leader of the group {group.GroupId}. New leader is user {newLeaderId}");
                await _groupRepository.UpdateGroup(group, ct);
                // Уведомление новому лидеру группы, что он теперь лидер
                await SendYouAreTheLeaderNowMessageToGroupLeader(newLeaderId, ct);
                response.Deleted = false;
            }
        }
        else
        {
            _logger.LogInformation($"User {userId} was an ordinary member of the group {group.GroupId}. Deleting only UserGroup");
            await _groupRepository.DeleteUserGroup(activeUserGroup, ct);
            response.Deleted = true;
        }

        // Уведомление остальным участникам группы, что данный юзер вышел из группы
        await SendUserLeftGroupMessageToGroupMembers(userId, userName, activeUserGroup.GroupId, ct);
        _chatService.RemoveUserFromChat($"group:{group.GroupId}", userId);
        return response;
    }

    private async Task InviteFriendToGroup(NeoDaoUser user, string? data, CancellationToken ct)
    {
        Guid userId = user.UserId!.Value;
        InviteFriendToGroupRequest? request = JsonConvert.DeserializeObject<InviteFriendToGroupRequest>(data!, _jsonSerializerSettings)!;
        bool isValid = await ValidateInviteFriendToGroup(userId, request, ct);
        if (!isValid)
        {
            return;
        }

        UserGroup userGroup = new()
        {
            UserId = request.FriendUserId,
            GroupId = request.GroupId,
            UserStatus = UserGroupStatus.Invited,
        };
        await _groupRepository.CreateUserGroup(userGroup, ct);

        // Уведомление юзеру, что его пригласили в группу
        GroupInvitationMessageToUser messageData = new()
        {
            GroupId = request.GroupId,
            GroupLeaderUserId = userId,
            GroupLeaderUserName = user.UserName!
        };
        await SendGroupInvitationMessageToUser(request.FriendUserId, messageData, ct);
    }

    private async Task<IOutputMessageData?> AcceptInvitationToGroup(NeoDaoUser user, string? data, CancellationToken ct)
    {
        Guid userId = user.UserId!.Value;
        AcceptInvitationToGroupRequest? request = JsonConvert.DeserializeObject<AcceptInvitationToGroupRequest>(data!, _jsonSerializerSettings)!;
        bool isValid = await ValidateAcceptInvitationToGroup(userId, request, ct);
        if (!isValid)
        {
            return null;
        }

        UserGroup userGroup = (await GetUserGroup(userId, request.GroupId, ct))!;
        YouJoinedGroupMessage messageData = new()
        {
            GroupId = request.GroupId
        };
        if (request.IsAccepted)
        {
            userGroup.UserStatus = UserGroupStatus.Active;
            await _groupRepository.UpdateUserGroup(userGroup, ct);

            await _chatService.AddUserToChat($"group:{request.GroupId}", userId, ct);
            // Уведомление остальным членам группы, что появился новый участник
            await SendNewUserJoinedGroupMessageToGroupMembers(userId, user.UserName!, request.GroupId, ct);
            messageData.IsJoined = true;

            var notActiveGroupList = await _groupRepository.GetNotActiveUserGroup(userId, ct);
            foreach (var group in notActiveGroupList)
            {
                await SendUserDeclineGroupJoinMessageToGroupLeader(userId, user.UserName!, group.GroupId, ct);
            }
            await _groupRepository.DeleteNotActiveUserGroups(userId, ct);
        }
        else
        {
            await SendUserDeclineGroupJoinMessageToGroupLeader(userId, user.UserName!, userGroup.GroupId, ct);
            await _groupRepository.DeleteUserGroup(userGroup, ct);
            messageData.IsJoined = false;
        }
        return messageData;
    }

    private async Task JoinFriendGroup(NeoDaoUser user, string? data, CancellationToken ct)
    {
        Guid userId = user.UserId!.Value;
        JoinFriendGroupRequest? request = JsonConvert.DeserializeObject<JoinFriendGroupRequest>(data!, _jsonSerializerSettings)!;
        bool isValid = await ValidateJoinFriendGroup(userId, request, ct);
        if (!isValid)
        {
            return;
        }

        UserGroup? group = await _groupRepository.GetActiveUserGroup(request!.UserId, ct);
        UserGroup userGroup = new()
        {
            UserId = userId,
            GroupId = group!.GroupId,
            UserStatus = UserGroupStatus.RequestedJoin,
        };
        await _groupRepository.CreateUserGroup(userGroup, ct);

        // Уведомление лидеру группы о входящем запросе на присоединение
        await SendUserRequestedGroupJoinMessageToGroupLeader(userId, user.UserName!, group!.GroupId, ct);
    }

    private async Task AcceptUserJoiningGroup(Guid userId, string? data, CancellationToken ct)
    {
        AcceptUserJoiningGroupRequest? request =
            JsonConvert.DeserializeObject<AcceptUserJoiningGroupRequest>(data!, _jsonSerializerSettings)!;
        bool isValid = await ValidateAcceptUserJoiningGroup(userId, request, ct);
        if (!isValid)
        {
            return;
        }

        UserGroup userGroup = (await GetUserGroup(request.UserId, request.GroupId, ct))!;
        if (request.IsAccepted)
        {
            userGroup.UserStatus = UserGroupStatus.Active;
            await _groupRepository.UpdateUserGroup(userGroup, ct);

            // Уведомление остальным членам группы, что появился новый участник
            User joinedUser = await _userDataService.GetById(request.UserId, ct);
            await SendNewUserJoinedGroupMessageToGroupMembers(request.UserId, joinedUser.UserName!, request.GroupId, ct);

            await _chatService.AddUserToChat($"group:{request.GroupId}", request.UserId, ct);

            // Уведомление юзеру, что его запрос о вступлении в группу одобрен
            await SendYouJoinedGroupMessageToUser(request.UserId, request.GroupId, ct);

            var notActiveGroupList = await _groupRepository.GetNotActiveUserGroup(request.UserId, ct);
            foreach (var group in notActiveGroupList)
            {
                await SendUserDeclineGroupJoinMessageToGroupLeader(request.UserId, joinedUser.UserName!, group.GroupId, ct);
            }
            await _groupRepository.DeleteNotActiveUserGroups(request.UserId, ct);
        }
        else
        {
            await _groupRepository.DeleteUserGroup(userGroup, ct);
            await SendDeclineRequestJoinGroupMessageToUser(request.UserId, request.GroupId, ct);
        }
    }

    private async Task<IEnumerable<UserGroup>> GetActiveGroupMembers(Guid groupId, CancellationToken ct)
    {
        List<UserGroup> allGroupMembers = await _groupRepository.GetGroupMembers(groupId, ct);
        return allGroupMembers.Where(userGroup => userGroup.UserStatus == UserGroupStatus.Active).ToList();
    }

    private async Task<UserGroup?> GetUserGroup(Guid userId, Guid groupId, CancellationToken ct)
    {
        List<UserGroup> allGroupMembers = await _groupRepository.GetGroupMembers(groupId, ct);
        return allGroupMembers.Where(userGroup => userGroup.UserId == userId).FirstOrDefault();
    }

    #endregion

    #region Notification

    private async Task SendGroupInvitationMessageToUser(Guid userId, IOutputMessageData messageData, CancellationToken ct)
    {
        await _notificationService.AddNotificationToSingleUser(userId, messageData, _endpointCategory.ToString(), GroupEndpointKind.YouWereInvitedNotification.ToString(), ct);
    }

    private async Task SendNewUserJoinedGroupMessageToGroupMembers(Guid newUserId, string newUserName, Guid groupId, CancellationToken ct)
    {
        IEnumerable<UserGroup> groupMembers = await GetActiveGroupMembers(groupId, ct);
        foreach (UserGroup groupMember in groupMembers)
        {
            Guid groupMemberUserId = groupMember.UserId;
            if (groupMemberUserId.Equals(newUserId))
            {
                continue;
            }
            UserWithNameMessage messageData = new()
            {
                UserId = newUserId,
                UserName = newUserName,
            };
            await _notificationService.AddNotificationToSingleUser(groupMemberUserId, messageData, _endpointCategory.ToString(),
                GroupEndpointKind.NewUserJoinedGroupNotification.ToString(), ct);
        }
    }

    private async Task SendUserLeftGroupMessageToGroupMembers(Guid userId, string userName, Guid groupId, CancellationToken ct)
    {
        IEnumerable<UserGroup> groupMembers = await GetActiveGroupMembers(groupId, ct);
        foreach (UserGroup groupMember in groupMembers)
        {
            Guid groupMemberUserId = groupMember.UserId;
            UserWithNameMessage messageData = new()
            {
                UserId = userId,
                UserName = userName,
            };
            await _notificationService.AddNotificationToSingleUser(groupMemberUserId, messageData, _endpointCategory.ToString(),
                        GroupEndpointKind.UserLeftGroupNotification.ToString(), ct);
        }
    }

    private async Task SendUserRequestedGroupJoinMessageToGroupLeader(Guid userId, string userName, Guid groupId, CancellationToken ct)
    {
        Group group = await _groupRepository.GetById(groupId, ct);
        Guid leaderUserId = group.LeaderUserId;
        UserWithNameMessage messageData = new()
        {
            UserId = userId,
            UserName = userName,
        };
        await _notificationService.AddNotificationToSingleUser(leaderUserId, messageData, _endpointCategory.ToString(),
            GroupEndpointKind.UserRequestedGroupJoinNotification.ToString(), ct);
    }
    
    private async Task SendUserDeclineGroupJoinMessageToGroupLeader(Guid userId, string userName, Guid groupId, CancellationToken ct)
    {
        Group group = await _groupRepository.GetById(groupId, ct);
        Guid leaderUserId = group.LeaderUserId;
        UserWithNameMessage messageData = new()
        {
            UserId = userId,
            UserName = userName,
        };
        await _notificationService.AddNotificationToSingleUser(leaderUserId, messageData, _endpointCategory.ToString(),
            GroupEndpointKind.UserDeclineInviteToGroupNotification.ToString(), ct);
    }

    private async Task SendYouJoinedGroupMessageToUser(Guid userId, Guid groupId, CancellationToken ct)
    {
        YouJoinedGroupMessage messageData = new()
        {
            GroupId = groupId,
        };
        await _notificationService.AddNotificationToSingleUser(userId, messageData, _endpointCategory.ToString(),
                GroupEndpointKind.YouJoinedGroupNotification.ToString(), ct);
    }

    private async Task SendDeclineRequestJoinGroupMessageToUser(Guid userId, Guid groupId, CancellationToken ct)
    {
        YouJoinedGroupMessage messageData = new()
        {
            GroupId = groupId,
            IsJoined = false
        };
        await _notificationService.AddNotificationToSingleUser(userId, messageData, _endpointCategory.ToString(),
                GroupEndpointKind.DeclineRequestJoinGroupNotification.ToString(), ct);
    }

    private async Task SendYouAreTheLeaderNowMessageToGroupLeader(Guid userId, CancellationToken ct)
    {
        await _notificationService.AddNotificationToSingleUser(userId, messageData: null, _endpointCategory.ToString(),
            GroupEndpointKind.YouAreTheLeaderNowNotification.ToString(), ct);
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateCreateGroup(Guid userId, CancellationToken ct)
    {
        UserGroup? activeGroup = await _groupRepository.GetActiveUserGroup(userId, ct);
        if (activeGroup != null)
        {
            _validationStorage.AddError(ErrorCode.UserIsAlreadyAGroupMember, $"User {userId} is already a group member");
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateExitGroup(Guid userId, CancellationToken ct)
    {
        UserGroup? activeGroup = await _groupRepository.GetActiveUserGroup(userId, ct);
        if (activeGroup == null)
        {
            _validationStorage.AddError(ErrorCode.UserIsNotAGroupMember, $"User {userId} is not a group member");
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateInviteFriendToGroup(Guid userId, InviteFriendToGroupRequest? request, CancellationToken ct)
    {
        bool isValid = ValidationUtils.BasicWsValidation(request, _validationStorage);
        if (!isValid)
        {
            return false;
        }

        Guid friendUserId = request!.FriendUserId;
        Guid groupId = request.GroupId;
        bool isFriendUserValid = await _userDataService.ValidateUser(friendUserId, ct);
        if (!isFriendUserValid)
        {
            return false;
        }
        if (!await _userRelationsService.IsFriend(userId, friendUserId, ct))
        {
            _validationStorage.AddError(ErrorCode.UserIsNotYourFriend, $"User {friendUserId} is not a friend of the user {userId}");
            return false;
        }
        Group? group = await _groupRepository.GetByIdOptional(groupId, ct);
        if (group == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownGroup, $"Group {groupId} does not exist");
            return false;
        }
        if (!group.LeaderUserId.Equals(userId))
        {
            _validationStorage.AddError(ErrorCode.YouAreNotALeader, $"User {userId} is not a leader of the group {groupId}!");
            return false;
        }
        UserGroup? activeFriendGroup = await _groupRepository.GetActiveUserGroup(friendUserId, ct);
        if (activeFriendGroup != null)
        {
            _validationStorage.AddError(ErrorCode.UserIsAlreadyAGroupMember, $"User {friendUserId} is already a group member");
        }
        IEnumerable<UserGroup> activeGroupMembers = await GetActiveGroupMembers(groupId, ct);
        if (activeGroupMembers.Count() >= MaxUsersInGroup)
        {
            _validationStorage.AddError(ErrorCode.GroupSizeExceeded, $"Group {groupId} size exceeded");
        }
        UserGroup? currentUserGroup = await GetUserGroup(friendUserId, groupId, ct);
        if (currentUserGroup != null)
        {
            _validationStorage.AddError(ErrorCode.UserIsAlreadyInGroup, $"User {friendUserId} already has connection to group {groupId}");
        }
        await _userDataService.ValidateUserIsOnline(friendUserId, _validationStorage, ct);

        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateAcceptInvitationToGroup(Guid userId, AcceptInvitationToGroupRequest? request, CancellationToken ct)
    {
        bool isValid = ValidationUtils.BasicWsValidation(request, _validationStorage);
        if (!isValid)
        {
            return false;
        }

        Guid groupId = request!.GroupId;
        bool isAccepted = request.IsAccepted;
        Group? group = await _groupRepository.GetByIdOptional(groupId, ct);
        if (group == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownGroup, $"Group {groupId} does not exist");
            return false;
        }
        UserGroup? currentUserGroup = await GetUserGroup(userId, groupId, ct);
        if (currentUserGroup == null || currentUserGroup.UserStatus != UserGroupStatus.Invited)
        {
            _validationStorage.AddError(ErrorCode.GroupInvitationDoesNotExist, $"User {userId} is not invited to group {groupId}");
        }
        // Проверки на активную группу юзера и на размер группы имеют смысл, только если пользователь принял приглашение, а не отклонил
        if (isAccepted)
        {
            UserGroup? activeFriendGroup = await _groupRepository.GetActiveUserGroup(userId, ct);
            if (activeFriendGroup != null)
            {
                _validationStorage.AddError(ErrorCode.UserIsAlreadyAGroupMember, $"User {userId} is already a group member");
            }
            await ValidateGroupSize(groupId, ct);
        }

        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateJoinFriendGroup(Guid userId, JoinFriendGroupRequest? request, CancellationToken ct)
    {
        bool isValid = ValidationUtils.BasicWsValidation(request, _validationStorage);
        if (!isValid)
        {
            return false;
        }

        UserGroup? group = await _groupRepository.GetActiveUserGroup(request!.UserId, ct);
        if (group == null)
        {
            _validationStorage.AddError(ErrorCode.UserIsNotAGroupMember, $"User {request!.UserId} is not a group member");
            return false;
        }
        UserGroup? activeFriendGroup = await _groupRepository.GetActiveUserGroup(userId, ct);
        if (activeFriendGroup != null)
        {
            _validationStorage.AddError(ErrorCode.UserIsAlreadyAGroupMember, $"User {userId} is already a group member");
        }
        UserGroup? currentUserGroup = await GetUserGroup(userId, group.GroupId, ct);
        if (currentUserGroup != null)
        {
            _validationStorage.AddError(ErrorCode.UserIsAlreadyInGroup, $"User {userId} already has a connection to group {group.GroupId}");
        }
        await ValidateGroupSize(group.GroupId, ct);

        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateAcceptUserJoiningGroup(Guid userId, AcceptUserJoiningGroupRequest? request, CancellationToken ct)
    {
        bool isValid = ValidationUtils.BasicWsValidation(request, _validationStorage);
        if (!isValid)
        {
            return false;
        }

        Guid otherUserId = request!.UserId;
        Guid groupId = request!.GroupId;
        bool isAccepted = request.IsAccepted;
        bool isUserValid = await _userDataService.ValidateUser(otherUserId, ct);
        if (!isUserValid)
        {
            return false;
        }
        Group? group = await _groupRepository.GetByIdOptional(groupId, ct);
        if (group == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownGroup, $"Group {groupId} does not exist");
            return false;
        }
        if (!group.LeaderUserId.Equals(userId))
        {
            _validationStorage.AddError(ErrorCode.YouAreNotALeader, $"User {userId} is not a leader of the group {groupId}!");
            return false;
        }
        UserGroup? currentUserGroup = await GetUserGroup(otherUserId, groupId, ct);
        if (currentUserGroup == null || currentUserGroup.UserStatus != UserGroupStatus.RequestedJoin)
        {
            _validationStorage.AddError(ErrorCode.GroupJoiningRequestDoesNotExist, $"User {otherUserId} did not request joining group {groupId}");
            return false;
        }
        // Проверки на активную группу юзера и на размер группы имеют смысл, только если пользователь принял приглашение, а не отклонил
        if (isAccepted)
        {
            UserGroup? activeFriendGroup = await _groupRepository.GetActiveUserGroup(otherUserId, ct);
            if (activeFriendGroup != null)
            {
                _validationStorage.AddError(ErrorCode.UserIsAlreadyAGroupMember, $"User {otherUserId} is already a group member");
            }
            await ValidateGroupSize(groupId, ct);
        }
        await _userDataService.ValidateUserIsOnline(otherUserId, _validationStorage, ct);

        return _validationStorage.IsValid;
    }

    private async Task ValidateGroupSize(Guid groupId, CancellationToken ct)
    {
        IEnumerable<UserGroup> activeGroupMembers = await GetActiveGroupMembers(groupId, ct);
        if (activeGroupMembers.Count() >= MaxUsersInGroup)
        {
            _validationStorage.AddError(ErrorCode.GroupSizeExceeded, $"Group {groupId} size exceeded");
        }
    }

    #endregion
}