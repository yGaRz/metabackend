using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Emotion;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserEmotionController: BaseController
{
    private readonly UserEmotionService _emotionService;
    
    public UserEmotionController(UserEmotionService emotionService, IValidationStorage validationStorage): base(validationStorage)
    {
        _emotionService = emotionService;
    }

    [ServerAuthorized]
    [HttpGet("getUserEmotions")]
    public async Task<IActionResult> GetUserEmotions([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _emotionService.GetUserEmotions(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("addUserEmotion")]
    public async Task<IActionResult> AddUserEmotion([FromBody] AddUserEmotionRequestDto dto, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _emotionService.AddUserEmotion(dto, token), ct);
    }
}