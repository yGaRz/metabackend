using System.Numerics;

namespace NeoDaoBackend.Models.graphQL;

public class Token {
    public List<TokenBalance>? Balances { get; set; }
    public Chain? Chain { get; set; }
    public string ChainId { get; set; }
    public Contract? Contract { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public List<GameTokenMetadata> GameMetadata { get; set; }
    public int? Owners { get; set; }
    public string TokenAddress { get; set; }
    // public TokenBalancesConnection? tokenBalances { get; set; }
    public BigInteger? TokenId { get; set; }
    public TokenMetadata? TokenMetadata { get; set; }
    public BigInteger? Total { get; set; }
    // public TransfersConnection? transfers { get; set; }
}
