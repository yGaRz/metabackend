using System.Reflection.Metadata;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.NFTAuction;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Validation;
using static NeoDaoBackend.Util.EnvUtils;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Service;

public class AuctionService
{
    private readonly ILogger<AuctionService> _logger;
    private readonly IValidationStorage _validationStorage;
    private readonly IAuctionRepository _auctionRepository;
    private readonly UserDataService _userDataService;
    private readonly UserBalanceService _userBalanceService;
    private readonly IMapper _mapper;
    private readonly IUserInventoryRepository _userInventoryRepository;

    private decimal AuctionLotCommission { get; init; }
    private decimal FeeForListingAuctionItem { get; init; }

    public AuctionService(ILogger<AuctionService> logger, IValidationStorage validationStorage,
        IMapper mapper, IAuctionRepository auctionRepository, UserDataService userDataService,
        IUserInventoryRepository userInventoryRepository, UserBalanceService userBalanceService)
    {
        _logger = logger;
        _validationStorage = validationStorage;
        _auctionRepository = auctionRepository;
        _userDataService = userDataService;
        _mapper = mapper;
        _userInventoryRepository = userInventoryRepository;
        _userBalanceService = userBalanceService;

        AuctionLotCommission = (decimal)GetDoubleEnvVariable("AUCTION_LOT_COMMISSION_RATE");
        FeeForListingAuctionItem = (decimal)GetDoubleEnvVariable("FEE_FOR_LISTING_AUCTION_ITEM");
    }

    #region Action

    public async Task<IEnumerable<AuctionLotDTO>> GetAuctionLots(CancellationToken ct)
    {
        IEnumerable<AuctionLotWithDetails> auctionLots = await _auctionRepository.GetAuctionLots(ct, AuctionStatus.InAuction, AuctionStatus.Waiting);
        var auctionLotsDTO = _mapper.Map<List<AuctionLotDTO>>(auctionLots);
        return auctionLotsDTO;
    }

    public async Task<IEnumerable<AuctionLotDTO>?> GetUserAuctionLots(Guid userId, CancellationToken ct)
    {
        bool isRequestValid = await ValidateGetNftLots(userId, ct);
        if (!isRequestValid)
        {
            return null;
        }
        IEnumerable<AuctionLotWithDetails> auctionLots = await _auctionRepository.GetAuctionLotsByUserId(userId, ct);
        var auctionLotsDTO = _mapper.Map<List<AuctionLotDTO>>(auctionLots);
        return auctionLotsDTO;
    }

    public async Task<CreateAuctionLotResponse?> CreateAuctionLotItem(CreateAuctionLotRequest createAuctionLotRequest, CancellationToken ct)
    {
        bool isValid = await ValidateCreateAuctionLotItem(createAuctionLotRequest, ct);
        if (!isValid)
        {
            return null!;
        }

        var auctionLots = new AuctionLot()
        {
            LotId = Guid.NewGuid(),
            InventoryItemId = createAuctionLotRequest.ItemId,
            UserId = createAuctionLotRequest.UserId,
            Created = DateTimeOffset.UtcNow,
            CoinType = createAuctionLotRequest.CoinType,
            InitialPrice = createAuctionLotRequest.InitialPrice,
            AuctionId = createAuctionLotRequest.AuctionId
        };
        var reason = $"User with id = {createAuctionLotRequest.UserId} listed an Auction item with id = {createAuctionLotRequest.ItemId} " +
                     $"for auction. Initial price = {createAuctionLotRequest.InitialPrice}.";
        await _userBalanceService.DoSubtractCoins(createAuctionLotRequest.UserId, CoinType.HardCoin, FeeForListingAuctionItem, reason, ct);
        await _auctionRepository.CreateAuctionLotItem(auctionLots, ct);
        var nftLotResponse = new CreateAuctionLotResponse()
        {
            LotId = auctionLots.LotId,
        };
        return nftLotResponse;
    }

    public async Task<bool> CreateAuction(CreateAuctionRequest request, CancellationToken ct)
    {
        bool isValid = await ValidateCreateAuctionRequest(request, ct);
        if (!isValid)
        {
            return false;
        }

        await _auctionRepository.CreateAuction(request, ct);
        return true;
    }
    
