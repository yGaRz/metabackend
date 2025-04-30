using GraphQL.Client.Http;
using NeoDaoBackend.Models.Auth;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Util;

public class AuthUtils
{
    public static NeoDaoUser? GetUserFromContext(HttpContext context)
    {
        object? user = context.Items[UserKey];
        if (user == null || !(user is NeoDaoUser))
        {
            return null;
        }
        return (NeoDaoUser)user;
    }

    public static void AddGraphQLAuthorizationHeader(GraphQLHttpClient graphQLClient, NeoDaoUser user)
    {
        graphQLClient.HttpClient.DefaultRequestHeaders
                .TryAddWithoutValidation("Authorization", $"Bearer {user.Token!}");
    }
}
