using XrmSync.Model;

namespace XrmSync.Options;


[Flags]
internal enum ConfigurationScope
{
    None = 0,
    PluginSync = 1,
    PluginAnalysis = 2,
    All = PluginSync | PluginAnalysis
}

internal interface IConfigurationValidator
{
    void Validate(ConfigurationScope scope);
}

internal interface IConfigurationBuilder
{
    XrmSyncConfiguration Build();
}