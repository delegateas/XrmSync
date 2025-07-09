namespace XrmSync.SyncService.Difference;

public interface IDifferenceUtility
{
    Differences CalculateDifferences(CompiledData localData, CompiledData remoteData);
}