    public async Task<bool> CreateAuctionBids(CreateAuctionBidsRequest auctionBidsRequest, CancellationToken ct)
    {
        bool isValid = await ValidateCreateAuctionBids(auctionBidsRequest, ct);
        if (!isValid)
        {
            return false;
        }

        var auctionLot = await _auctionRepository.GetBidForLot(auctionBidsRequest.LotId, ct);
        if (auctionLot != null)
        {
            var reason = $"User with id = {auctionLot.CurrentBidUserId} lost bids at Auction Lot = {auctionLot.LotId} " +
                            $"on auction. User's coins {auctionLot.CurrentPrice} come back on balance.";
            if (auctionLot.CurrentBidUserId != null)
            {
                await _userBalanceService.DoAddCoins(auctionLot.CurrentBidUserId.Value, auctionLot.CoinType, auctionLot.CurrentPrice.Value, reason, ct);
            }
            auctionLot.CurrentPrice = auctionBidsRequest.CurrentPrice;
            auctionLot.CurrentBidUserId = auctionBidsRequest.UserId;
            auctionLot.LastUpdated = DateTimeOffset.UtcNow;
        }
        await _auctionRepository.UpdateAuctionLot(auctionLot, ct);
        var reasonBids = $"User with id = {auctionBidsRequest.UserId} make bids at NFT Lot = {auctionBidsRequest.LotId} on auction.";
        await _userBalanceService.DoSubtractCoins(auctionBidsRequest.UserId, auctionBidsRequest.CoinType, auctionBidsRequest.CurrentPrice, reasonBids, ct);
        return true;
    }

    public async Task<Auction?> GetCurrentAuction(CancellationToken ct)
    {
        var result = await _auctionRepository.GetCurrentAuction(ct);
        if (result == null)
        {
            _validationStorage.AddError(ErrorCode.NoEnableAuction, $"There are currently no active auctions");
        }
        return result;
    }

    public async Task<List<Auction>> GetAllAuction(CancellationToken ct)
    {
        return await _auctionRepository.GetAllAuctions(ct);
    }
    
