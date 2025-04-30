using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.Customization;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Repository;

public class UserCustomizationRepository : IUserCustomizationRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserCustomizationRepository(NeoDaoDbContext context)
    {
        _dbContext = context;
    }
    
    public async Task<IEnumerable<UserAvailableCustomization>> GetUserAvailableCustomizations(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserAvailableCustomizations
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task CreateUserAvailableCustomization(UserAvailableCustomization availableCustomization, CancellationToken ct)
    {
        _dbContext.UserAvailableCustomizations.Add(availableCustomization);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<UserAvailableCustomization?> GetUserAvailableCustomization(Guid userId, string unrealId, CancellationToken ct)
    {
        return await _dbContext.UserAvailableCustomizations
            .Where(x => x.UserId == userId && x.UnrealId == unrealId)
            .FirstOrDefaultAsync(ct);
    }

    public void UpdateUserAvailableCustomizationNoSave(UserAvailableCustomization availableCustomization)
    {
        _dbContext.UserAvailableCustomizations.Update(availableCustomization);
    }

    public async Task<IEnumerable<UserActiveCustomization>> GetUserActiveCustomizations(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserActiveCustomizations
            .Where(x => x.UserId == userId)
            .Include(x => x.AvailableCustomization)
            .ToListAsync(ct);
    }

    public async Task<UserActiveCustomization?> GetUserActiveCustomization(Guid userId, CustomizationSlotType slotType,
        CancellationToken ct)
    {
        return await _dbContext.UserActiveCustomizations
            .Where(x => x.UserId == userId && x.SlotType == slotType)
            .FirstOrDefaultAsync(ct);
    }

    public async Task CreateUserActiveCustomization(UserActiveCustomization activeCustomization, CancellationToken ct)
    {
        _dbContext.UserActiveCustomizations.Add(activeCustomization);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateUserActiveCustomization(UserActiveCustomization activeCustomization, CancellationToken ct)
    {
        _dbContext.UserActiveCustomizations.Update(activeCustomization);
        await _dbContext.SaveChangesAsync(ct);
    }
}