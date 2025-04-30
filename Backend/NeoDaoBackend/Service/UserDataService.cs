using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL.Responses;
using NeoDaoBackend.Models.UserData;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Service.GraphQL;
using NeoDaoBackend.Services.OpenMatch;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Service;

public class UserDataService : AbstractWebSocketService<UserDataEndpointKind>
{
    private readonly IBridgeService _bridgeService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUserDataRepository _userDataRepository;
    private readonly IUserAvatarRepository _userAvatarRepository;
    private readonly IUserInventoryRepository _userInventoryRepository;
    private readonly IUserBalanceRepository _userBalanceRepository;
    private readonly IOpenMatchTicketService _ticketService;
    private readonly int _promocode_activation_count = 1;

    public UserDataService(ILogger<UserDataService> logger,
        IValidationStorage validationStorage,
        JsonSerializerSettings jsonSerializerSettings,
        NotificationService notificationService,
        IBridgeService bridgeService,
        IUserSessionRepository userSessionRepository,
        IUserDataRepository userDataRepository,
        IUserAvatarRepository userAvatarRepository,
        IUserInventoryRepository userInventoryRepository,
        IUserBalanceRepository userBalanceRepository,
        IOpenMatchTicketService ticketService) :
        base(EndpointCategory.AvatarSettings, validationStorage, logger, jsonSerializerSettings, notificationService)
    {
        _bridgeService = bridgeService;
        _userSessionRepository = userSessionRepository;
        _userDataRepository = userDataRepository;
        _userAvatarRepository = userAvatarRepository;
        _userInventoryRepository = userInventoryRepository;
        _userBalanceRepository = userBalanceRepository;
        _ticketService = ticketService;
        //_promocode_activation_count = int.Parse(Environment.GetEnvironmentVariable("PROMOCODE_ACTIVATION_COUNT")!);
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, UserDataEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case UserDataEndpointKind.SaveAvatarSettings:
                outputMessage = await SaveAvatarSettings(user, data!, ct);
                break;
            case UserDataEndpointKind.GetAvatarSettings:
                outputMessage = await GetAvatarSettings(user, ct);
                break;
            case UserDataEndpointKind.UpdateUserName:
                await UpdateUserName(JsonConvert.DeserializeObject<UpdateUserNameRequest>(data!)!, user, ct);
                break;
            default:
                _validationStorage.AddError(ErrorCode.WrongEndpointKind, $"Unknown event type for category \"{_endpointCategory}\": \"{data}\"");
                break;
        }
        return outputMessage;
    }

    #region Actions

    public async Task<GetMePayload?> LoadUserInfo(NeoDaoUser user, CancellationToken ct)
    {
        GetMePayload? me = await _bridgeService.GetUserInfo(user, ct);
        if (me == null)
        {
            return null;
        }

        Guid internalSessionId = user.InternalSessionId!.Value;

        User? userPlayer = await _userDataRepository.GetByIdOptional(me.Id, ct);
        if (userPlayer == null)
        {
            if (string.IsNullOrEmpty(me.Name))
            {
                me.Name = DefaultUserName;
            }
            Guid inventoryId = Guid.NewGuid();
            InitUserNoSave(me.Id, me.Name, inventoryId, ct);
            user.InventoryId = inventoryId;
        }
        else
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test")
            {
                me.Name = userPlayer.UserName;
            }
            else
            {
                if (string.IsNullOrEmpty(me.Name))
                {
                    me.Name = DefaultUserName;
                }
            }
            userPlayer.LastLoginAt = DateTimeOffset.UtcNow;
            userPlayer.UserName = me.Name;
            Inventory inventory = await _userInventoryRepository.GetInventoryByUserId(me.Id, ct);
            user.InventoryId = inventory.InventoryId;
        }

        await _userSessionRepository.UpdateUserId(internalSessionId, me.Id, ct);
        return me;
    }

    private async Task<SaveAvatarSettingsData> SaveAvatarSettings(NeoDaoUser user, string data, CancellationToken ct)
    {
        SaveAvatarSettingsData? settings = JsonConvert.DeserializeObject<SaveAvatarSettingsData>(data);
        bool isValid = await ValidateSaveAvatarSettings(user, settings, ct);
        if (!isValid)
        {
            return null!;
        }
        await _userAvatarRepository.Create(user.UserId!.Value, settings!.Gender, ct);
        _logger.LogInformation($"Creating user avatar with userId {user.UserId!.Value}: gender {settings!.Gender}");
        return settings;
    }

    private async Task<GetAvatarSettingsData> GetAvatarSettings(NeoDaoUser user, CancellationToken ct)
    {
        UserAvatar? userAvatar = await _userAvatarRepository.GetByUserId(user.UserId!.Value, ct);
        GetAvatarSettingsData settings = new GetAvatarSettingsData { };
        if (userAvatar != null)
        {
            settings.Gender = userAvatar.Gender;
        }
        return settings;
    }

    public async Task<Guid> AuthorizeFakeUser(Guid userId, string? userName, CancellationToken ct)
    {
        Session? session;
        if (!await _userDataRepository.UserExists(userId, ct))
        {
            _logger.LogInformation($"User {userId} not found. Creating a fake user with a fake session");
            session = Session.GetDevelopmentSession(userId, "whatever");
            string name = string.IsNullOrEmpty(userName) ? Guid.NewGuid().ToString() : userName;
            InitUserNoSave(userId, name, Guid.NewGuid(), ct);
            await _userSessionRepository.SaveDevelopmentSession(session, ct);
            return session.ExternalSessionId;
        }

        session = await _userSessionRepository.GetConfirmedSessionByUserId(userId, ct);
        if (session == null)
        {
            session = Session.GetDevelopmentSession(userId, "whatever");
            _logger.LogInformation($"User {userId} doesn't have a confirmed session. Creating a new session {session.ExternalSessionId}");
            await _userSessionRepository.SaveDevelopmentSession(session, ct);
        }
        else
        {
            _logger.LogInformation($"Skip session creation: user {userId} already has confirmed session {session.ExternalSessionId}");
        }
        return session.ExternalSessionId;
    }

    public async Task<bool> IsUserOnline(Guid userId, CancellationToken ct)
    {
        return await _userDataRepository.IsUserOnline(userId, ct);
    }

    public void UpdateIsOnlineStatus(Guid userId, bool onlineStatus)
    {
        _userDataRepository.UpdateIsOnlineStatus(userId, onlineStatus);
    }

    public async Task<bool> DeleteUser(Guid userId, CancellationToken ct)
    {
        bool isValid = await ValidateUser(userId, ct);
        if (!isValid)
        {
            return false;
        }
        Inventory inventory = await _userInventoryRepository.GetInventoryByUserId(userId, ct);
        await _userDataRepository.DeleteUserData(userId, inventory.InventoryId, ct);
        return true;
    }

    public async Task<bool> UpdateUserName(UpdateUserNameRequest request, CancellationToken ct)
    {
        bool isValid = await ValidateUser(request.UserId, ct);
        if (!isValid)
        {
            return false;
        }

        await _userDataRepository.UpdateUserName(request.UserId, request.UserName, ct);
        return true;
    }

    public async Task<bool> UpdateUserName(UpdateUserNameRequest request, NeoDaoUser user, CancellationToken ct)
    {
        bool isValid = await ValidateUser(request.UserId, ct);
        if (!isValid)
        {
            return false;
        }

        await _userDataRepository.UpdateUserName(request.UserId, request.UserName, ct);
        user.UserName = request.UserName;
        return true;
    }

    public async Task<List<string>> GetPromocodeList(CancellationToken token)
    {
        return await _userDataRepository.GetActivePromocodes(token);
    }

    public async Task<Guid> ActivatePromocode(string code, CancellationToken ct)
    {
        await ValidatePromocode(code, ct);
        if (!_validationStorage.IsValid)
        {
            return Guid.Empty;
        }
        await _userDataRepository.UsePromocode(code, ct);
        return await AuthorizeFakeUser(Guid.NewGuid(), null, ct);
    }

    public async Task<List<string>> GeneratePromocode(int count, CancellationToken ct)
    {
        var result = new List<string>();
        for (int i = 0; i < count; i++)
        {
            string base64Guid = "Metacity_" + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            while(base64Guid.Contains('+') || base64Guid.Contains('/'))
            {
                base64Guid = "Metacity_" + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }
            result.Add(base64Guid);
        }
        await _userDataRepository.AddPromocodes(result, ct);
        return result;
    }

    public async Task<bool> RemoveAllPromocode(CancellationToken ct)
    {
        await _userDataRepository.ClearPromocodes(ct);
        return true;
    }

    private void InitUserNoSave(Guid userId, string userName, Guid inventoryId, CancellationToken ct)
    {
        _userDataRepository.CreateUserNoSave(userId, userName, ct);
        _userInventoryRepository.CreateInventoryNoSave(userId, inventoryId, ct);
        _userBalanceRepository.CreateBalanceNoSave(userId, InitialSoftCoins, ct);
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateSaveAvatarSettings(NeoDaoUser user, SaveAvatarSettingsData? settings, CancellationToken ct)
    {
        if (settings == null)
        {
            ValidationUtils.AddEmptyDataError(_validationStorage);
            return false;
        }
        UserAvatar? userAvatar = await _userAvatarRepository.GetByUserId(user.UserId!.Value, ct);
        if (userAvatar != null)
        {
            _validationStorage.AddError(ErrorCode.UserAvatarAlreadyExists, $"Cannot save user avatar: user already has an avatar");
        }
        return _validationStorage.IsValid;
    }

    public async Task<bool> ValidateUser(Guid userId, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(userId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, userId);
        }
        return _validationStorage.IsValid;
    }

    public async Task<User> GetById(Guid userId, CancellationToken ct)
    {
        return await _userDataRepository.GetById(userId, ct);
    }

    public async Task FinalizeUserSession(NeoDaoUser user, CancellationToken ct)
    {
        Guid externalSessionId = user.ExternalSessionId!.Value;
        Session? session = await _userSessionRepository.GetByExternalId(externalSessionId, ct);
        if (session == null)
        {
            string errorMsg = $"No session found for externalSessionId {externalSessionId}";
            _logger.LogError(errorMsg);
            throw new ApplicationException(errorMsg);
        }

        bool revokeTokenResult = await _bridgeService.RevokeToken(session.Token!, ct);
        if (!revokeTokenResult)
        {
            _logger.LogWarning("Revoke token (ChainBridge) session was unsuccessful!");
        }
        _logger.LogInformation($"Finalizing session with external id {externalSessionId}, user id = {user.UserId}: removing Open Match ticket {session.OpenMatchTicket}");
        await _ticketService.RemoveByTicketId(session.OpenMatchTicket);
        await _userSessionRepository.RemoveUserSession(session, ct);
    }

    public async Task ValidateUserIsOnline(Guid userId, IValidationStorage validationStorage, CancellationToken ct)
    {
        if (!await IsUserOnline(userId, ct))
        {
            validationStorage.AddError(ErrorCode.UserIsOffline, $"User {userId} is offline");
        }
    }

    private async Task ValidatePromocode(string code, CancellationToken ct)
    {
        if (await _userDataRepository.GetCountUsesCode(code, ct) == 0)
        {
            _validationStorage.AddError(ErrorCode.CodeIsNotValid, $"Code {code} is not valid");
        }
    }

    #endregion
}