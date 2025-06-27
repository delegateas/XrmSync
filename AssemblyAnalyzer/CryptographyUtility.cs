using System.Security.Cryptography;

namespace DG.XrmPluginSync.AssemblyAnalyzer;

internal static class CryptographyUtility
{
    public static string Sha1Checksum(byte[] bytes)
    {
        return BitConverter.ToString(SHA1.Create().ComputeHash(bytes))
            .Replace("-", string.Empty);
    }
}
