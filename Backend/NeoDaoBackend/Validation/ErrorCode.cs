namespace NeoDaoBackend.Validation;

public enum ErrorCode
{
    // Common
    CannotParseMessage,
    WrongEndpointKind,
    EmptyData,
    InvalidData,
    InternalServerError,
    ErrorDateTime,
    // Authorization
    PlatformIsMissing,
    ConfirmSessionInconsistentPayload,
    UnknownSession,
    // User
    UnknownUser,
    UserIsOffline,
    // Inventory
    UnknownInventoryItem,
    NotEnoughItem,
    // Missions
    UnknownUserMission,
    UserMissionAlreadyExists,
    UserMissionHasDuplicatedObjective,
    // Balance
    NotEnoughSoftCoins,
    NotEnoughHardCoins,
    InsufficientHardCoins,
    NotEnoughCoins,
    // Transports
    UnknownTransport,
    TransportAlreadyExists,
    UserTransportDoesNotExist,
    UserTransportAlreadyExists,
    // User avatar
    UserAvatarAlreadyExists,
    // User relations
    YouBlockedUser,
    UserBlockedYou,
    OutgoingBlockAlreadyExists,
    OutgoingBlockDoesNotExist,
    FriendsInviteAlreadyExists,
    IncomingFriendsInviteExists,
    IncomingInviteDoesNotExist,
    OutgoingInviteDoesNotExist,
    UserIsYourFriend,
    UserIsNotYourFriend,
    AlreadyYourFriend,
    // Group
    UnknownGroup,
    UserIsAlreadyAGroupMember,
    UserIsNotAGroupMember,
    YouAreNotALeader,
    GroupSizeExceeded,
    GroupJoiningRequestDoesNotExist,
    GroupInvitationDoesNotExist,
    UserIsAlreadyInGroup,
    // Chat
    UnknownChat,
    ForbiddenCharacters,
    WrongChat,
    EqualPreviousMessage,
    // FreeTradeItem
    UnknownFreeTradeItem,
    FreeTradeItemAlreadyExists,
    // Equipment
    EquipmentSlotIsAlreadyUsed,
    EquipmentSlotIsEmpty,
    // StoreItem
    UnknownStoreItem,
    CannotDeleteOwnedStoreItem,
    StoreItemIsAlreadyBought,
    StoreItemAlreadyExists,
    NFTIsMultiPurchasable,
    NFTCanNotBeSelled,
    // Streaming
    UnknownStream,
    // Emotions
    UserEmotionAlreadyExists,
    // Customization
    UserAvailableCustomizationAlreadyExists,
    UserActiveCustomizationAlreadyExists,
    UnknownUserAvailableCustomization,
    UnknownUserActiveCustomization,
    // NFT Item
    UnknownNFTSlot,
    NFTItemSlotAlreadyExists,
    ItemOwnershipViolation,
    ItemIsNotNFTItem,
    // Auction
    AuctionNotFound,
    InvalidAuctionTime,
    NoEnableAuction,
    WrongCoinType,
    //Promocode
    CodeIsNotValid
}
