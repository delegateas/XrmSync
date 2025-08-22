using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public interface IDifferenceUtility
{
    Differences CalculateDifferences(AssemblyInfo localData, AssemblyInfo? remoteData);
}