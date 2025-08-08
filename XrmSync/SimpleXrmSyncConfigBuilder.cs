using Microsoft.Extensions.Configuration;
using XrmSync.Model;

namespace XrmSync;

internal static class SimpleXrmSyncConfigBuilder
{
    public static XrmSyncOptions BuildFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var configSection = configuration.GetSection("XrmSync");
        return new XrmSyncOptions(
            configSection.GetValue<string>(nameof(XrmSyncOptions.AssemblyPath)) ?? string.Empty,
            configSection.GetValue<string>(nameof(XrmSyncOptions.SolutionName)) ?? string.Empty,
            configSection.GetValue<string>(nameof(XrmSyncOptions.LogLevel)) ?? "Information",
            configSection.GetValue<bool>(nameof(XrmSyncOptions.DryRun)),
            configSection.GetValue<string>(nameof(XrmSyncOptions.DataverseUrl)) ?? string.Empty
        );
    }
}
