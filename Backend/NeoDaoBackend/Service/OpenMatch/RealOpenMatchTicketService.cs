using Grpc.Net.Client;
using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Matchmaking;
using NeoDaoBackend.Repository;
using OpenMatch;

using static OpenMatch.FrontendService;

namespace NeoDaoBackend.Services.OpenMatch;

public class RealOpenMatchTicketService : IOpenMatchTicketService {
    private readonly ILogger<RealOpenMatchTicketService> _logger;
    private readonly string _frontendAddress;
    private readonly IUserSessionRepository _userSessionRepository;

    public RealOpenMatchTicketService(ILogger<RealOpenMatchTicketService> logger, IUserSessionRepository userSessionRepository) {
        _logger = logger;
        string? frontendAddressEnv = Environment.GetEnvironmentVariable("OM_FRONTEND_ADDRESS") ??
            throw new ApplicationException("Environment variable OM_FRONTEND_ADDRESS is not set!");
        _frontendAddress = frontendAddressEnv!;
        _userSessionRepository = userSessionRepository;
    }
    
    public async Task<Ticket> Create(NeoDaoUser user, GameServerType gameServerType, CancellationToken ct) 
    {
        _logger.LogInformation($"Creating ticket with game server type {gameServerType}");

        // Retrieve the session
        Session? session = await _userSessionRepository.GetByExternalId(user.ExternalSessionId.Value!, ct);

        // Check if the session already has an OpenMatchTicket
        if (!string.IsNullOrEmpty(session!.OpenMatchTicket)) {
            _logger.LogInformation($"Session already has a ticket with id {session.OpenMatchTicket}. Deleting old ticket before creating a new one.");
            await RemoveByTicketId(session.OpenMatchTicket);
        }

        // Proceed to create a new ticket
        FrontendServiceClient frontendClient = getOpenMatchFrontendClient();
        Ticket ticket = MakeTicket(gameServerType.ToString());
        CreateTicketRequest createRequest = new CreateTicketRequest { Ticket = ticket };
        Ticket createdTicket = await frontendClient.CreateTicketAsync(createRequest);

        _logger.LogInformation($"Created ticket with id {createdTicket.Id} and game server type {gameServerType}");

        // Update the session with the new ticket ID
        session.OpenMatchTicket = createdTicket.Id;
        await _userSessionRepository.UpdateSession(session, ct);

        _logger.LogInformation($"Saved new ticket with id {createdTicket.Id} in DataBase");

        return createdTicket;
    }


    public async Task<Ticket> GetTicketById(string ticketId) {
        _logger.LogInformation($"Getting ticket with id {ticketId}");
        FrontendServiceClient frontendClient = getOpenMatchFrontendClient();
        GetTicketRequest request = new GetTicketRequest { TicketId = ticketId };
        Ticket ticket = await frontendClient.GetTicketAsync(request);
        _logger.LogInformation($"Ticket with id {ticketId} is obtained");
        return ticket;
    }

    public async Task RemoveByTicketId(string ticketId) {
        _logger.LogInformation("Removing ticket with id {id}", ticketId);
        FrontendServiceClient frontendClient = getOpenMatchFrontendClient();
        DeleteTicketRequest deleteRequest = new DeleteTicketRequest { TicketId = ticketId };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            await frontendClient.DeleteTicketAsync(deleteRequest, cancellationToken: cts.Token);
            _logger.LogInformation("Ticket with id {id} is removed", ticketId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError($"Ticket removal timed out for id {ticketId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to remove ticket with id {ticketId} message: {e.Message}");
        }
    }

    private FrontendServiceClient getOpenMatchFrontendClient() {
        var channel = GrpcChannel.ForAddress(_frontendAddress);
        return new FrontendServiceClient(channel);
    }

    private static Ticket MakeTicket(string gameServerType) {
        Ticket ticket = new();
        ticket.SearchFields = new SearchFields();
        ticket.SearchFields.Tags.Add(gameServerType);
        return ticket;
    }
}
