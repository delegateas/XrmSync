namespace DG.XrmPluginSync.SyncService.Differences;

public interface IDifferenceUtility
{
    Differences CalculateDifferences(CompiledData localData, CompiledData remoteData);
}