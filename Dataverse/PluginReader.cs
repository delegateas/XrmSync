using XrmPluginCore.Enums;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse;

public class PluginReader(IDataverseReader reader) : IPluginReader
{
    public List<PluginDefinition> GetPluginTypes(Guid assemblyId)
    {
        return [.. from pt in reader.PluginTypes
                where pt.PluginAssemblyId != null && pt.PluginAssemblyId.Id == assemblyId
                select new PluginDefinition
                {
                    Id = pt.Id,
                    Name = pt.Name ?? string.Empty,
                    PluginSteps = new List<Step>()
                }];
    }

    public List<ParentReference<Step, PluginDefinition>> GetPluginSteps(IEnumerable<PluginDefinition> pluginTypes, Guid solutionId)
    {
        if (!pluginTypes.Any())
        {
            // If no plugin types are provided, return an empty list
            return [];
        }

        // Get all steps for the provided plugin types
        var steps = reader.RetrieveByColumn<SdkMessageProcessingStep>(
            s => s.PluginTypeId,
            [.. pluginTypes.Select(pt => pt.Id)],
            s => s.Name,
            s => s.Stage,
            s => s.Rank,
            s => s.Mode,
            s => s.SupportedDeployment,
            s => s.FilteringAttributes,
            s => s.ImpersonatingUserId,
            s => s.AsyncAutoDelete,
            s => s.SdkMessageId,
            s => s.SdkMessageFilterId,
            s => s.PluginTypeId
        ).ConvertAll(s => new {
            s.Id,
            s.Name,
            s.Stage,
            s.Rank,
            s.Mode,
            s.SupportedDeployment,
            s.FilteringAttributes,
            s.ImpersonatingUserId,
            s.AsyncAutoDelete,
            s.SdkMessageId,
            s.SdkMessageFilterId,
            s.PluginTypeId
        });

        // Filter by solution, get solution components for the steps
        var solutionComponents = reader.RetrieveByColumn<SolutionComponent, Guid?, Guid?>(
            sc => sc.ObjectId,
            [.. steps.Select(s => s.Id)],
            [(sc => sc.SolutionId, [solutionId])],
            sc => sc.ObjectId
        ).ConvertAll(sc => sc.ObjectId ?? Guid.Empty).ToHashSet();
        steps = [.. steps.Where(s => solutionComponents.Contains(s.Id))];

        var messages = reader.RetrieveByColumn<SdkMessage, Guid?>(
            m => m.SdkMessageId,
            [.. steps.Select(s => s.SdkMessageId?.Id ?? Guid.Empty).Distinct()],
            m => m.Name
        ).ToDictionary(m => m.Id, m => m.Name ?? string.Empty);

        var messageFilters = reader.RetrieveByColumn<SdkMessageFilter, Guid?>(
            mf => mf.SdkMessageFilterId,
            [.. steps.Select(s => s.SdkMessageFilterId?.Id ?? Guid.Empty).Distinct()],
            mf => mf.PrimaryObjectTypeCode
        ).ToDictionary(mf => mf.Id, mf => mf.PrimaryObjectTypeCode ?? string.Empty);

        var pluginImages = reader.RetrieveByColumn<SdkMessageProcessingStepImage>(
            pi => pi.SdkMessageProcessingStepId,
            [.. steps.Select(s => s.Id).Distinct()],
            pi => pi.SdkMessageProcessingStepImageId,
            pi => pi.Name,
            pi => pi.EntityAlias,
            pi => pi.Attributes1,
            pi => pi.ImageType,
            pi => pi.SdkMessageProcessingStepId
        ).ConvertAll(pi => (pi.SdkMessageProcessingStepId, Image: new Image
        {
            Id = pi.Id,
            Name = pi.Name ?? string.Empty,
            EntityAlias = pi.EntityAlias ?? string.Empty,
            Attributes = pi.Attributes1 ?? string.Empty,
            ImageType = (ImageType)(pi.ImageType ?? 0),
        })).ToLookup(pi => pi.SdkMessageProcessingStepId?.Id ?? Guid.Empty, pi => pi.Image);

        return steps.ConvertAll(s => new ParentReference<Step, PluginDefinition>(new Step
        {
            Id = s.Id,
            Name = s.Name ?? string.Empty,
            ExecutionStage = (ExecutionStage)(s.Stage ?? 0),
            EventOperation = messages.ContainsKey(s.SdkMessageId?.Id ?? Guid.Empty) ? messages[s.SdkMessageId?.Id ?? Guid.Empty] : string.Empty,
            LogicalName = messageFilters.ContainsKey(s.SdkMessageFilterId?.Id ?? Guid.Empty) ? messageFilters[s.SdkMessageFilterId?.Id ?? Guid.Empty] : string.Empty,
            Deployment = (Deployment)(s.SupportedDeployment ?? 0),
            ExecutionMode = (ExecutionMode)(s.Mode ?? 0),
            ExecutionOrder = s.Rank ?? 0,
            FilteredAttributes = s.FilteringAttributes ?? string.Empty,
            UserContext = s.ImpersonatingUserId?.Id ?? Guid.Empty,
            AsyncAutoDelete = s.AsyncAutoDelete ?? false,
            PluginImages = pluginImages.Contains(s.Id) ? [.. pluginImages[s.Id]] : []
        }, pluginTypes.FirstOrDefault(pt => pt.Id == s.PluginTypeId?.Id)
            ?? throw new XrmSyncException("Plugin type not found for step: " + (s.Name ?? string.Empty))));
    }

    public IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps)
    {
        Guid?[] userContextIds = [.. pluginSteps
            .Select(x => x.UserContext)
            .Where(x => x != Guid.Empty)
            .Distinct()];

        if (userContextIds.Length == 0)
        {
            return [];
        }

        HashSet<Guid?> existingUserContexts = [..
            reader.RetrieveByColumn<SystemUser, Guid?>(
            su => su.SystemUserId,
            userContextIds,
            su => su.SystemUserId
        ).ConvertAll(su => su.Id)];

        var missingUserContextIds = userContextIds.Except(existingUserContexts).ToHashSet();

        return pluginSteps.Where(x => missingUserContextIds.Contains(x.UserContext));
    }
}
