using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;

namespace NeoDaoBackend.Service;

public abstract class AbstractWebSocketService<T> where T : struct, Enum
{
    protected readonly EndpointCategory _endpointCategory;
    protected readonly ILogger _logger;
    protected readonly JsonSerializerSettings _jsonSerializerSettings;
    protected readonly IValidationStorage _validationStorage;
    protected readonly NotificationService _notificationService;

    public AbstractWebSocketService(EndpointCategory endpointCategory,
        IValidationStorage validationStorage,
        ILogger logger,
        JsonSerializerSettings jsonSerializerSettings,
        NotificationService notificationService)
    {
        _endpointCategory = endpointCategory;
        _logger = logger;
        _jsonSerializerSettings = jsonSerializerSettings;
        _validationStorage = validationStorage;
        _notificationService = notificationService;
    }

    public async Task<(IOutputMessageData?, ErrorCode?, string?)> ProcessMessage(NeoDaoUser user, string command, string? data, CancellationToken ct)
    {
        bool isEndpointCorrect = Enum.TryParse(command, ignoreCase: true, out T endpointKind);
        if (isEndpointCorrect)
        {
            IOutputMessageData? outputMessage = await ProcessWebSocketMessage(user, endpointKind, data, ct);
            if (_validationStorage.IsValid)
            {
                _logger.LogInformation($"Ws request {endpointKind} successfully executed");
                return (outputMessage, null, null);
            }
            (ErrorCode errorCode, string errorMessage) = _validationStorage.GetError();
            return (null, errorCode, errorMessage);
        }
        else
        {
            return (null, ErrorCode.WrongEndpointKind, $"Unknown event type for category \"{_endpointCategory}\": \"{command}\"");
        }
    }

    protected abstract Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, T endpointKind, string? data, CancellationToken ct);
}
