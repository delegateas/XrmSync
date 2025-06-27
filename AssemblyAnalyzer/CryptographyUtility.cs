using System;
using System.Security.Cryptography;
using System.Text;

namespace AssemblyAnalyzer
{
    internal static class CryptographyUtility
    {
        public static string Sha1Checksum(byte[] bytes)
        {
            return BitConverter.ToString(SHA1.Create().ComputeHash(bytes))
                .Replace("-", string.Empty);
        }
        public static string Sha1Checksum(string s)
        {
            return Sha1Checksum(Encoding.UTF8.GetBytes(s));
        }
    }
}
