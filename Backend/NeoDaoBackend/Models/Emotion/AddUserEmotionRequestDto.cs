using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Emotion;

public class AddUserEmotionRequestDto
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotEmptyString]
    public string UnrealId { get; set; }
}