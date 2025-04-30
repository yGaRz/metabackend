namespace NeoDaoBackend.Models.Emotion;

public class GetUserEmotionsResponse
{
    public IEnumerable<UserEmotionDto> Emotions { get; set; } = null!;
}

public class UserEmotionDto
{
    public string UnrealId { get; set; } = "";
}