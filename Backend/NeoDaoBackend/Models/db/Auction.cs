namespace NeoDaoBackend.Models.db;

public class Auction
{
    public Guid AuctionId { get; set; }
    public DateTimeOffset LotsStartTime { get; set; }
    public DateTimeOffset AuctionStartTime { get; set; }
    public DateTimeOffset AuctionEndTime { get; set; }

    public virtual ICollection<AuctionLot> AuctionLots { get; set; }
}