using System.Reflection;

namespace XrmSync.SyncService;

public class Description
{
    private readonly Lazy<string> _toolHeader;
    private readonly Lazy<string> _syncDescription;

    public Description()
    {
        _toolHeader = new Lazy<string>(() =>
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var toolName = assembly.GetName().Name;
            var version = assembly.GetName().Version?.ToString() ?? "unknown";
            return $"{toolName} v{version}";
        });

        _syncDescription = new Lazy<string>(() =>
            $"Synced with {ToolHeader} " +
            $"by '{Environment.UserName}' " +
            $"at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss \"GMT\"zzz}");
    }

    public string ToolHeader => _toolHeader.Value;
    public string SyncDescription => _syncDescription.Value;
}
