using XrmSync.Model;

namespace XrmSync.Options;


[Flags]
internal enum ConfigurationScope
{
    None = 0,
    PluginSync = 1,
    PluginAnalysis = 2,
    WebresourceSync = 4,
    All = PluginSync | PluginAnalysis | WebresourceSync
}

internal interface IConfigurationValidator
{
    void Validate(ConfigurationScope scope);
}

internal interface IConfigurationBuilder
{
    XrmSyncConfiguration Build();
    ProfileConfiguration? GetProfile(string? profileName);
}