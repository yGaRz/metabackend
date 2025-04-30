using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserTransport;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Validation;

using static NeoDaoBackend.Validation.ErrorCode;

namespace NeoDaoBackend.Service;

public class UserTransportService
{
    private readonly ILogger<UserTransportService> _logger;
    private readonly IValidationStorage _validationStorage;
    private readonly UserDataService _userDataService;
    private readonly IUserTransportRepository _userTransportRepository;
    private readonly UserBalanceService _userBalanceService;

    public UserTransportService(ILogger<UserTransportService> logger,
        IValidationStorage validationStorage,
        UserDataService userDataService,
        IUserTransportRepository userTransportRepository,
        UserBalanceService userBalanceService)
    {
        _logger = logger;
        _validationStorage = validationStorage;
        _userDataService = userDataService;
        _userTransportRepository = userTransportRepository;
        _userBalanceService = userBalanceService;
    }

    #region Actions 

    public async Task<List<string>> GetUserTransport(Guid userId, CancellationToken ct)
    {
        bool isValid = await _userDataService.ValidateUser(userId, ct);
        if (!isValid)
        {
            return null!;
        }
        return await _userTransportRepository.GetUserTransport(userId, ct);
    }

    public async Task<bool> UseTransport(UseTransportRequest request, CancellationToken ct)
    {
        bool isValid = await ValidateUseTransport(request.UserId, request.TransportId, request.Price, ct);
        if (!isValid)
        {
            return false;
        }
        if (request.Price != 0)
        {
            string reason = $"User with Id = '{request.UserId}' spent '{request.Price}' on transport";
            await _userBalanceService.DoSubtractCoins(request.UserId, CoinType.SoftCoin, request.Price, reason, ct);
        }
        return true;
    }

    public async Task<bool> UseTaxi(UseTaxiRequest request, CancellationToken ct)
    {
        bool isValid = await _userBalanceService.ValidateUserOperationWithMoney(request.UserId, request.Price, ct);
        if (!isValid)
        {
            return false;
        }
        if (request.Price != 0)
        {
            string reason = $"User with Id = '{request.UserId}' spent '{request.Price}' on taxi";
            await _userBalanceService.DoSubtractCoins(request.UserId, CoinType.SoftCoin, request.Price, reason, ct);
        }
        return true;
    }

    public async Task<bool> AddTransportToUser(AddTransportToUserRequest request, CancellationToken ct)
    {
        bool isValid = await ValidateAddTransportToUser(request.UserId, request.TransportId, ct);
        if (!isValid)
        {
            return false;
        }
        await _userTransportRepository.AddUserTransport(request.UserId, request.TransportId, ct);
        return true;
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateAddTransportToUser(Guid userId, string transportId, CancellationToken ct)
    {
        bool isValid = await _userDataService.ValidateUser(userId, ct);
        if (!isValid)
        {
            return false;
        }        
        if (await _userTransportRepository.UserTransportExists(userId, transportId, ct))
        {
            _validationStorage.AddError(UserTransportAlreadyExists, $"User with Id {userId} has already purchased transport {transportId}");
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateUseTransport(Guid userId, string transportId, int price, CancellationToken ct)
    {
        bool isValid = await _userBalanceService.ValidateUserOperationWithMoney(userId, price, ct);
        if (!isValid)
        {
            return false;
        } 
        if (!await _userTransportRepository.UserTransportExists(userId, transportId, ct))
        {
            _validationStorage.AddError(UserTransportDoesNotExist, $"User with Id {userId} has not purchased transport {transportId} yet");
        }
        return _validationStorage.IsValid;
    }

    #endregion
}
