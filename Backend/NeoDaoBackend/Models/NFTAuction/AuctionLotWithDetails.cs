using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.NFTAuction;

public class AuctionLotWithDetails
{
    public Guid LotId { get; set; }
    public Guid? InventoryItemId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset Created { get; set; }
    public CoinType CoinType { get; set; }
    public decimal InitialPrice { get; set; }
    public string? MetaforceNftId { get; set; }
    public string? NftDescription { get; set; }
    public Guid? CurrentBidUserId { get; set; }
    public string? CurrentBidUserName { get; set; }
    public decimal? CurrentPrice { get; set; }
    public DateTimeOffset? LastUpdated { get; set; }
}