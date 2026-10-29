using System.Text;

namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>JWT/Easy Auth base64url decoding (no padding in input).</summary>
internal static class Base64Url
{
    /// <summary>Decodes a base64url segment to bytes; throws on invalid input.</summary>
    public static byte[] Decode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');
        return Convert.FromBase64String(base64);
    }

    /// <summary>Decodes a base64url segment to a UTF-8 string.</summary>
    public static string DecodeToUtf8String(string value)
    {
        return Encoding.UTF8.GetString(Decode(value));
    }
}
