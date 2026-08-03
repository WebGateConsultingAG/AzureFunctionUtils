using System.Text;

namespace WebGate.Azure.FunctionsUtils.Internal;

internal static class Base64Url
{
    public static byte[] Decode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');
        return Convert.FromBase64String(base64);
    }

    public static string DecodeToUtf8String(string value)
    {
        return Encoding.UTF8.GetString(Decode(value));
    }
}
