namespace NeoDaoBackend.Util;

public class EnvUtils
{
    public static double GetDoubleEnvVariable(string envVariableName)
    {
        string? value = Environment.GetEnvironmentVariable(envVariableName);
        if (value == null || !double.TryParse(value, out double doubleValue))
        {
            throw new ApplicationException($"Environment variable {envVariableName} is not set or not a valid double!");
        }
        return doubleValue;
    }
}