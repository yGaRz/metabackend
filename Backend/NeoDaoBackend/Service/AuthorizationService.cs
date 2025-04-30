using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL;
using NeoDaoBackend.Models.graphQL.Responses;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Service.GraphQL;
using NeoDaoBackend.Validation;

using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Service;

public class AuthorizationService
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IValidationStorage _validationStorage;
    private readonly IBridgeService _bridgeService;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(IValidationStorage validationStorage, ILogger<AuthorizationService> logger,
        IUserSessionRepository userSessionRepository, IBridgeService bridgeService)
    {
        _validationStorage = validationStorage;
        _logger = logger;
        _userSessionRepository = userSessionRepository;
        _bridgeService = bridgeService;
    }

    public async Task<NeoDaoUser> GetUser(Guid? externalSessionId)
    {
        // check header first
        if (externalSessionId == null)
        {
            _logger.LogInformation("User session id is empty");
            return new NeoDaoUser();
        }

        // Lookup in the database
        Session? userSession = await _userSessionRepository.GetByExternalId(externalSessionId.Value, CancellationToken.None);
        if (userSession == null)
        {
            _logger.LogWarning("User session by external id not found");
            return new NeoDaoUser();
        }
        if (userSession.Status == SessionStatus.REMOVED)
        {
            _logger.LogWarning($"User session by internal id {userSession.InternalSessionId} has already expired");
            return new NeoDaoUser();
        }
        if (userSession.Token == null)
        {
            _logger.LogWarning($"User session by internal id {userSession.InternalSessionId} has still not been initialized with jwt token");
        }
        _logger.LogInformation("User session detected");
        return new NeoDaoUser(userSession!);
    }

    public async Task<CreateUserSessionResponse> CreateSessionRequest(IHeaderDictionary headers, CancellationToken ct)
    {
        bool isValid = ValidateCreateSessionRequest(headers);
        if (!isValid)
        {
            return null!;
        }

        bool isMobile = headers[PlatformHeader] == PlatformMobileKey;
        UserSession userSession = await _bridgeService.CreateUserSessionRequest(isMobile, ct) ??
            throw new ApplicationException("ChainBridge couldn't create a session");
        await _userSessionRepository.CreateUserSession(userSession, ct);
        CreateUserSessionResponse response = new()
        {
            Id = userSession.Id,
            Code = userSession.Code,
            SessionUrl = userSession.SessionUrl,
            ExpiredAt = userSession.ExpiredAt
        };
        return response;
    }

    public async Task<bool> ConfirmSession(WebhookPayload payload, CancellationToken ct)
    {
        bool isValid = await ValidateConfirmSession(payload, ct);
        if (!isValid)
        {
            return false;
        }

        Session session = (await _userSessionRepository.GetByExternalId(payload.SessionRequestId, ct))!;
        await _userSessionRepository.ConfirmSession(session, payload.Success, payload.Token, ct);
        return true;
    }

    public async Task<bool> PushSessionConfirmation(PushSessionConfirmationRequest request, CancellationToken ct)
    {
        bool isValid = await ValidatePushSessionConfirmation(request, ct);
        if (!isValid)
        {
            return false;
        }

        Session session = (await _userSessionRepository.GetByExternalId(request.ExternalSessionId, ct))!;
        if (session!.Status == SessionStatus.CONFIRMED)
        {
            _logger.LogInformation($"Don't need to push confirmation for session with external id {request.ExternalSessionId}" +
                $" as it is already confirmed");
            return true;
        }
        (AuthTokenByUserSessionRequestPayload? payload, bool isNeedToUpdate) = await _bridgeService.GetAuthToken(request.ExternalSessionId, ct);
        if (payload == null)
        {
            throw new ApplicationException("ChainBridge couldn't send us auth token");
        }
        if (isNeedToUpdate)
        {
            await _userSessionRepository.ConfirmSession(session!, payload.Success, payload.Token, ct);
        }
        return true;
    }

    #region Validation

    private bool ValidateCreateSessionRequest(IHeaderDictionary headers)
    {
        if (!headers.ContainsKey(PlatformHeader))
        {
            _validationStorage.AddError(ErrorCode.PlatformIsMissing, "Platform header is not set");
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateConfirmSession(WebhookPayload payload, CancellationToken ct)
    {
        if (payload.Success && payload.Token == null)
        {
            _validationStorage.AddError(ErrorCode.ConfirmSessionInconsistentPayload,
                "Invalid webhook payload! Token must be set if success = true");
        }
        Session? session = await _userSessionRepository.GetByExternalId(payload.SessionRequestId, ct);
        if (session == null || session.Status == SessionStatus.REMOVED)
        {
            _validationStorage.AddError(ErrorCode.UnknownSession,
                $"Session with external id {payload.SessionRequestId} does not exist");
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidatePushSessionConfirmation(PushSessionConfirmationRequest request, CancellationToken ct)
    {
        Guid externalSessionId = request.ExternalSessionId;
        Session? userSession = await _userSessionRepository.GetByExternalId(externalSessionId, ct);
        if (userSession == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownSession, $"Session with external id {externalSessionId} does not exist");
        }
        return _validationStorage.IsValid;
    }
    #endregion
}
