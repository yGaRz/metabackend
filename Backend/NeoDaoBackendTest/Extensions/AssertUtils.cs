using NeoDaoBackend.Validation;
using Newtonsoft.Json.Linq;

namespace NeoDaoBackendTest.Extensions;

public static class AssertUtils
{
    public static bool HasErrorCode(string message, ErrorCode code)
    {
        JToken jsonTocken = JToken.Parse(message);
        if (jsonTocken != null)
        {
            return jsonTocken.FirstOrDefault(x => x[code.ToString()] != null)!=null;
        }
        return false;
    }
}
