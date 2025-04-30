namespace NeoDaoBackend.Models.Customization;

public class AvailableCustomizationResponse
{
    public IEnumerable<UserAvailableCustomizationDto> AvailableCustomizations { get; set; } = null!;
}

public class UserAvailableCustomizationDto
{
    public string UnrealId { get; set; } = "";
}