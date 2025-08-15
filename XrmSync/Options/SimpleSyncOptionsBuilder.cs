using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync.Options;

internal class SimpleSyncOptionsBuilder(IConfiguration configuration) : ISyncOptionsBuilder
{
    public XrmSyncOptions Build()
    {
        var configSection = configuration.GetSection("XrmSync");
        return new XrmSyncOptions(
            configSection.GetValue<string>(nameof(XrmSyncOptions.AssemblyPath)) ?? string.Empty,
            configSection.GetValue<string>(nameof(XrmSyncOptions.SolutionName)) ?? string.Empty,
            configSection.GetValue<LogLevel?>(nameof(XrmSyncOptions.LogLevel)) ?? LogLevel.Information,
            configSection.GetValue<bool>(nameof(XrmSyncOptions.DryRun))
        );
    }
}