    // Method only for Jobs
    public async Task СalculatingAuctionLots(CancellationToken ct)
    {
        Auction? auction = await _auctionRepository.GetLatestAuction(ct);
        if (auction?.AuctionEndTime < DateTimeOffset.UtcNow)
        {
            var auctionLots = await _auctionRepository.GetLatestAuctionWithHighestBids(ct);
            if (!auctionLots.IsNullOrEmpty())
            {
                _logger.LogInformation("Finishing auction...");
                decimal commissionRate = AuctionLotCommission; // N% комиссия

                foreach (var lot in auctionLots)
                {
                    if (lot.CurrentBidUserId != Guid.Empty &&
                        lot.CurrentPrice > 0) // Проверка наличия ставки и пользователя
                    {
                        decimal totalPrice = lot.CurrentPrice.Value;
                        decimal feeAmount = totalPrice * commissionRate;
                        decimal netAmount = totalPrice - feeAmount;

                        // Проверка на случай, если сумма комиссии или расчет неточен
                        if (netAmount > 0)
                        {
                            _logger.LogInformation(
                                $"Processing lot {lot.LotId} with highest bid of {totalPrice}. Commission: {feeAmount} ({commissionRate * 100}%), Net amount: {netAmount}.");

                            string reasonBids =
                                $"Deducting {feeAmount} as commission and {netAmount} as net amount for NFT Lot with id {lot.LotId}.";
                            await _userBalanceService.DoAddCoins(lot.UserId, lot.CoinType, netAmount, reasonBids, ct);

                            var item = await _userInventoryRepository.GetItemById(lot.InventoryItemId.Value, ct);
                            var inventory =
                                await _userInventoryRepository.GetInventoryByUserId(lot.CurrentBidUserId.Value, ct);
                            item.InventoryId = inventory.InventoryId;
                            await _userInventoryRepository.UpdateItem(item, ct);
                        }
                        else
                        {
                            _logger.LogWarning(
                                $"Lot {lot.LotId} resulted in a non-positive net amount after commission. Skipping.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Lot {lot.LotId} has no valid bids.");
                    }
                }

                await _auctionRepository.DeleteAuctionLots(auctionLots[0].AuctionId, ct);
                _logger.LogInformation("Auction finished successfully.");
            }
        }
    }

    public async Task<bool> DeleteAuction(Guid auctionId, CancellationToken ct)
    {
        bool isValid = await ValidateAuction(auctionId, ct);
        if (!isValid)
        {
            return false;
        }

        await _auctionRepository.DeleteAuction(auctionId, ct);
        return true;
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateGetNftLots(Guid userId, CancellationToken ct)
    {
        return await _userDataService.ValidateUser(userId, ct);
    }

    private async Task<bool> ValidateCreateAuctionLotItem(CreateAuctionLotRequest createAuctionLotRequest, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(createAuctionLotRequest.UserId, ct))
        {
            return false;
        }

        // Получение предмета и инвентаря пользователя
        var item = await _userInventoryRepository.GetItemByIdOptional(createAuctionLotRequest.ItemId, ct);
        var inventory = await _userInventoryRepository.GetInventoryByUserId(createAuctionLotRequest.UserId, ct);
        if (item == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownInventoryItem, $"NFT Item with id {createAuctionLotRequest.ItemId} does not exist");
            return false;
        }
        else if (item.InventoryId != inventory.InventoryId)
        {
            _validationStorage.AddError(ErrorCode.ItemOwnershipViolation, $"You cannot sell an NFT Item with id {createAuctionLotRequest.ItemId} that does not belong to you.");
            return false;
        }

        // Проверка, существует ли уже слот с этим предметом
        var nftSlot = await _auctionRepository.GetAuctionLotItemByItemId(createAuctionLotRequest.ItemId, ct);
        if (nftSlot != null)
        {
            _validationStorage.AddError(ErrorCode.NFTItemSlotAlreadyExists, $"NFT Item with id {createAuctionLotRequest.ItemId} already exists");
            return false;
        }

        // Проверка временных интервалов
        var auction = await _auctionRepository.GetAuctionDetails(createAuctionLotRequest.AuctionId, ct);  // Метод для получения данных аукциона
        if (auction != null)
        {
            var currentTime = DateTimeOffset.UtcNow;

            if (currentTime < auction.LotsStartTime || currentTime > auction.AuctionStartTime)
            {
                _validationStorage.AddError(ErrorCode.InvalidAuctionTime, "The current time is not within the auction time range.");
                return false;
            }
        }
        else
        {
            _validationStorage.AddError(ErrorCode.AuctionNotFound, "Auction does not found.");
            return false;
        }

        var userBalance = await _userBalanceService.GetUserBalance(createAuctionLotRequest.UserId, ct);
        if (userBalance.HardAmount < FeeForListingAuctionItem)
        {
            _validationStorage.AddError(ErrorCode.InsufficientHardCoins, $"Insufficient balance to cover the {FeeForListingAuctionItem} HardCoins. Available: {userBalance.HardAmount}");
            return false;
        }
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateCreateAuctionBids(CreateAuctionBidsRequest auctionBidsRequest, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(auctionBidsRequest.UserId, ct))
        {
            return false;
        }

        // Проверка, существует ли лот с этим предметом
        var nftLot = await _auctionRepository.GetAuctionLotItemByLotId(auctionBidsRequest.LotId, ct);
        if (nftLot == null)
        {
            _validationStorage.AddError(ErrorCode.UnknownNFTSlot, $"NFT Lot with id {auctionBidsRequest.LotId} does not exist");
            return false;
        }
        if (nftLot?.CoinType != auctionBidsRequest.CoinType)
        {
            _validationStorage.AddError(ErrorCode.WrongCoinType, $"Coin Type {auctionBidsRequest.CoinType} is not correct");
            return false;
        }

        // Получение данных аукциона
        var auction = await _auctionRepository.GetAuctionDetails(nftLot.AuctionId, ct);
        if (auction == null)
        {
            _validationStorage.AddError(ErrorCode.AuctionNotFound, $"Auction for Lot with id {auctionBidsRequest.LotId} does not exist");
            return false;
        }
        else
        {
            var currentTime = DateTimeOffset.UtcNow;

            // Проверка, что текущая дата находится между AuctionStartTime и AuctionEndTime
            if (currentTime < auction.AuctionStartTime || currentTime > auction.AuctionEndTime)
            {
                _validationStorage.AddError(ErrorCode.InvalidAuctionTime, "The current time is not within the auction's active period.");
                return false;
            }
        }
        
        // Проверка, баланс самого покупателя
        var userBalance = await _userBalanceService.GetUserBalance(auctionBidsRequest.UserId, ct);
        if (auctionBidsRequest.CoinType == CoinType.HardCoin)
        {
            if (userBalance.HardAmount < auctionBidsRequest.CurrentPrice)
            {
                _validationStorage.AddError(ErrorCode.NotEnoughCoins, $"User with Id {auctionBidsRequest.UserId} does not have enough Hard Coins");
                return false;
            }
        }
        else
        {
            if (userBalance.BitForceAmount < auctionBidsRequest.CurrentPrice)
            {
                _validationStorage.AddError(ErrorCode.NotEnoughCoins, $"User with Id {auctionBidsRequest.UserId} does not have enough BitForce Coins");
                return false;
            }
        }

        // Проверка, является ли данная ставка больше предыдущей
        var auctionBid = await _auctionRepository.GetBidForLot(auctionBidsRequest.LotId, ct);
        if (auctionBid != null)
        {
            // Вычисление минимальной необходимой ставки: на 5% больше предыдущей
            var minRequiredBid = auctionBid.CurrentPrice * BidsMinimumValue;
            if (auctionBidsRequest.CurrentPrice < minRequiredBid)
            {
                _validationStorage.AddError(ErrorCode.NotEnoughCoins, $"Your bid must be at least 5% greater than the last bid. Minimum required bid: {minRequiredBid}");
                return false;
            }
        }
        else
        {
            // Если нет предыдущих ставок, просто проверяем, что текущая ставка выше нуля или начальной цены
            if (auctionBidsRequest.CurrentPrice < nftLot.InitialPrice)
            {
                _validationStorage.AddError(ErrorCode.NotEnoughCoins, $"Your bid must be greater than the starting price of the lot.");
                return false;
            }
        }
        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateCreateAuctionRequest(CreateAuctionRequest request, CancellationToken ct)
    {
        if (request.LotsStartTime >= request.AuctionStartTime || request.AuctionStartTime >= request.AuctionEndTime)
        {
            _validationStorage.AddError(ErrorCode.ErrorDateTime, $"Lots start time must be before the start of the auction");
            return false;
        }
        if (request.LotsStartTime <= DateTimeOffset.UtcNow)
        {
            _validationStorage.AddError(ErrorCode.ErrorDateTime, $"Lots start time must be before time now");
            return false;
        }
        var auctionList = await GetAllAuction(ct);
        auctionList = auctionList.OrderBy(x => x.LotsStartTime).ToList();
        foreach (var auction in auctionList)
        {
            if (IsOverlapping(request, auction))
            {
                _validationStorage.AddError(ErrorCode.ErrorDateTime, $"Auction hours should not overlap");
                return false;
            }
        }
        return true;
    }

    private async Task<bool> ValidateAuction(Guid auctionId, object ct)
    {
        if (!await _auctionRepository.AuctionExists(auctionId, ct))
        {
            _validationStorage.AddError(ErrorCode.AuctionNotFound, $"Auction with id = '{auctionId}' is not found");
        }
        return _validationStorage.IsValid;
    }

    private bool IsOverlapping(CreateAuctionRequest request, Auction auction)
    {
        //Проверка, чтобы временные интервалы не пересекались
        return request.LotsStartTime >= auction.LotsStartTime && request.LotsStartTime <= auction.AuctionEndTime ||
                request.AuctionEndTime >= auction.LotsStartTime && request.AuctionEndTime <= auction.AuctionEndTime ||
                request.AuctionStartTime >= auction.LotsStartTime && request.AuctionStartTime <= auction.AuctionEndTime ||
                auction.LotsStartTime >= request.LotsStartTime && auction.LotsStartTime <= request.AuctionEndTime ||
                auction.AuctionEndTime >= request.LotsStartTime && auction.AuctionEndTime <= request.AuctionEndTime ||
                auction.AuctionStartTime >= request.LotsStartTime && auction.AuctionStartTime <= request.AuctionEndTime;
    }

    #endregion
}