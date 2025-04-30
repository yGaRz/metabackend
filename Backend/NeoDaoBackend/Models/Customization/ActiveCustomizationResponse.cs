namespace NeoDaoBackend.Models.Customization;

public class ActiveCustomizationResponse
{
    public IEnumerable<UserActiveCustomizationDto> UserActiveCustomizations { get; set; }
}

public class UserActiveCustomizationDto
{
    public string UnrealId { get; set; }
    public CustomizationSlotType SlotType { get; set; }
    public string Properties { get; set; }
}