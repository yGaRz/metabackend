using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.NFTAuction;

public class CreateAuctionLotRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidGuid]
    public Guid ItemId { get; set; }
    [ValidGuid]
    public Guid AuctionId { get; set; }
    public CoinType CoinType { get; set; }
    [ValidPositiveDecimal]
    public decimal InitialPrice { get; set; }
}