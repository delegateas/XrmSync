using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public interface IDifferenceCalculator
{
    Differences CalculateDifferences(AssemblyInfo localData, AssemblyInfo? remoteData);
}