namespace NeoDaoBackend.Models.graphQL.Inputs;

public class RevokeTokenModel
{
    public string Token { get; set; }
}

public class RevokeTokenInput
{
    public RevokeTokenModel Model { get; set; }
}

public class RevokeTokenRequest
{
    public RevokeTokenInput Input { get; set; }
}