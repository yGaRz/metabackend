namespace NeoDaoBackend.Models.Group;

public enum GroupEndpointKind
{
    GetGroupData,
    CreateGroup,
    ExitGroup,
    InviteFriendToGroup,
    AcceptInvitationToGroup,
    JoinFriendGroup,
    AcceptUserJoiningGroup,

    // Notifications
    YouWereInvitedNotification,
    NewUserJoinedGroupNotification,
    UserLeftGroupNotification,
    UserRequestedGroupJoinNotification,
    UserDeclineInviteToGroupNotification,
    YouJoinedGroupNotification,
    DeclineRequestJoinGroupNotification,
    YouAreTheLeaderNowNotification,
}
