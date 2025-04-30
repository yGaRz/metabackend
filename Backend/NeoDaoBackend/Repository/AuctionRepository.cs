using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.NFTAuction;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Repository;

public class AuctionRepository : IAuctionRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public AuctionRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AuctionLotWithDetails>> GetAuctionLots(CancellationToken ct, params AuctionStatus[] statuses)
    {
        IQueryable<AuctionLot> query = _dbContext.AuctionLots;

        return await (from lot in query
                            join item in _dbContext.Items on lot.InventoryItemId equals item.ItemId
                            join user in _dbContext.Users on lot.CurrentBidUserId equals user.UserId into userJoin
                            from user in userJoin.DefaultIfEmpty() // Левое соединение
                            select new AuctionLotWithDetails
                            {
                                LotId = lot.LotId,
                                InventoryItemId = lot.InventoryItemId,
                                UserId = lot.UserId,
                                Created = lot.Created,
                                CoinType = lot.CoinType,
                                InitialPrice = lot.InitialPrice,
                                MetaforceNftId = item.MetaforceNftId,
                                NftDescription = item.NftDescription,
                                CurrentBidUserId = lot.CurrentBidUserId,
                                CurrentBidUserName = user != null ? user.UserName : null, // Проверка на null
                                CurrentPrice = lot.CurrentPrice,
                                LastUpdated = lot.LastUpdated
                            }).ToListAsync(ct);
    }
    
    public async Task<IEnumerable<AuctionLotWithDetails>> GetAuctionLotsByUserId(Guid userId, CancellationToken ct)
    {
        return await (from lot in _dbContext.AuctionLots
                      join item in _dbContext.Items on lot.InventoryItemId equals item.ItemId
                      join user in _dbContext.Users on lot.CurrentBidUserId equals user.UserId into userJoin
                      from user in userJoin.DefaultIfEmpty() // Это реализует левое соединение
                      where lot.UserId == userId
                      select new AuctionLotWithDetails
                      {
                          LotId = lot.LotId,
                          InventoryItemId = lot.InventoryItemId,
                          UserId = lot.UserId,
                          Created = lot.Created,
                          CoinType = lot.CoinType,
                          InitialPrice = lot.InitialPrice,
                          MetaforceNftId = item.MetaforceNftId,
                          NftDescription = item.NftDescription,
                          CurrentBidUserId = lot.CurrentBidUserId,
                          CurrentBidUserName = user != null ? user.UserName : null, // Если user null, то CurrentBidUserName тоже null
                          CurrentPrice = lot.CurrentPrice,
                          LastUpdated = lot.LastUpdated
                      }).ToListAsync(ct);
    }

    public async Task<AuctionLot?> GetAuctionLotItemByLotId(Guid lotId, CancellationToken ct)
    {
        return await _dbContext.AuctionLots
            .Where(e => e.LotId == lotId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AuctionLot?> GetAuctionLotItemByItemId(Guid ItemId, CancellationToken ct)
    {
        return await _dbContext.AuctionLots
            .Where(e => e.InventoryItemId == ItemId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Auction?> GetAuctionDetails(Guid auctionId, CancellationToken ct)
    {
        return await _dbContext.Auctions
            .Where(e => e.AuctionId == auctionId)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<AuctionLot?> GetBidForLot(Guid lotId, CancellationToken ct)
    {
        return await _dbContext.AuctionLots
            .Where(bid => bid.LotId == lotId)
            .OrderByDescending(bid => bid.CurrentPrice)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task UpdateAuctionLot(AuctionLot auctionLot, CancellationToken ct)
    {
        _dbContext.AuctionLots.Update(auctionLot);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAuctionLots(Guid auctionId, CancellationToken ct)
    {
        await _dbContext.AuctionLots.Where(ui => ui.AuctionId == auctionId).ExecuteDeleteAsync(ct);
    }

    public async Task<List<AuctionLot>> GetLatestAuctionWithHighestBids(CancellationToken ct)
    {
        // Находим последний аукцион
        var latestAuction = await _dbContext.Auctions
            .OrderByDescending(a => a.AuctionEndTime)
            .FirstOrDefaultAsync(ct);

        if (latestAuction == null)
        {
            return new List<AuctionLot>();
        }

        // Получаем все лоты для этого аукциона
        var auctionLots = await _dbContext.AuctionLots
            .Where(lot => lot.AuctionId == latestAuction.AuctionId)
            .ToListAsync(ct);

        return auctionLots;
    }
    
    public async Task<Auction?> GetLatestAuction(CancellationToken ct)
    {
        return await _dbContext.Auctions
            .OrderByDescending(a => a.AuctionEndTime)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<Auction?> GetLatestAuction(CancellationToken ct, bool activeOnly = false)
    {
        IQueryable<Auction> query = _dbContext.Auctions;
        
        return await query
            .OrderByDescending(a => a.AuctionEndTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task CreateAuctionLotItem(AuctionLot auctionLot, CancellationToken ct)
    {
        _dbContext.AuctionLots.Add(auctionLot);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Auction?> GetCurrentAuction(CancellationToken ct)
    {
        return await _dbContext.Auctions.OrderBy(x => x.AuctionEndTime).Where(x => x.AuctionEndTime > DateTimeOffset.UtcNow).FirstOrDefaultAsync(ct);
    }

    public async Task CreateAuction(CreateAuctionRequest request, CancellationToken ct)
    {
        _dbContext.Auctions.Add(new Auction()
        {
            AuctionEndTime = request.AuctionEndTime.UtcDateTime,
            AuctionStartTime = request.AuctionStartTime.UtcDateTime,
            LotsStartTime = request.LotsStartTime.UtcDateTime,
        });
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Auction>> GetAllAuctions(CancellationToken ct)
    {
        return await _dbContext.Auctions.OrderBy(x => x.AuctionEndTime).Where(x => x.AuctionEndTime > DateTimeOffset.UtcNow).ToListAsync();
    }

    public async Task<bool> AuctionExists(Guid auctionId, object ct)
    {
        return await _dbContext.Auctions.Where(x => x.AuctionId == auctionId).AnyAsync();
    }

    public async Task DeleteAuction(Guid auctionId, CancellationToken ct)
    {
        var auction = await _dbContext.Auctions.FirstAsync(x => x.AuctionId == auctionId);
        _dbContext.Auctions.Remove(auction);
        await _dbContext.SaveChangesAsync(ct);
    }
}