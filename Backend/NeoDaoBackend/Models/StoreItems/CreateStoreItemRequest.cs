using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.StoreItems;

public class CreateStoreItemRequest
{
    [ValidNotEmptyString]
    public string UnrealId { get; set; }
    public StoreType StoreType { get; set; }
    public CoinType CoinType { get; set; }
    [ValidNotNegativeDecimal]
    public decimal Price { get; set; }

    public bool IsMultiPurchasable { get; set; }
}