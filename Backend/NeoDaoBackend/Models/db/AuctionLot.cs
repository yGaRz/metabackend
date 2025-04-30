namespace NeoDaoBackend.Models.db;

public class AuctionLot
{
    public Guid LotId { get; set; }
    public Guid? InventoryItemId { get; set; }
    public Guid UserId { get; set; }
    public Guid AuctionId { get; set; }
    public DateTimeOffset Created { get; set; }
    public CoinType CoinType { get; set; }
    public decimal InitialPrice { get; set; }

    // Поля из AuctionBid, которые могут быть null
    public Guid? CurrentBidUserId { get; set; }  // Пользователь, сделавший текущую ставку
    public decimal? CurrentPrice { get; set; }    // Текущая цена (ставка)
    public DateTimeOffset? LastUpdated { get; set; } // Время последнего обновления ставки

    // Навигационные свойства
    public virtual Item InventoryItem { get; set; }
    public virtual User User { get; set; } // Владелец лота
    public virtual Auction Auction { get; set; }  // Аукцион

    // Текущий пользователь, сделавший ставку
    public virtual User CurrentBidUser { get; set; }
}
