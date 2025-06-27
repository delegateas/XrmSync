using System.Reflection;

namespace DG.XrmPluginSync.SyncService.Common;

internal static class InternalUtility
{
    public static string assemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public static string daxifVersion => $"DAXIF# v.{assemblyVersion}";
    public static TimeSpan defaultServiceTimeOut => new TimeSpan(0, 59, 0);
    public static string CreateTempFolder()
    {
        var newFolderName = Guid.NewGuid().ToString();
        var tmpFolder = Path.Combine(Path.GetTempPath(), newFolderName);
        EnsureDirectoryExists(tmpFolder);
        return tmpFolder;
    }

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
