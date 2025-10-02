using XrmPluginCore.Enums;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse;

public class PluginReader(IDataverseReader reader, ServiceClient serviceClient) : IPluginReader
{
    public List<PluginDefinition> GetPluginTypes(Guid assemblyId)
    {
        using var xrm = new DataverseContext(serviceClient);

        return [.. (from pt in xrm.PluginTypeSet
                where pt.PluginAssemblyId != null && pt.PluginAssemblyId.Id == assemblyId
                select new PluginDefinition
                {
                    Id = pt.Id,
                    Name = pt.Name ?? string.Empty,
                    PluginSteps = new List<Step>()
                })];
    }

    public List<ParentReference<Step, PluginDefinition>> GetPluginSteps(IEnumerable<PluginDefinition> pluginTypes, Guid solutionId)
    {
        if (!pluginTypes.Any())
        {
            // If no plugin types are provided, return an empty list
            return [];
        }

        // Create QueryExpression for SdkMessageProcessingStep
        var query = new QueryExpression(SdkMessageProcessingStep.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                SdkMessageProcessingStep.Fields.SdkMessageProcessingStepId,
                SdkMessageProcessingStep.Fields.Name,
                SdkMessageProcessingStep.Fields.Stage,
                SdkMessageProcessingStep.Fields.Rank,
                SdkMessageProcessingStep.Fields.Mode,
                SdkMessageProcessingStep.Fields.SupportedDeployment,
                SdkMessageProcessingStep.Fields.FilteringAttributes,
                SdkMessageProcessingStep.Fields.ImpersonatingUserId,
                SdkMessageProcessingStep.Fields.AsyncAutoDelete,
                SdkMessageProcessingStep.Fields.SdkMessageFilterId,
                SdkMessageProcessingStep.Fields.PluginTypeId),
            Criteria = new FilterExpression(LogicalOperator.And)
        };

        // Add condition for pluginTypeIds
        query.Criteria.AddCondition(SdkMessageProcessingStep.Fields.PluginTypeId, ConditionOperator.In, [.. pluginTypes.Select(p => p.Id)]);

        // Join with SolutionComponent
        var solutionComponentLink = query.AddLink(SolutionComponent.EntityLogicalName, SdkMessageProcessingStep.Fields.SdkMessageProcessingStepId, SolutionComponent.Fields.ObjectId);
        solutionComponentLink.LinkCriteria.AddCondition(SolutionComponent.Fields.SolutionId, ConditionOperator.Equal, solutionId);

        // Join with SdkMessage
        var sdkMessageLink = query.AddLink(SdkMessage.EntityLogicalName, SdkMessageProcessingStep.Fields.SdkMessageId, SdkMessage.Fields.SdkMessageId);
        sdkMessageLink.Columns = new ColumnSet(SdkMessage.Fields.Name);
        sdkMessageLink.EntityAlias = "ms";

        // Outer join with SdkMessageFilter to get LogicalName
        var sdkMessageFilterLink = query.AddLink(SdkMessageFilter.EntityLogicalName, SdkMessageProcessingStep.Fields.SdkMessageFilterId, SdkMessageFilter.Fields.SdkMessageFilterId, JoinOperator.LeftOuter);
        sdkMessageFilterLink.Columns = new ColumnSet(SdkMessageFilter.Fields.PrimaryObjectTypeCode);
        sdkMessageFilterLink.EntityAlias = "mf";

        // Outer join with SdkMessageProcessingStepImage to get PluginImages
        var pluginImageLink = query.AddLink(SdkMessageProcessingStepImage.EntityLogicalName, SdkMessageProcessingStep.Fields.SdkMessageProcessingStepId, SdkMessageProcessingStepImage.Fields.SdkMessageProcessingStepId, JoinOperator.LeftOuter);
        pluginImageLink.Columns = new ColumnSet(
            SdkMessageProcessingStepImage.Fields.Id,
            SdkMessageProcessingStepImage.Fields.Name,
            SdkMessageProcessingStepImage.Fields.EntityAlias,
            SdkMessageProcessingStepImage.Fields.Attributes1,
            SdkMessageProcessingStepImage.Fields.ImageType
        );
        pluginImageLink.EntityAlias = "pi";

        var results = reader.RetrieveMultiple(query);
        var groupedSteps = results.GroupBy(entity => entity.Id);

