using System;
using System.IO;

namespace AssemblyAnalyzer
{
    internal static class FileUtility
    {
        public static string CopyFileToTempPath(string relativePath, string fileExtension)
        {
            var fullPath = Path.GetFullPath(relativePath);
            var tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + fileExtension);
            File.Copy(fullPath, tmpPath, true);
            return Path.GetFullPath(tmpPath);
        }

        public static string GetBase64StringFromFile(string path)
        {
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }
    }
}