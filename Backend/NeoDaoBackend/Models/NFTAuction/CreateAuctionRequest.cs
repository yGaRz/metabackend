namespace NeoDaoBackend.Models.NFTAuction;

public class CreateAuctionRequest
{
    public DateTimeOffset LotsStartTime {  get; set; }
    public DateTimeOffset AuctionStartTime { get; set; }
    public DateTimeOffset AuctionEndTime { get;set; }
}
