using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.StoreItems;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Validation;
using static NeoDaoBackend.Util.EnvUtils;
using NeoDaoBackend.Models;

namespace NeoDaoBackend.Service;

public class StoresService
{
    private readonly ILogger<StoresService> _logger;
    private readonly UserBalanceService _userBalanceService;
    private readonly IValidationStorage _validationStorage;
    private readonly UserDataService _userDataService;
    private readonly IStoreRepository _storeRepository;
    private readonly IUserInventoryRepository _userInventoryRepository;
    
    private double PurchaseRate { get; init; }
    private double SaleRate { get; init; }

    public StoresService(ILogger<StoresService> logger, UserBalanceService userBalanceService, 
        IValidationStorage validationStorage, UserDataService userDataService, 
        IStoreRepository storeRepository, IUserInventoryRepository userInventoryRepository)
    {
        _logger = logger;
        _userBalanceService = userBalanceService;
        _validationStorage = validationStorage;
        _userDataService = userDataService;
        _storeRepository = storeRepository;
        _userInventoryRepository = userInventoryRepository;
        
        PurchaseRate = GetDoubleEnvVariable("PURCHASE_RATE");
        SaleRate = GetDoubleEnvVariable("SALE_RATE");
    }

    #region Actions

    public async Task<IEnumerable<StoreItemsDTO>> GetStoreItems(StoreType storeType, CancellationToken ct)
    {
        IEnumerable<StoreItem> storeItems = await _storeRepository.GetStoreItemsByType(storeType, ct);
        
        // Осуществляем маппинг с расчетом стоимости предмета.
        var storeItemsDtos = storeItems.Select(item => StoreItemsDTO.FromStoreItem(item, PurchaseRate, SaleRate)).ToList();
        return storeItemsDtos;
    }
    
    public async Task<IEnumerable<UserStoreItemsDTO>> GetUserStoreItems(Guid userId, List<string> unrealIds, CancellationToken ct)
    {
        bool valid = await ValidateStoreItemIds(userId, unrealIds, ct);
        if (!valid)
        {
            return Enumerable.Empty<UserStoreItemsDTO>();
        }
        IEnumerable<StoreItem> storeItems = await _storeRepository.GetStoreItems(unrealIds, ct);
        IEnumerable<string> purchasedItems = await _storeRepository.GetUserStorePurchases(userId, unrealIds, ct);
        
        // Осуществляем маппинг с расчетом стоимости предмета.
        var storeItemsDtos = storeItems.Select(item => UserStoreItemsDTO.FromUserStoreItem(item,PurchaseRate,SaleRate)).ToList();
        foreach (var item in storeItemsDtos)
        {
            item.IsBought = purchasedItems.Contains(item.UnrealId);
        }
        return storeItemsDtos;
    }

    public async Task<bool> BuyStoreItem(BuyStoreItemRequest storeItemRequest, CancellationToken ct)
    {
        bool isValid = await ValidateBuyStoreItem(storeItemRequest.UserId, storeItemRequest.UnrealId,
            storeItemRequest.Count, ct);
        if (!isValid)
        {
            return false;
        }

        await _storeRepository.BeginTransaction(ct);
        try
        {
            StoreItem? storeItem = await _storeRepository.GetStoreItemByUnrealId(storeItemRequest.UnrealId, ct);
            var userStoresPurchase = new UserStorePurchase()
            {
                UserId = storeItemRequest.UserId,
                StoreItemId = storeItem.InternalId
            };
            decimal totalPrice = storeItem.StoreType == StoreType.FreeStore
                ? (storeItem.Price * (decimal)PurchaseRate) * storeItemRequest.Count
                : storeItem.Price * storeItemRequest.Count;

            if (!await _storeRepository.HasUserAlreadyPurchased(storeItemRequest.UserId, storeItem.InternalId, ct))
            {
                if (storeItem.StoreType != StoreType.NftStore)
                {
                    _storeRepository.AddUserStoreItemPurchaseNoSave(userStoresPurchase);
                }
            }

            var reason =
                $"User with id = {storeItemRequest.UserId} was bought: StoreType = '{storeItem.StoreType.ToString()}', StoreItem= '{storeItem.InternalId}', " +
                $"Count = {storeItemRequest.Count}, TotalPrice = {totalPrice}";
            if (storeItem.CoinType == CoinType.HardCoin)
            {
                await _userBalanceService.DoSubtractCoins(storeItemRequest.UserId, CoinType.HardCoin, totalPrice,
                    reason, ct);
            }
            else
            {
                await _userBalanceService.DoSubtractCoins(storeItemRequest.UserId, CoinType.SoftCoin, totalPrice,
                    reason, ct);
            }

            await _storeRepository.CommitTransaction(ct);
            return true;
        }
        catch (Exception)
        {
            await _storeRepository.RollbackTransaction(ct);
            throw; // Пробрасываем исключение дальше
        }
    }

