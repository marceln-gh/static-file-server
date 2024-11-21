using System.Globalization;

namespace MarcelN.StaticFileServer.Web;

internal static class Environment
{
    public static TValue GetEnvironmentVariableOrDefault<TValue>(string variableName, TValue defaultValue) where TValue : IParsable<TValue>
    {
        var stringValue = System.Environment.GetEnvironmentVariable(variableName);
        return TValue.TryParse(stringValue, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    }
}
