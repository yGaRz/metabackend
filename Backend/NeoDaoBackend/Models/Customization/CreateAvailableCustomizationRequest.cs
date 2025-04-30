using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Customization;

public class CreateAvailableCustomizationRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotEmptyString]
    public string UnrealId { get; set; } = null!;
    [ValidJson]
    public string Properties { get; set; }=null!;
}