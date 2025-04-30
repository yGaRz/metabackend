using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.graphQL.Responses;
using NeoDaoBackend.Models.graphQL;
using NeoDaoBackend.Models.graphQL.Enums;
using NeoDaoBackend.Repository;

namespace NeoDaoBackend.Service.GraphQL;

public class FakeBridgeService : IBridgeService
{
    private readonly IUserDataRepository _userDataRepository;
    public FakeBridgeService(IUserDataRepository userDataRepository)
    {
        _userDataRepository = userDataRepository;
    }

    public Task<TokenBalancesCollectionSegment?> GetUserNfts(Models.Common.PaginationModel pagination, NeoDaoUser user, CancellationToken ct)
    {
        return Task.FromResult(new TokenBalancesCollectionSegment
        {
            TotalCount = 0,
            PageInfo = new CollectionSegmentInfo
            {
                HasNextPage = false,
                HasPreviousPage = false,
            },
            Items = new List<TokenBalance>(),
        })!;
    }

    public Task<GetMePayload?> GetUserInfo(NeoDaoUser user, CancellationToken ct)
    {
        if (user.UserId == null)
        {
            return Task.FromResult(new GetMePayload
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                TactileLevel = 0,
                UniteVerseLevel = 0,
                ProfilePic = "whatever",
                Address = "Nowhere"
            })!;
        }
        else
        {
            var userFromBD = _userDataRepository.GetById(user.UserId.Value, CancellationToken.None).Result;
            return Task.FromResult(new GetMePayload
            {
                Id = user.UserId.Value,
                Name = userFromBD.UserName,
                TactileLevel = 0,
                UniteVerseLevel = 0,
                ProfilePic = "whatever",
                Address = "Nowhere"
            }) !;
        }
    }

    public Task<UserSession?> CreateUserSessionRequest(bool isMobile, CancellationToken ct)
    {
        return Task.FromResult(new UserSession
        {
            Id = Guid.NewGuid(),
            Status = UserSessionStatus.CREATED,
            ExpiredAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            SessionUrl = "whatever"
        })!;
    }

    public Task<(AuthTokenByUserSessionRequestPayload?, bool isNeedToUpdate)> GetAuthToken(Guid ExternalSessionId, CancellationToken ct)
    {
        AuthTokenByUserSessionRequestPayload payload = new AuthTokenByUserSessionRequestPayload
        {
            SessionRequestId = ExternalSessionId,
            Success = true,
            Token = "Super-secret-(not)-JWT-token",
        };
        (AuthTokenByUserSessionRequestPayload?, bool) result = (payload, true);
        return Task.FromResult(result);
    }

    public Task<bool> RevokeToken(string token, CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}
