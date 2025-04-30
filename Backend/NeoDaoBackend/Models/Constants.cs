namespace NeoDaoBackend.Models;

public class Constants
{
    public readonly static string SessionIdHeader = "Session-Id";
    public readonly static string PlatformHeader = "Platform";
    public readonly static string ServerSecretHeader = "Backend-Secret";
    public readonly static string UserKey = "user";
    public readonly static string PlatformMobileKey = "Mobile";
    public readonly static int SessionExpirationJobWaitingTimeInMs = 60 * 1000;
    public readonly static int NotificationExpirationJobWaitingTimeInMs = 100;
    public readonly static int CalculatingJobWaitingTimeInMs = 60 * 1000;
    public readonly static int StreamingJobWaitingTimeInMs = 60 * 1000;
    public readonly static string WelcomeMessage = "Welcome to the Metacity, {0}!";
    public readonly static string MetacityFrom = "metacity";
    public readonly static string DefaultUserName = "Player";
    public readonly static string GlobalChatId = "global";
    public readonly static string AdminChatId = "admin";
    public readonly static string GreetingsChatId = "greetings";
    public readonly static string ExpiredSessionError = "session_request_has_expired";
    public readonly static string NotApprovedSessionError = "session_request_not_approved";
    public readonly static string UnknownSessionError = "session_request_not_found";
    public readonly static string PrivateChannelIdRegexString = "^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}" +
                                                                ":" +
                                                                "[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$";
    public readonly static string DateTimeOffsetFormatString = "yyyy-MM-ddTHH:mm:ss.fffK";
    public readonly static int MigrationCountRetry = 6;
    public readonly static int InitialSoftCoins = 0;
    public readonly static int MaxUsersInGroup = 10;
    public readonly static decimal BidsMinimumValue = 1.05m;
    public readonly static int CountNFTItemStore = 1;

    public readonly static List<string> KnownAuthTokenByUserSessionRequestErrors =
        [ExpiredSessionError, NotApprovedSessionError, UnknownSessionError];

    public readonly static string AddUserSessionRequest = @"
        mutation($input: AddUserSessionRequestInput!) {
          addUserSessionRequest(input: $input) {
            userSessionRequest {
              id
              createdAt
              expiredAt
              status
              sessionUrl
              code
            }
          }
        }";
    public readonly static string GetUserNftsQuery = @"
        query getUserNfts($skip: Int, $take: Int, $owner: String!) {
          tokenBalances(where: { owner: { eq: $owner } }, take: $take, skip: $skip) {
            items {
              value
              token {
                tokenId
                chainId
                tokenAddress
                total
                tokenMetadata {
                  name
                  description
                  image
                  animationUrl
                  backgroundColor
                  externalUrl
                  createdAt
                  attributes {
                  traitType
                    value
                  }
                  metadataUrl
                }
                gameMetadata {
                  tags
                }
                contract {
                  name
                  symbol
                  type
                }
              }
            }
            pageInfo {
              hasNextPage
              hasPreviousPage
            }
            totalCount
          }
        }";
    public readonly static string MeQuery = @"
        query {
          me {
            id
            address
            name
            profilePic
            tactileLevel
            uniteVerseLevel
          }
        }";
    public readonly static string AuthTokenByUserSessionRequestQuery = @"
        query($requestId: UUID!) {
          authTokenByUserSessionRequest(requestId: $requestId) {
            sessionRequestId
            success
            token
          }
        }";
    public static readonly string RevokeTokenMutation = @"
        mutation RevokeTokenAsync($input: RevokeTokenInput!) {
          revokeToken(input: $input) {
            revokeTokenResponseDto {
              success
            }
          }
        }";
}
