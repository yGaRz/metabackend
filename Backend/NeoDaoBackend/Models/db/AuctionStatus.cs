using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.db;

public enum AuctionStatus
{
    Waiting = 0,
    InAuction = 1,
    Sold = 2,
    Unsold = 3
}