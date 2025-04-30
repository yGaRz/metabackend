namespace NeoDaoBackend.Models.Balance;

public class BalanceTransactionDTO
{
    public decimal SoftDifference { get; set; }
    public string Reason { get; set; } = null!;
    public DateTimeOffset Created { get; set; }
}
