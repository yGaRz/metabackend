using NeoDaoBackend.Models.Customization;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Validation;

namespace NeoDaoBackend.Service;

public class UserCustomizationService
{
    private readonly UserDataService _userDataService;
    private readonly IUserCustomizationRepository _customizationRepository;
    private readonly IValidationStorage _validationStorage;
    
    public UserCustomizationService(UserDataService userDataService, IUserCustomizationRepository customizationRepository, IValidationStorage validationStorage)
    {
        _userDataService = userDataService;
        _customizationRepository = customizationRepository;
        _validationStorage = validationStorage;
    }
    
    public async Task<AvailableCustomizationResponse> GetUserAvailableCustomizations(Guid userId, CancellationToken ct)
    {
        bool isValid = await ValidateGetUserCustomizations(userId, ct);
        if (!isValid)
        {
            return null!;
        }
        IEnumerable<UserAvailableCustomization> availableCustomizations =
            await _customizationRepository.GetUserAvailableCustomizations(userId, ct);
        return new AvailableCustomizationResponse
        {
            AvailableCustomizations = availableCustomizations.Select(x => new UserAvailableCustomizationDto
            {
                UnrealId = x.UnrealId
            })
        };
    }

    public async Task<bool> CreateUserAvailableCustomization(CreateAvailableCustomizationRequest dto, CancellationToken ct)
    {
        bool isRequestValid = await ValidateCreateUserAvailableCustomization(dto, ct);
        if (!isRequestValid)
        {
            return false;
        }
        UserAvailableCustomization customization = new UserAvailableCustomization
        {
            UnrealId = dto.UnrealId,
            UserId = dto.UserId,
            Properties = dto.Properties
        };
        await _customizationRepository.CreateUserAvailableCustomization(customization, ct);
        return true;
    } 

    public async Task<ActiveCustomizationResponse> GetUserActiveCustomizations(Guid userId, CancellationToken ct)
    {
        bool isValid = await ValidateGetUserCustomizations(userId, ct);
        if (!isValid)
        {
            return null!;
        }
        IEnumerable<UserActiveCustomization> activeCustomizations =
            await _customizationRepository.GetUserActiveCustomizations(userId, ct);
        return new ActiveCustomizationResponse
        {
            UserActiveCustomizations = activeCustomizations.Select(x => new UserActiveCustomizationDto
            {
                SlotType = x.SlotType,
                UnrealId = x.UnrealId,
                Properties = x.AvailableCustomization.Properties
            })
        };
    }

    public async Task<bool> CreateActiveUserCustomization(ActiveUserCustomizationRequest dto, CancellationToken ct)
    {
        bool isValid = await ValidateCreateUserActiveCustomization(dto, ct);
        if (!isValid)
        {
            return false;
        }

        UserAvailableCustomization availableCustomization =
            (await _customizationRepository.GetUserAvailableCustomization(dto.UserId, dto.UnrealId, ct))!;
        
        availableCustomization.Properties = dto.Properties;
        _customizationRepository.UpdateUserAvailableCustomizationNoSave(availableCustomization);
        
        UserActiveCustomization customization = new UserActiveCustomization
        {
            UserId = dto.UserId,
            UnrealId = dto.UnrealId,
            SlotType = dto.SlotType
        };
        await _customizationRepository.CreateUserActiveCustomization(customization, ct);
        return true;
    }

    public async Task<bool> UpdateActiveUserCustomization(ActiveUserCustomizationRequest dto, CancellationToken ct)
    {
        bool isValid = await ValidateUpdateUserActiveCustomization(dto, ct);
        if (!isValid)
        {
            return false;
        }
        
        UserAvailableCustomization availableCustomization =
            (await _customizationRepository.GetUserAvailableCustomization(dto.UserId, dto.UnrealId, ct))!;
        UserActiveCustomization activeCustomization =
            (await _customizationRepository.GetUserActiveCustomization(dto.UserId, dto.SlotType, ct))!;

        availableCustomization.Properties = dto.Properties;
        activeCustomization.UnrealId = dto.UnrealId;
        activeCustomization.AvailableCustomization = availableCustomization;
        await _customizationRepository.UpdateUserActiveCustomization(activeCustomization, ct);
        return true;
    }

    
    #region Validation
    private async Task<bool> ValidateGetUserCustomizations(Guid userId, CancellationToken ct)
    {
        return await _userDataService.ValidateUser(userId, ct);
    }

    private async Task<bool> ValidateCreateUserAvailableCustomization(CreateAvailableCustomizationRequest dto, CancellationToken ct)
    {
        bool isUserValid = await _userDataService.ValidateUser(dto.UserId, ct);
        if (isUserValid)
        {
            return false;
        }
        UserAvailableCustomization? availableCustomization =
            await _customizationRepository.GetUserAvailableCustomization(dto.UserId, dto.UnrealId, ct);
        if (availableCustomization != null)
        {
            _validationStorage.AddError(ErrorCode.UserAvailableCustomizationAlreadyExists, "User available customization already exists");
        }

        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateCreateUserActiveCustomization(ActiveUserCustomizationRequest dto,
        CancellationToken ct)
    {
        bool isUserValid = await _userDataService.ValidateUser(dto.UserId, ct);
        if (!isUserValid)
        {
            return false;
        }
        UserAvailableCustomization? availableCustomization =
            await _customizationRepository.GetUserAvailableCustomization(dto.UserId, dto.UnrealId, ct);
        if (availableCustomization == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownUserAvailableCustomization, "Available customization not exists for this user");
        }

        UserActiveCustomization? activeCustomization =
            await _customizationRepository.GetUserActiveCustomization(dto.UserId, dto.SlotType, ct);
        if (activeCustomization != null)
        {
            _validationStorage.AddError(ErrorCode.UserActiveCustomizationAlreadyExists, "Active customization already exists for this user");
        }
        
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateUpdateUserActiveCustomization(ActiveUserCustomizationRequest dto,
        CancellationToken ct)
    {
        bool isUserValid = await _userDataService.ValidateUser(dto.UserId, ct);
        if (!isUserValid)
        {
            return false;
        }

        UserAvailableCustomization? availableCustomization =
            await _customizationRepository.GetUserAvailableCustomization(dto.UserId, dto.UnrealId, ct);
        if (availableCustomization == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownUserAvailableCustomization, "Available customization not exists for this user");
        }
        
        UserActiveCustomization? activeCustomization =
            await _customizationRepository.GetUserActiveCustomization(dto.UserId, dto.SlotType, ct);
        if (activeCustomization == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownUserActiveCustomization, "Active customization not exists for this user");
        }

        return _validationStorage.IsValid;
    }
    #endregion
}