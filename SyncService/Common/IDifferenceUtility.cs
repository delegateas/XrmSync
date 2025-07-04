using DG.XrmPluginSync.Model.Plugin;

namespace DG.XrmPluginSync.SyncService.Common;

public interface IDifferenceUtility
{
    Differences CalculateDifferences(CompiledData localData, CompiledData remoteData);
}