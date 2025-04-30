using GraphQL;
using NeoDaoBackend.Models.graphQL.Responses;
using GraphQL.Client.Http;
using NeoDaoBackend.Models.graphQL;
using NeoDaoBackend.Models.Auth;

using static NeoDaoBackend.Util.AuthUtils;
using static NeoDaoBackend.Util.CollectionUtils;
using static NeoDaoBackend.Models.Constants;
using NeoDaoBackend.Models.graphQL.Inputs;
using Newtonsoft.Json;

namespace NeoDaoBackend.Service.GraphQL;

public class RealBridgeService : IBridgeService {
    private readonly GraphQLHttpClient _graphQLClient;
    private readonly ILogger<RealBridgeService> _logger;
    private readonly string _selfAddress;
    private readonly string _appDeepLink;

    public RealBridgeService(GraphQLHttpClient graphQLClient, ILogger<RealBridgeService> logger) {
        _graphQLClient = graphQLClient;
        _logger = logger;

        string selfAddressEnv = Environment.GetEnvironmentVariable("SELF_ADDRESS") ??
                throw new ApplicationException("Environment variable SELF_ADDRESS is not set!");
        _appDeepLink = Environment.GetEnvironmentVariable("APP_DEEPLINK") ??
                throw new ApplicationException("Environment variable APP_DEEPLINK is not set!");
        _selfAddress = $"{selfAddressEnv}/api/public";
    }

    public async Task<TokenBalancesCollectionSegment?> GetUserNfts(Models.Common.PaginationModel pagination, NeoDaoUser user, CancellationToken ct) {
        AddGraphQLAuthorizationHeader(_graphQLClient, user);
        var query = new GraphQLRequest {
            Query = GetUserNftsQuery,
            Variables = new {
                owner = user.UserId!.Value,
                take = pagination.Count,
                skip = pagination.Offset,
            }
        };
        var response = await _graphQLClient.SendQueryAsync<TokenBalancesResponse>(query, ct);
        if (response.Errors != null) {
            _logger.LogError($"Cannot process getUserNfts request: there were graphQL errors {FormatGraphQLErrors(response.Errors)}");
            return null;
        }
        return response.Data.TokenBalances;
    }

    public async Task<GetMePayload?> GetUserInfo(NeoDaoUser user, CancellationToken ct) {
        AddGraphQLAuthorizationHeader(_graphQLClient, user);
        var graphQLRequest = new GraphQLRequest {
            Query = MeQuery,
        };
        var meResponse = await _graphQLClient.SendQueryAsync<GetMeResponse>(graphQLRequest, ct);
        if (meResponse.Errors != null) {
            string errorMessage = $"Cannot process me request: there were graphQL errors {FormatGraphQLErrors(meResponse.Errors)}";
            _logger.LogError(errorMessage);
            return null;
        }
        return meResponse.Data.Me;
    }

    public async Task<UserSession?> CreateUserSessionRequest(bool isMobile, CancellationToken ct) {
        string webhookUrl = $"{_selfAddress}/confirmSession";
        AddUserSessionInput inputModel = new AddUserSessionInput {
            SuccessWebhookUrl = webhookUrl,
            FailureWebhookUrl = webhookUrl,
        };
        if (isMobile) {
            inputModel.SuccessRedirectUrl = _appDeepLink;
            inputModel.FailureRedirectUrl = _appDeepLink;
        }
        AddUserSessionRequestInput input = new AddUserSessionRequestInput { Model = inputModel };
        _logger.LogInformation($"Sending createSessionRequest graphQL query with input: {JsonConvert.SerializeObject(inputModel)}");
        var graphQLRequest = new GraphQLRequest {
            Query = AddUserSessionRequest,
            Variables = new { input },
        };
        var userSessionResponse = await _graphQLClient.SendQueryAsync<AddUserSessionRequestResponse>(graphQLRequest, ct);
        if (userSessionResponse.Errors != null) {
            _logger.LogError($"Cannot process createSession request: there were graphQL errors " +
                    $"{FormatGraphQLErrors(userSessionResponse.Errors)}");
            return null;
        }
        return userSessionResponse.Data.AddUserSessionRequest.UserSessionRequest;
    }

    public async Task<(AuthTokenByUserSessionRequestPayload?, bool isNeedToUpdate)> GetAuthToken(Guid externalSessionId, CancellationToken ct) {
        var graphQLRequest = new GraphQLRequest {
            Query = AuthTokenByUserSessionRequestQuery,
            Variables = new { requestId = externalSessionId }
        };
        var tokenResponse = await _graphQLClient.SendQueryAsync<AuthTokenByUserSessionRequestResponse>(graphQLRequest, ct);
        List<string> errors = tokenResponse.Errors?.Select(error => error.Message).ToList() ?? [];
        List<string> fatalErrors = errors.Where(error => !KnownAuthTokenByUserSessionRequestErrors.Contains(error)).ToList();
        if (fatalErrors.Count > 0) {
            _logger.LogError($"Cannot process authTokenByUserSessionRequest request: there were graphQL errors {FormatEnumerable(errors)}");
            return (null, false);
        }
        AuthTokenByUserSessionRequestPayload payload = tokenResponse.Data.AuthTokenByUserSessionRequest;
        if (!payload.Success) {
            _logger.LogWarning("Getting token attempt was unsuccessful");
        }
        List<string> knownErrors = errors.Where(KnownAuthTokenByUserSessionRequestErrors.Contains).ToList();
        bool isNeedToUpdate = payload.Success || knownErrors.Contains(ExpiredSessionError) || knownErrors.Contains(UnknownSessionError);
        return (payload, isNeedToUpdate);
    }

    public async Task<bool> RevokeToken(string token, CancellationToken ct)
    {
        var revokeTokenRequest = new RevokeTokenRequest
        {
            Input = new RevokeTokenInput
            {
                Model = new RevokeTokenModel
                {
                    Token = token
                }
            }
        };
        
        var graphQLRequest = new GraphQLRequest
        {
            Query = RevokeTokenMutation,
            Variables = revokeTokenRequest,
        };

        var response = await _graphQLClient.SendMutationAsync<RevokeTokenResponse>(graphQLRequest, ct);
    
        if (response.Errors != null && response.Errors.Any())
        {
            _logger.LogError($"Cannot process RemoveTokenAsync request: there were GraphQL errors {FormatGraphQLErrors(response.Errors)}");
            return false;
        }

        return response.Data?.RevokeToken?.RevokeTokenResponseDto?.Success ?? false;
    }
}
