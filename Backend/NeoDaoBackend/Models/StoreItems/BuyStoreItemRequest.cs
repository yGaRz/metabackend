using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.StoreItems;

public class BuyStoreItemRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotEmptyString]
    public string UnrealId { get; set; }
    [ValidPositiveInteger]
    public int Count { get; set; }
}