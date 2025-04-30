namespace NeoDaoBackend.Models.Balance;

public class GetBalanceTransactionRequest
{
    public Guid UserId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set;}
}
