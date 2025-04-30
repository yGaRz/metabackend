using GraphQL;

namespace NeoDaoBackend.Util;

public class CollectionUtils {
    private CollectionUtils() { }

    public static string FormatEnumerable<TValue>(IEnumerable<TValue> list) where TValue : notnull {
        return FormatEnumerable(list, "; ");
    }

    public static string FormatEnumerable<TValue>(IEnumerable<TValue> list, string separator) where TValue : notnull {
        return "[" + string.Join(separator, list) + "]";
    }

    public static string FormatGraphQLErrors(GraphQLError[] graphQLErrors) {
        List<string> messages = graphQLErrors.Select(error => error.Message).ToList();
        return FormatEnumerable(messages);
    }
}
