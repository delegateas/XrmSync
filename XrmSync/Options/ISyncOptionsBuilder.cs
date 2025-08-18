using XrmSync.Model;

namespace XrmSync.Options;

internal interface ISyncOptionsBuilder
{
    XrmSyncOptions Build();
}

internal interface IAnalysisOptionsBuilder
{
    PluginAnalysisOptions Build();
}