using NeoDaoBackend.Models.Customization;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository.Interface;

public interface IUserCustomizationRepository
{
    Task<IEnumerable<UserAvailableCustomization>> GetUserAvailableCustomizations(Guid userId, CancellationToken ct);
    Task CreateUserAvailableCustomization(UserAvailableCustomization availableCustomization, CancellationToken ct);
    Task<UserAvailableCustomization?> GetUserAvailableCustomization(Guid userId, string unrealId, CancellationToken ct);
    void UpdateUserAvailableCustomizationNoSave(UserAvailableCustomization availableCustomization);
    
    Task<IEnumerable<UserActiveCustomization>> GetUserActiveCustomizations(Guid userId, CancellationToken ct);
    Task<UserActiveCustomization?> GetUserActiveCustomization(Guid userId, CustomizationSlotType slotType, CancellationToken ct);
    Task CreateUserActiveCustomization(UserActiveCustomization activeCustomization, CancellationToken ct);
    Task UpdateUserActiveCustomization(UserActiveCustomization activeCustomization, CancellationToken ct);
}