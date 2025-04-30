using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Repository;

public class UserTransportRepository : IUserTransportRepository
{
    private readonly NeoDaoDbContext _dbContext;
    public UserTransportRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }  

    public async Task AddUserTransport(Guid userId, string unrealTransportId, CancellationToken ct)
    {
        UserTransport transport = new UserTransport()
        {
            UserId = userId,
            TransportUnrealId = unrealTransportId
        };
        await _dbContext.UserTransports.AddAsync(transport, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<string>> GetUserTransport(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserTransports
            .Where(x => x.UserId == userId)
            .Select(x => x.TransportUnrealId)
            .ToListAsync(ct);
    }

    public async Task<bool> UserTransportExists(Guid userId, string unrealTransportId, CancellationToken ct)
    {
        return await _dbContext.UserTransports
            .Where(x => x.UserId == userId)
            .Where(x => x.TransportUnrealId == unrealTransportId)
            .AnyAsync(ct);
    }
}