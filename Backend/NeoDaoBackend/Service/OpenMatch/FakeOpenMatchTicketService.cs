using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Matchmaking;
using OpenMatch;

namespace NeoDaoBackend.Services.OpenMatch;

public class FakeOpenMatchTicketService : IOpenMatchTicketService {
    public Task<Ticket> Create(NeoDaoUser user, GameServerType gameServerType, CancellationToken ct) {
        Ticket ticket = new Ticket();
        ticket.SearchFields = new SearchFields();
        ticket.SearchFields.Tags.Add(gameServerType.ToString());
        ticket.Id = Guid.NewGuid().ToString();
        return Task.FromResult(ticket);
    }

    public Task<Ticket> GetTicketById(string ticketId) {
        Ticket ticket = new Ticket();
        ticket.SearchFields = new SearchFields();
        ticket.SearchFields.Tags.Add("mine");
        ticket.Id = ticketId;
        return Task.FromResult(ticket);
    }

    public Task RemoveByTicketId(string ticketId) {
        return Task.CompletedTask;
    }
}
