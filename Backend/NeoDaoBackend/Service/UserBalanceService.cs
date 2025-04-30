using NeoDaoBackend.Extensions;
using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Balance;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;

namespace NeoDaoBackend.Service;

public class UserBalanceService : AbstractWebSocketService<BalanceEndpointKind>
{
    private readonly UserDataService _userDataService;
    private readonly IUserBalanceRepository _userBalanceRepository;
    private readonly UserRelationsService _userRelationsService;

    public UserBalanceService(
        ILogger<UserBalanceService> logger,
        IValidationStorage validationStorage,
        JsonSerializerSettings jsonSerializerSettings,
        UserDataService userDataService,
        IUserBalanceRepository userBalanceRepository,
        UserRelationsService userRelationsService,
        NotificationService notificationService) :
        base(EndpointCategory.UserBalance, validationStorage, logger, jsonSerializerSettings, notificationService)
    {
        _userDataService = userDataService;
        _userBalanceRepository = userBalanceRepository;
        _userRelationsService = userRelationsService;
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, BalanceEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case BalanceEndpointKind.GetBalance:
                outputMessage = await GetUserBalance(user.UserId!.Value, ct);
                break;
            default:
                _validationStorage.AddError(ErrorCode.WrongEndpointKind, $"Unknown event type for category \"{_endpointCategory}\": \"{data}\"");
                break;
        }
        return outputMessage;
    }

    #region Actions

    public async Task<UserBalanceResponse> GetUserBalance(Guid userId, CancellationToken ct)
    {
        bool isValid = await _userDataService.ValidateUser(userId, ct);
        if (!isValid)
        {
            return null!;
        }

        var balance = await _userBalanceRepository.GetUserBalance(userId, ct);
        return new UserBalanceResponse
        {
            SoftAmount = balance.SoftAmount,
            BitForceAmount = balance.BitForceAmount,
            HardAmount = balance.HardAmount
        };
    }

    public async Task<bool> AddCoinsToUserBalance(AddUserBalanceRequest userBalanceData, CancellationToken ct)
    {
        bool isValid = await _userDataService.ValidateUser(userBalanceData.UserId, ct);
        if (!isValid)
        {
            return false;
        }

        await DoAddCoins(userBalanceData.UserId, userBalanceData.CoinType, userBalanceData.Amount, userBalanceData.Reason, ct);
        return true;
    }

    public async Task DoAddCoins(Guid userId, CoinType coinType, decimal amount, string reason, CancellationToken ct)
    {
        await _userBalanceRepository.UpdateBalance(userId, coinType, amount, reason, ct);
        _logger.LogInformation("Successfully added {CoinType} = CoinType : {Amount} = Amount to user = {UserId} balance", coinType.ToString(), amount, userId);
        var balance = await GetUserBalance(userId, ct);
        await _notificationService.AddNotificationToSingleUser(userId, balance, _endpointCategory.ToString(), BalanceEndpointKind.CurrencyChanged.ToString(), ct);
    }
    
    public async Task DoSubtractCoins(Guid userId, CoinType coinType, decimal amount, string reason, CancellationToken ct)
    {
        await _userBalanceRepository.UpdateBalance(userId, coinType, -amount, reason, ct);
        _logger.LogInformation("Successfully subtracted {Amount} {CoinType} from user {UserId} balance", amount, coinType.ToString(), userId);
        var balance = await GetUserBalance(userId, ct);
        await _notificationService.AddNotificationToSingleUser(userId, balance, _endpointCategory.ToString(), BalanceEndpointKind.CurrencyChanged.ToString(), ct);
    }
    
    public async Task<IEnumerable<BalanceTransactionDTO>> GetUserBalanceTransaction(GetBalanceTransactionRequest request, CancellationToken ct)
    {
        if (!await ValidateGetUserBalanceTransactionAsync(request.UserId, request.From, request.To, ct))
        {
            return null!;
        }

        return await _userBalanceRepository.GetTransactions(request.UserId, request.From, request.To, ct);
    }

    #endregion

    #region Validation

    public async Task<bool> ValidateUserOperationWithMoney(Guid userId, decimal softAmount, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        if (!await _userBalanceRepository.DoesUserHaveEnoughSoftCoins(userId, softAmount, ct))
        {
            _validationStorage.AddError(ErrorCode.NotEnoughSoftCoins, $"User with Id {userId} does not have enough soft coins");
            return false;
        }
        return true;
    }
    
    public async Task<bool> ValidateUserOperationWithHardCoin(Guid userId, decimal hardAmount, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        if (!await _userBalanceRepository.DoesUserHaveEnoughHardCoins(userId, hardAmount, ct))
        {
            _validationStorage.AddError(ErrorCode.NotEnoughHardCoins, $"User with Id {userId} does not have enough hard coins");
            return false;
        }
        return true;
    }

    private async Task<bool> ValidateGetUserBalanceTransactionAsync(Guid userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        if (from != null && (from >= DateTimeOffset.UtcNow || to != null && from >= to))
        {
            _validationStorage.AddError(ErrorCode.ErrorDateTime, $"Interval is not valid");
        }
        return _validationStorage.IsValid;
    }

    #endregion
}