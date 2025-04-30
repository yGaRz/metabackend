using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.graphQL;
using NeoDaoBackend.Models.graphQL.Responses;

namespace NeoDaoBackend.Service.GraphQL;

public interface IBridgeService {
    public Task<TokenBalancesCollectionSegment?> GetUserNfts(PaginationModel pagination, NeoDaoUser user, CancellationToken ct);

    public Task<GetMePayload?> GetUserInfo(NeoDaoUser user, CancellationToken ct);

    public Task<UserSession?> CreateUserSessionRequest(bool isMobile, CancellationToken ct);

    public Task<(AuthTokenByUserSessionRequestPayload?, bool isNeedToUpdate)> GetAuthToken(Guid ExternalSessionId, CancellationToken ct);

    public Task<bool> RevokeToken(string token, CancellationToken ct);
}
