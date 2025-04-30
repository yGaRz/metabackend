using NeoDaoBackend.Models.graphQL.Enums;
using System.Numerics;

namespace NeoDaoBackend.Models.graphQL;

public class TokenBalance {
    public TokenBalanceType? BalanceType { get; set; }
    public Chain? Chain { get; set; }
    public string? ChainId { get; set; }
    public Contract? Contract { get; set; }
    public Guid? Id { get; set; }
    public string? Owner { get; set; }
    public Token? Token { get; set; }
    public string? TokenAddress { get; set; }
    public BigInteger? TokenId { get; set; }
    public BigInteger? Value { get; set; }
}
