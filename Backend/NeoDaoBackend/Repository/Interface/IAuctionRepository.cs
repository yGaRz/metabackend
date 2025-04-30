using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.NFTAuction;

namespace NeoDaoBackend.Repository.Interface;

public interface IAuctionRepository
{
    Task<IEnumerable<AuctionLotWithDetails>> GetAuctionLots(CancellationToken ct, params AuctionStatus[] statuses);
    Task<IEnumerable<AuctionLotWithDetails>> GetAuctionLotsByUserId(Guid userId, CancellationToken ct);
    Task<AuctionLot?> GetAuctionLotItemByLotId(Guid lotId, CancellationToken ct);
    Task<AuctionLot?> GetAuctionLotItemByItemId(Guid ItemId, CancellationToken ct);
    Task<Auction?> GetAuctionDetails(Guid auctionId, CancellationToken ct);
    Task<AuctionLot?> GetBidForLot(Guid lotId, CancellationToken ct);
    Task UpdateAuctionLot(AuctionLot auctionBid, CancellationToken ct);
    Task DeleteAuctionLots(Guid auctionId, CancellationToken ct);
    Task<List<AuctionLot>> GetLatestAuctionWithHighestBids(CancellationToken ct);
    Task<Auction?> GetLatestAuction(CancellationToken ct);
    Task CreateAuctionLotItem(AuctionLot auctionLot, CancellationToken ct);
    Task<Auction?> GetCurrentAuction(CancellationToken ct);
    Task CreateAuction(CreateAuctionRequest request, CancellationToken ct);
    Task<List<Auction>> GetAllAuctions(CancellationToken ct);
    Task<bool> AuctionExists(Guid auctionId, object ct);
    Task DeleteAuction(Guid auctionId, CancellationToken ct);
}