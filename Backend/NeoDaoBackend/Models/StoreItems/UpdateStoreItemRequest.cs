using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.StoreItems;

public class UpdateStoreItemRequest
{
    [ValidGuid]
    public Guid InternalId { get; set; }
    [ValidNotEmptyString]
    public string UnrealId { get; set; }
    public StoreType StoreType { get; set; }
    public CoinType CoinType { get; set; }
    [ValidNotNegativeDecimal]
    public decimal Price { get; set; }
    public bool IsMultiPurchasable { get; set; }
}