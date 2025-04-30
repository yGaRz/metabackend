using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Matchmaking;
using OpenMatch;

namespace NeoDaoBackend.Services.OpenMatch;

public interface IOpenMatchTicketService {
    public Task<Ticket> Create(NeoDaoUser user,GameServerType gameServerType, CancellationToken ct);

    public Task<Ticket> GetTicketById(string ticketId);

    public Task RemoveByTicketId(string ticketId);
}
