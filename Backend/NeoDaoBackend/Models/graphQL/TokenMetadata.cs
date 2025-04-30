using System.Numerics;

namespace NeoDaoBackend.Models.graphQL;

public class TokenMetadata {
    public string AnimationUrl { get; set; } // VideoUrl - Unknown scalar type?
    public List<TokenMetadataAttribute> Attributes { get; set; }
    public string? BackgroundColor { get; set; }
    public Chain? Chain { get; set; }
    public string? ChainId { get; set; }
    public Contract? Contract { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Description { get; set; }
    public string? ExternalUrl { get; set; }
    public string? FeeRecipient { get; set; }
    public Guid? Id { get; set; }
    public string Image { get; set; } // ImageUrl - Unknown scalar type?
    public string MetadataUrl { get; set; }
    public string? Name { get; set; }
    public int? SellerFeeBasisPoints { get; set; }
    public string? TokenAddress { get; set; }
    public BigInteger? TokenId { get; set; }
}
