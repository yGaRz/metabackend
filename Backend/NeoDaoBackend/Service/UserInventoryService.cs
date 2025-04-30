using AutoMapper;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.InventoryItems;
using NeoDaoBackend.Models.NFTAuction;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;

namespace NeoDaoBackend.Service;

public class UserInventoryService
{
    private readonly ILogger<UserInventoryService> _logger;
    private readonly IValidationStorage _validationStorage;
    private readonly IUserInventoryRepository _userInventoryRepository;
    private readonly IUserDataRepository _userDataRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly StoresService _storesService;
    private readonly IMapper _mapper;
    
    public UserInventoryService(ILogger<UserInventoryService> logger, IValidationStorage validationStorage,
        IUserInventoryRepository userInventoryRepository, IUserDataRepository userDataRepository, IMapper mapper, 
        IAuctionRepository auctionRepository, StoresService storesService)
    {
        _logger = logger;
        _validationStorage = validationStorage;
        _userInventoryRepository = userInventoryRepository;
        _userDataRepository = userDataRepository;
        _auctionRepository = auctionRepository;
        _storesService = storesService;
        _mapper = mapper;
    }

    #region Actions

    public async Task<IEnumerable<GetItemsUserInventoryDTO>> GetItems(Guid userId, CancellationToken ct)
    {
        bool isValid = await ValidateGetItems(userId, ct);
        if (!isValid)
        {
            return null!;
        }

        IEnumerable<Item> userItems = await _userInventoryRepository.GetInventoryItemsByUserId(userId, ct);
        IEnumerable<AuctionLotWithDetails> nftSlots = await _auctionRepository.GetAuctionLotsByUserId(userId, ct);
        var result = new List<GetItemsUserInventoryDTO>();

        foreach (var item in userItems)
        {
            var dto = _mapper.Map<GetItemsUserInventoryDTO>(item);

            if (item.MetaforceNftId != null)
            {
                dto.MetaforceNftId = item.MetaforceNftId;
                dto.NftDescription = item.NftDescription;
                dto.IsReservedForAuction = false;

                AuctionLotWithDetails? slot = nftSlots.FirstOrDefault(ns => ns.InventoryItemId == item.ItemId);
                if (slot != null)
                {
                    dto.IsReservedForAuction = true;
                }
            }

            result.Add(dto);
        }

        return result;
    }

    
    public async Task<CreateItemUsersInventoryResponse> CreateItem(CreateItemPlayerInventoryRequest request, CancellationToken ct)
    {
        bool isValid = await ValidateCreateItem(request, ct);
        if (!isValid)
        {
            return null!;
        }

        Guid itemId = Guid.NewGuid();
        CreateItemUsersInventoryResponse item = new CreateItemUsersInventoryResponse()
        {
            ItemId = itemId
        };

        var itemDTO = new Item()
        {
            ItemId = itemId,
            UnrealItemId = request.item.UnrealItemId,
            Quantity = request.item.Quantity,
            ExpirationDate = request.item.ExpirationDate,
            Properties = request.item.Properties,
            MetaforceNftId = request.item.MetaforceNftId,
            NftDescription = request.item.NftDescription
        };

        await CreateItem(request.UserId, itemDTO, ct);
        _logger.LogInformation($"Successfully created item {itemId} for user {request.UserId}");
        return item;
    }

    public async Task<bool> UpdateItem(UpdateItemInventoryRequest request, CancellationToken ct)
    {
        bool isValid = await ValidateUpdateItem(request, ct);
        if (!isValid)
        {
            return false;
        }

        Item item = await _userInventoryRepository.GetItemById(request.ItemId, ct);
        item.Quantity = request.Quantity;
        item.Properties = request.Properties;

        await _userInventoryRepository.UpdateItem(item, ct);
        return true;
    }

    public async Task<bool> DeleteItem(DeleteItemInventoryRequest request, CancellationToken ct)
    {
        //TODO: Think about check when you grabbing items
        bool isValid = await ValidateDeleteItem(request, ct);
        if (!isValid)
        {
            return false;
        }

        Item item = await _userInventoryRepository.GetItemById(request.ItemId, ct);
        List<Item> items = await _userInventoryRepository.GetItemCollection(item.UnrealItemId, item.InventoryId, ct);
        if (items.Count == 1)
        {
            await _storesService.DeleteItemPurchased(item.UnrealItemId, request.UserId, ct);
        }
        await _userInventoryRepository.DeleteItem(item, ct);
        _logger.LogInformation($"Deleted Item for {request.UserId}: inventory {item.InventoryId}");
        return true;
    }

    private async Task CreateItem(Guid userId, Item item, CancellationToken ct)
    {
        var inventory = await _userInventoryRepository.GetInventoryByUserId(userId, ct);

        _logger.LogInformation($"Creating Item for {userId}: inventory {inventory.InventoryId}");

        item.InventoryId = inventory.InventoryId;
        await _userInventoryRepository.CreateItem(item, ct);
    }

    #endregion

    #region Validation

    public async Task<bool> ValidateGetItems(Guid userId, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(userId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, userId);
        }
        return _validationStorage.IsValid;
    }

    public async Task<bool> ValidateCreateItem(CreateItemPlayerInventoryRequest request, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(request.UserId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, request.UserId);
        }
        // TODO validate item type
        return _validationStorage.IsValid;
    }

    public async Task<bool> ValidateUpdateItem(UpdateItemInventoryRequest request, CancellationToken ct)
    {
        var item = await _userInventoryRepository.GetItemByIdOptional(request.ItemId, ct);
        if (item == null)
        {
            ValidationUtils.AddUnknownInventoryItemError(_validationStorage, request.ItemId);
            return false;
        }
        return _validationStorage.IsValid;
    }

    public async Task<bool> ValidateDeleteItem(DeleteItemInventoryRequest request, CancellationToken ct)
    {
        if (!await _userDataRepository.UserExists(request.UserId, ct))
        {
            ValidationUtils.AddUnknownUserError(_validationStorage, request.UserId);
        }
        var item = await _userInventoryRepository.GetItemByIdOptional(request.ItemId, ct);
        if (item == null)
        {
            ValidationUtils.AddUnknownInventoryItemError(_validationStorage, request.ItemId);
            return false;
        }
        return _validationStorage.IsValid;
    }

    #endregion
}