using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.Model.Plugin;
using DG.XrmPluginSync.SyncService.Comparers;
using DG.XrmPluginSync.SyncService.Extensions;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync.SyncService.Common;

public class Difference<T>
{
    public List<T> Creates { get; set; } = [];
    public List<T> Updates { get; set; } = [];
    public List<T> Deletes { get; set; } = [];
}

public record Differences(Difference<PluginType> Types,
    Difference<Step> PluginSteps, Difference<Image> PluginImages,
    Difference<ApiDefinition> CustomApis, Difference<RequestParameter> RequestParameters, Difference<ResponseProperty> ResponseProperties);

public class DifferenceUtility(ILogger log,
    IEqualityComparer<PluginType> pluginTypeComparer,
    IEqualityComparer<Step> pluginStepComparer,
    IEqualityComparer<Image> pluginImageComparer,
    IEqualityComparer<ApiDefinition> customApiComparer,
    IEqualityComparer<RequestParameter> requestComparer,
    IEqualityComparer<ResponseProperty> responseComparer) : IDifferenceUtility
{
    private static Difference<T> GetDifference<T>(List<T> list1, List<T> list2, IEqualityComparer<T> comparer, Func<T, string>? nameSelector = null) where T : EntityBase
    {
        nameSelector ??= (x) => x.Name;

        var creates = list1
            .Where(x => !list2.Any(y => nameSelector(x) == nameSelector(y)))
            .ToList();

        var deletes = list2
            .Where(x => !list1.Any(y => nameSelector(x) == nameSelector(y)))
            .ToList();

        var updates = list1
            .Where(x => list2.Any(y => nameSelector(x) == nameSelector(y) && !comparer.Equals(x, y)))
            .ToList();

        return new Difference<T>
        {
            Creates = creates,
            Deletes = deletes,
            Updates = updates
        };
    }

    public Differences CalculateDifferences(CompiledData localData, CompiledData remoteData)
    {
        var pluginTypeDifference = GetDifference(localData.Types, remoteData.Types, pluginTypeComparer);
        log.Print(pluginTypeDifference, "Types", x => x.Name);

        var pluginStepsDifference = GetDifference(localData.Steps, remoteData.Steps, pluginStepComparer);
        log.Print(pluginStepsDifference, "Plugin Steps", x => x.Name);

        var pluginImagesDifference = GetDifference(localData.Images, remoteData.Images, pluginImageComparer, x => $"[{x.Name}] {x.PluginStepName}");
        log.Print(pluginImagesDifference, "Plugin Images", x => $"[{x.Name}] {x.PluginStepName}");

        var customApiDifference = GetDifference(localData.CustomApis, remoteData.CustomApis, customApiComparer);
        log.Print(customApiDifference, "Custom APIs", x => x.Name);

        var requestDifference = GetDifference(localData.RequestParameters, remoteData.RequestParameters, requestComparer);
        log.Print(requestDifference, "Custom API Request Parameters", x => x.Name);

        var responseDifference = GetDifference(localData.ResponseProperties, remoteData.ResponseProperties, responseComparer);
        log.Print(responseDifference, "Custom API Response Properties", x => x.Name);

        return new(pluginTypeDifference,
            pluginStepsDifference, pluginImagesDifference,
            customApiDifference, requestDifference, responseDifference);
    }
}
