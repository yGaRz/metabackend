namespace NeoDaoBackend.Models.db;

public class UserFinancialTransaction
{
    public long TransactionId {  get; set; }
    public Guid UserId { get; set; }
    public CoinType CoinType { get; set; }
    public decimal AmountDifference { get; set; } 
    public string Reason { get; set; } = null!;
    public DateTimeOffset Created { get; set; }

    public virtual User User { get; set; } = null!;
}