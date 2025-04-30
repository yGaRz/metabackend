using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.NFTAuction;

public class CreateAuctionBidsRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidGuid]
    public Guid LotId { get; set; }
    public CoinType CoinType { get; set; }
    [ValidPositiveDecimal]
    public decimal CurrentPrice { get; set; }
}