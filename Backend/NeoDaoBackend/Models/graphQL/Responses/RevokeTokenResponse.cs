namespace NeoDaoBackend.Models.graphQL.Responses;

public class RevokeTokenResponse
{
    public RevokeTokenData RevokeToken { get; set; }
}

public class RevokeTokenData
{
    public RevokeTokenResponseDto RevokeTokenResponseDto { get; set; }
}

public class RevokeTokenResponseDto
{
    public bool Success { get; set; }
}