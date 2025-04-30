using NeoDaoBackend.Models.db;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Balance;

public class AddUserBalanceRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    public CoinType CoinType { get; set; }
    [ValidPositiveDecimal]
    public decimal Amount { get; set; }
    [ValidNotEmptyString]
    public string Reason { get; set; } = null!;
}