    public async Task<bool> SellStoreItem(BuyStoreItemRequest storeItemRequest, CancellationToken ct)
    {
        bool isValid = await ValidateSellStoreItem(storeItemRequest.UserId, storeItemRequest.UnrealId, storeItemRequest.Count, ct);
        if (!isValid)
        {
            return false;
        }
        User user = await _userDataService.GetById(storeItemRequest.UserId, ct);
        StoreItem? storeItem = await _storeRepository.GetStoreItemByUnrealId(storeItemRequest.UnrealId, ct);

        decimal totalPrice = storeItem.StoreType == StoreType.FreeStore 
            ? ((storeItem.Price * (decimal)SaleRate) * storeItemRequest.Count) 
            : storeItem.Price * storeItemRequest.Count;
        var reason = $"User with id = {user.UserId} sold: StoreType = '{storeItem.StoreType.ToString()}', StoreItem = '{storeItem.InternalId}', " +
                     $"Count = {storeItemRequest.Count}, TotalPrice = {totalPrice}";
        await _userBalanceService.DoAddCoins(storeItemRequest.UserId, CoinType.SoftCoin ,totalPrice, reason, ct);
        return true;
    }
    
    public async Task<CreateStoreItemResponse?> CreateStoreItem(CreateStoreItemRequest createStoreItemRequest, CancellationToken ct)
    {
        bool isValid = await ValidateCreateStoreItem(createStoreItemRequest.UnrealId, ct);
        if (!isValid)
        {
            return null;
        }
        
        Guid internalId = Guid.NewGuid();
        var storeItem = new StoreItem()
        {
            InternalId = internalId,
            UnrealId = createStoreItemRequest.UnrealId,
            StoreType = createStoreItemRequest.StoreType,
            CoinType = createStoreItemRequest.CoinType,
            Price = createStoreItemRequest.Price,
            IsMultiPurchasable = createStoreItemRequest.IsMultiPurchasable
        };
        await _storeRepository.CreateStoreItem(storeItem, ct);
        return new CreateStoreItemResponse { InternalId = internalId };
    }
    
    public async Task<bool> UpdateStoreItem(UpdateStoreItemRequest updateStoreItemRequest, CancellationToken ct)
    {
        bool isValid = await ValidateUpdateStoreItem(updateStoreItemRequest.InternalId, updateStoreItemRequest.UnrealId, ct);
        if (!isValid)
        {
            return false;
        }

        var storeItem = await _storeRepository.GetStoreItemByInternalId(updateStoreItemRequest.InternalId, ct);
        storeItem.UnrealId = updateStoreItemRequest.UnrealId;
        storeItem.StoreType = updateStoreItemRequest.StoreType;
        storeItem.CoinType = updateStoreItemRequest.CoinType;
        storeItem.Price = updateStoreItemRequest.Price;
        storeItem.IsMultiPurchasable = updateStoreItemRequest.IsMultiPurchasable;
        await _storeRepository.UpdateStoreItem(storeItem, ct);
        return true;
    }
    
    public async Task<bool> DeleteStoreItem(Guid internalId, CancellationToken ct)
    {
        bool isValid = await ValidateDeleteStoreItem(internalId, ct);
        if (!isValid)
        {
            return false;
        }

        var storeItem = await _storeRepository.GetStoreItemByInternalId(internalId, ct);
        await _storeRepository.DeleteStoreItem(storeItem, ct);
        return true;
    }
    
