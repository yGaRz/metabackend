using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Matchmaking;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Services.OpenMatch;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;
using OpenMatch;

namespace NeoDaoBackend.Service;

public class MatchmakingService : AbstractWebSocketService<MatchmakingEndpointKind>
{
    private readonly IOpenMatchTicketService _openMatchTicketService;

    public MatchmakingService(ILogger<MatchmakingService> logger, 
        IValidationStorage validationStorage,
        IOpenMatchTicketService openMatchTicketService,
        JsonSerializerSettings jsonSerializerSettings,
        NotificationService notificationService) :
        base(EndpointCategory.UserBalance, validationStorage, logger, jsonSerializerSettings, notificationService)
    {
        _openMatchTicketService = openMatchTicketService;
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, MatchmakingEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case MatchmakingEndpointKind.CreateTicket:
                outputMessage = await CreateTicket(user,JsonConvert.DeserializeObject<CreateTicketRequestData>(data!), ct);
                break;
            case MatchmakingEndpointKind.GetTicketAssignment:
                outputMessage = await GetTicketAssignment(JsonConvert.DeserializeObject<GetTicketAssignmentRequestData>(data!));
                break;
            default:
                _validationStorage.AddError(ErrorCode.WrongEndpointKind, $"Unknown event type for category \"{_endpointCategory}\": \"{data}\"");
                break;
        }
        return outputMessage;
    }

    private async Task<CreateTicketResponseData> CreateTicket(NeoDaoUser user, CreateTicketRequestData? data, CancellationToken ct)
    {
        _logger.LogInformation($"Creating a new ticket. Data = {data}");
        // TODO handle errors
        Ticket ticket = await _openMatchTicketService.Create(user, data!.ServerType, ct);
        CreateTicketResponseData responseData = new CreateTicketResponseData
        {
            TicketId = ticket.Id,
            ServerType = data.ServerType,
        };
        return responseData;
    }

    private async Task<GetTicketAssignmentResponseData> GetTicketAssignment(GetTicketAssignmentRequestData? data)
    {
        _logger.LogInformation($"Getting ticket assignment. Data = {data}");
        // TODO handle errors
        Ticket ticket = await _openMatchTicketService.GetTicketById(data!.TicketId);
        GetTicketAssignmentResponseData responseData = new GetTicketAssignmentResponseData
        {
            TicketId = ticket.Id,
            ServerType = data.ServerType,
        };
        if (ticket.Assignment != null)
        {
            responseData.Url = ticket.Assignment.Connection;
        }
        return responseData;
    }
}