        return groupedSteps.Select(group =>
        {
            var entity = group.FirstOrDefault()
                ?? throw new XrmSyncException("No steps found but ID returned: " + group.Key);

            var pluginType = pluginTypes.FirstOrDefault(pt => pt.Id == entity.GetAttributeValue<EntityReference>(SdkMessageProcessingStep.Fields.PluginTypeId)?.Id)
                ?? throw new XrmSyncException("Plugin type not found for step: " + entity.GetAttributeValue<string>(SdkMessageProcessingStep.Fields.Name));

            var images = group
                .Where(e => e.GetAttributeValue<AliasedValue>($"pi.{SdkMessageProcessingStepImage.Fields.Id}") != null)
                .Select(e => new Image
                {
                    Id = Guid.Parse(e.GetAttributeValue<AliasedValue>($"pi.{SdkMessageProcessingStepImage.Fields.Id}")?.Value?.ToString() ?? string.Empty),
                    Name = e.GetAttributeValue<AliasedValue>($"pi.{SdkMessageProcessingStepImage.Fields.Name}")?.Value?.ToString() ?? string.Empty,
                    EntityAlias = e.GetAttributeValue<AliasedValue>($"pi.{SdkMessageProcessingStepImage.Fields.EntityAlias}")?.Value?.ToString() ?? string.Empty,
                    Attributes = e.GetAttributeValue<AliasedValue>($"pi.{SdkMessageProcessingStepImage.Fields.Attributes1}")?.Value?.ToString() ?? string.Empty,
                    ImageType = (ImageType)((e.GetAttributeValue<AliasedValue>($"pi.{SdkMessageProcessingStepImage.Fields.ImageType}")?.Value as OptionSetValue)?.Value ?? 0)
                });

            return new ParentReference<Step, PluginDefinition>(new Step
            {
                Id = entity.GetAttributeValue<Guid>(SdkMessageProcessingStep.Fields.Id),
                Name = entity.GetAttributeValue<string?>(SdkMessageProcessingStep.Fields.Name) ?? string.Empty,
                ExecutionStage = (ExecutionStage)(entity.GetAttributeValue<OptionSetValue>(SdkMessageProcessingStep.Fields.Stage)?.Value ?? 0),
                EventOperation = entity.GetAttributeValue<AliasedValue>($"ms.{SdkMessage.Fields.Name}")?.Value?.ToString() ?? string.Empty,
                LogicalName = entity.GetAttributeValue<AliasedValue>($"mf.{SdkMessageFilter.Fields.PrimaryObjectTypeCode}")?.Value?.ToString() ?? string.Empty,
                Deployment = (Deployment)(entity.GetAttributeValue<OptionSetValue>(SdkMessageProcessingStep.Fields.SupportedDeployment)?.Value ?? 0),
                ExecutionMode = (ExecutionMode)(entity.GetAttributeValue<OptionSetValue>(SdkMessageProcessingStep.Fields.Mode)?.Value ?? 0),
                ExecutionOrder = entity.GetAttributeValue<int>(SdkMessageProcessingStep.Fields.Rank),
                FilteredAttributes = entity.GetAttributeValue<string?>(SdkMessageProcessingStep.Fields.FilteringAttributes) ?? string.Empty,
                UserContext = entity.GetAttributeValue<EntityReference>(SdkMessageProcessingStep.Fields.ImpersonatingUserId)?.Id ?? Guid.Empty,
                AsyncAutoDelete = entity.GetAttributeValue<bool?>(SdkMessageProcessingStep.Fields.AsyncAutoDelete) ?? false,
                PluginImages = [.. images]
            }, pluginType);
        }).ToList();
    }

    public IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps)
    {
        var userContextIds = pluginSteps
            .Select(x => x.UserContext)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (userContextIds.Length == 0)
        {
            return [];
        }

        var existingUserContexts = reader.RetrieveMultiple(new QueryExpression(SystemUser.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(SystemUser.PrimaryIdAttribute),
            Criteria = new FilterExpression
            {
                Conditions = { new ConditionExpression(SystemUser.PrimaryIdAttribute, ConditionOperator.In, userContextIds) }
            }
        }).Select(x => x.GetAttributeValue<Guid>(SystemUser.PrimaryIdAttribute)).ToHashSet();

        var missingUserContextIds = userContextIds.Except(existingUserContexts).ToHashSet();

        return pluginSteps.Where(x => missingUserContextIds.Contains(x.UserContext));
    }
}