    public async Task DeleteItemPurchased(string unrealId, Guid userId, CancellationToken ct)
    {
        StoreItem storeItem = await _storeRepository.GetStoreItemByUnrealId(unrealId, ct);
        if (storeItem is not null)
        {
            UserStorePurchase? userStorePurchase = await _storeRepository.GetUserPurchased(storeItem.InternalId, userId, ct);
            if (userStorePurchase is not null)
            {
                _storeRepository.DeleteItemPurchased(userStorePurchase);
            }
        }
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateBuyStoreItem(Guid userId, string unrealId, int count, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        StoreItem? storeItem = await _storeRepository.GetStoreItemByUnrealId(unrealId, ct);
        if (storeItem == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownStoreItem, $"StoreItem with id {unrealId} does not exist");
            return false;
        }

        decimal price = storeItem.StoreType == StoreType.FreeStore 
            ? (storeItem.Price * (decimal)PurchaseRate * count) // Умножаем на коэффициент PurchaseRate
            : storeItem.Price * count; // Обычный расчет для остальных типов
        
        if (storeItem.StoreType == StoreType.NftStore)
        {
            if (count != Constants.CountNFTItemStore)
            {
                _validationStorage.AddError(ErrorCode.NFTIsMultiPurchasable, $"User with ID {userId} can not buy multiple NFT Item {unrealId}.");
                return false;
            }
            bool isValid = await _userBalanceService.ValidateUserOperationWithHardCoin(userId, price, ct);
            if (!isValid) { 
                return false;
            }
        }
        else
        {
            bool isValid = await _userBalanceService.ValidateUserOperationWithMoney(userId, price, ct);
            if (!isValid) {
                return false;
            }
            if (storeItem.IsMultiPurchasable)
            {
                return _validationStorage.IsValid;
            }
            if (await _storeRepository.HasUserAlreadyPurchased(userId, storeItem.InternalId, ct))
            {
                _validationStorage.AddError(ErrorCode.StoreItemIsAlreadyBought, $"User with ID {userId} has already purchased store_item with ID {unrealId}.");
            }
        }
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateSellStoreItem(Guid userId, string unrealId, int count, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        StoreItem? storeItem = await _storeRepository.GetStoreItemByUnrealId(unrealId, ct);
        if (storeItem == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownStoreItem, $"StoreItem with id {unrealId} does not exist");
            return false;
        }
        
        if (storeItem.StoreType == StoreType.NftStore || storeItem.CoinType == CoinType.HardCoin || storeItem.CoinType == CoinType.BitForceCoin)
        {
            _validationStorage.AddError(ErrorCode.NFTCanNotBeSelled, $"User with ID {userId} can not sell NFT Item {unrealId}.");
            return false;
        }
        
        Inventory inventory = await _userInventoryRepository.GetInventoryByUserId(userId, ct);
        Item? item = await _userInventoryRepository.GetItemByUnrealIdFromInventory(unrealId, inventory.InventoryId, ct);
        if (item == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownInventoryItem, $"Inventory item {unrealId} does not exist");
        }
        if (item.Quantity < count)
        {
            _validationStorage.AddError(ErrorCode.NotEnoughItem, $"User with ID {userId} has already purchased store_item with ID {unrealId}.");
        }
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateCreateStoreItem(string unrealId, CancellationToken ct)
    {
        StoreItem? storeItem = await _storeRepository.GetStoreItemByUnrealId(unrealId, ct);
        if (storeItem != null)
        {
            _validationStorage.AddError(ErrorCode.StoreItemAlreadyExists, $"StoreItem with id {unrealId} already exists");
        }
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateUpdateStoreItem(Guid internalId, string unrealId, CancellationToken ct)
    {
        StoreItem? storeItemByInternalId = await _storeRepository.GetStoreItemByInternalId(internalId, ct);
        if (storeItemByInternalId == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownStoreItem, $"StoreItem with internal id {internalId} does not exist");
        }
        StoreItem? storeItem = await _storeRepository.GetStoreItemByUnrealId(unrealId, ct);
        if (storeItem != null && storeItem.InternalId != internalId)
        {
            _validationStorage.AddError(ErrorCode.StoreItemAlreadyExists, $"StoreItem with id {unrealId} already exists");
        }
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateDeleteStoreItem(Guid internalId, CancellationToken ct)
    {
        StoreItem? storeItem = await _storeRepository.GetStoreItemByInternalId(internalId, ct);
        if (storeItem == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownStoreItem, $"StoreItem with internal id {internalId} does not exist");
            return false;
        }
        bool hasPurchases = await _storeRepository.IsStoreItemOwned(internalId, ct);
        if (hasPurchases)
        {
            _validationStorage.AddError(ErrorCode.CannotDeleteOwnedStoreItem, $"StoreItem with internal id {internalId} has associated purchases and cannot be deleted");
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateStoreItemIds(Guid userId, List<string> unrealIds, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        
        IEnumerable<StoreItem> storeItems = await _storeRepository.GetStoreItems(unrealIds, ct);
        var storeItemsDict = storeItems.ToDictionary(e => e.UnrealId);
        if (storeItemsDict.Count != unrealIds.Count)
        {
            var foundStoreItemsIds = storeItemsDict.Keys.ToList();
            var missingStoreItemsIds = unrealIds.Except(foundStoreItemsIds).ToList();

            // Add an error for missing storeItem IDs
            string missingIds = string.Join(", ", missingStoreItemsIds);
            _validationStorage.AddError(ErrorCode.UnknownStoreItem, $"StoreItem with IDs {missingIds} do not exist");
        }
        return _validationStorage.IsValid;
    }
    #endregion
}