using System.Numerics;

namespace NeoDaoBackend.Models.graphQL;

public class GameTokenMetadata {
    public string? ChainId { get; set; }
    public List<string> Tags { get; set; }
    public Token? Token { get; set; }
    // public BlockchainAddress tokenAddress { get; set; } // Unknown scalar type?
    public BigInteger? TokenId { get; set; }
}
