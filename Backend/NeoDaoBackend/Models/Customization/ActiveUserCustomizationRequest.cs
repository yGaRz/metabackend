using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Customization;

public class ActiveUserCustomizationRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotEmptyString]
    public string UnrealId { get; set; }
    public CustomizationSlotType SlotType { get; set; }
    [ValidJson]
    public string Properties { get; set; }
}