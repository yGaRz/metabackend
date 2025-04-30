using NeoDaoBackend.Models.Balance;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public interface IUserBalanceRepository
{
    Task<UserBalance> GetUserBalance(Guid userId, CancellationToken ct);
    Task UpdateBalance(Guid userId, CoinType coinType, decimal amount, string reason, CancellationToken ct);
    void CreateBalanceNoSave(Guid userId, int softAmount, CancellationToken ct);
    Task<bool> DoesUserHaveEnoughSoftCoins(Guid userId, decimal softAmount, CancellationToken ct);
    Task<bool> DoesUserHaveEnoughHardCoins(Guid userId, decimal hardAmount, CancellationToken ct);
    public Task<IEnumerable<BalanceTransactionDTO>> GetTransactions(Guid userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct);
}