namespace NeoDaoBackend.Models.graphQL;

public class User {
    public Guid Id { get; set; }
    public string? Address { get; set; } // unknown scalar type BlockchainAddress
    public DateTimeOffset CreatedAt { get; set; }
    public string? Name { get; set; }
    public string ProfilePic { get; set; }
}
