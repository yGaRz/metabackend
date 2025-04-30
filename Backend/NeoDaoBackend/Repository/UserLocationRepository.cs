using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using EFCore.BulkExtensions;

namespace NeoDaoBackend.Repository;

public class UserLocationRepository : IUserLocationRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserLocationRepository(NeoDaoDbContext dbContext)
    {
        //TODO:РњРёРіСЂР°С†РёСЏ СЃ РїРµСЂРµРёРјРµРЅРѕРІР°РЅРёРµРј С‚Р°Р±Р»РёС†С‹ playerLocation->UserLocation + РёР·РјРµРЅРµРЅРёРµ РІ РґРѕРєСѓРјРµРЅС‚Р°С†РёРё
        _dbContext = dbContext;
    }
    
    public async Task CreateOrUpdatePlayerLocations(List<PlayerLocation> playerLocations, CancellationToken ct)
    {
        //TODO:Убрать этот пакет, сделать нормальную логику в репо
        await _dbContext.BulkInsertOrUpdateAsync(playerLocations, cancellationToken: ct);
    }

    public async Task<PlayerLocation?> GetPlayerLocationByUserId(Guid userId, CancellationToken ct)
    {
        return (await _dbContext.PlayerLocations
            .Where(p => p.UserId == userId)
            .FirstOrDefaultAsync(ct))!;
    }
}