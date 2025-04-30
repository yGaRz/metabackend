using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.Balance;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public class UserBalanceRepository : IUserBalanceRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserBalanceRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<UserBalance> GetUserBalance(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserBalances.Where(u => u.UserId == userId).FirstAsync(ct);
    }
    
    public async Task UpdateBalance(Guid userId, CoinType coinType, decimal amount, string reason, CancellationToken ct)
    {
        UserBalance userBalance = await GetUserBalance(userId, ct);

        switch (coinType)
        {
            case CoinType.SoftCoin:
                userBalance.SoftAmount += amount;
                break;

            case CoinType.HardCoin:
                userBalance.HardAmount += amount;
                break;

            case CoinType.BitForceCoin:
                userBalance.BitForceAmount += amount;
                break;

            default:
                throw new InvalidOperationException($"Unsupported CoinType: {coinType}");
        }
        
        await _dbContext.UserBalanceTransactions.AddAsync(new UserBalanceTransaction()
        {
            UserId = userId,
            CoinType = coinType,
            AmountDifference = amount,
            Reason = reason
        }, ct);
        _dbContext.UserBalances.Update(userBalance);
        await _dbContext.SaveChangesAsync(ct);
    }

    public void CreateBalanceNoSave(Guid userId, int softAmount, CancellationToken ct)
    {
        //TODO: Новые персонажи получают 250к
        var balance = new UserBalance { UserId = userId, SoftAmount = 250000, BitForceAmount = 250000, HardAmount = 250000 };
        _dbContext.UserBalances.Add(balance);
    }

    public async Task<bool> DoesUserHaveEnoughSoftCoins(Guid userId, decimal softAmount, CancellationToken ct)
    {
        var userBalance = await GetUserBalance(userId, ct);
        return userBalance.SoftAmount >= softAmount;
    }
    
    public async Task<bool> DoesUserHaveEnoughHardCoins(Guid userId, decimal hardAmount, CancellationToken ct)
    {
        var userBalance = await GetUserBalance(userId, ct);
        return userBalance.HardAmount >= hardAmount;
    }

    public async Task<IEnumerable<BalanceTransactionDTO>> GetTransactions(Guid userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        var query = _dbContext.UserBalanceTransactions.Where(x => x.UserId == userId);
        if (from != null)
        {
            query = query.Where(x => x.Created >= from);
        }
        if (to != null)
        {
            query = query.Where(x => x.Created <= to);
        }
        return await query.Select(x => new BalanceTransactionDTO()
        {
            SoftDifference = x.AmountDifference,
            Created = x.Created,
            Reason = x.Reason,
        }).ToListAsync();
    }
}