using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Emotion;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Validation;

namespace NeoDaoBackend.Service;

public class UserEmotionService
{
    private readonly ILogger<UserEmotionService> _logger;
    private readonly IUserEmotionRepository _emotionRepository;
    private readonly IValidationStorage _validationStorage;
    private readonly UserDataService _userDataService;
    
    public UserEmotionService(ILogger<UserEmotionService> logger, IUserEmotionRepository emotionRepository, 
        IValidationStorage validationStorage, UserDataService userDataService)
    {
        _logger = logger;
        _emotionRepository = emotionRepository;
        _validationStorage = validationStorage;
        _userDataService = userDataService;
    }
    
    public async Task<GetUserEmotionsResponse> GetUserEmotions(Guid userId, CancellationToken ct)
    {
        bool isUserValid = await ValidateGetUserEmotions(userId, ct);
        if (!isUserValid)
        {
            return null!;
        }
        IEnumerable<UserEmotion> emotions = await _emotionRepository.GetUserEmotions(userId, ct);
        return new GetUserEmotionsResponse
        {
            Emotions = emotions.Select(x => new UserEmotionDto
            {
                UnrealId = x.UnrealId
            })
        };
    }

    public async Task<bool> AddUserEmotion(AddUserEmotionRequestDto dto, CancellationToken ct)
    {
        bool isRequestValid = await ValidateAddUserEmotion(dto, ct);
        if (!isRequestValid)
        {
            return false;
        }
        UserEmotion newEmotion = new UserEmotion
        {
            UserId = dto.UserId,
            UnrealId = dto.UnrealId,
        };
        await _emotionRepository.AddUserEmotion(newEmotion, ct);
        return true;
    }

    #region Validation

    private async Task<bool> ValidateGetUserEmotions(Guid userId, CancellationToken ct)
    {
        return await _userDataService.ValidateUser(userId, ct);
    }

    private async Task<bool> ValidateAddUserEmotion(AddUserEmotionRequestDto dto, CancellationToken ct)
    {
        await _userDataService.ValidateUser(dto.UserId, ct);
        
        UserEmotion? userEmotionExist = await _emotionRepository.GetUserEmotionByUnrealId(dto.UserId, dto.UnrealId, ct);
        if (userEmotionExist != null)
        {
            _validationStorage.AddError(ErrorCode.UserEmotionAlreadyExists, "Emotion already exists for this user");
        }
        
        return _validationStorage.IsValid;
    }
    
    #endregion

}