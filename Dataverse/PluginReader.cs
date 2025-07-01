using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.Dataverse;

public class PluginReader(ServiceClient serviceClient) : DataverseReader(serviceClient), IPluginReader
{

    public Entity GetPluginAssembly(Guid id)
    {
        return Retrieve(EntityTypeNames.PluginAssembly, id, new ColumnSet(true));
    }

    public Entity GetPluginAssembly(string name, string version)
    {
        LinkEntity link = new()
        {
            JoinOperator = JoinOperator.Inner,
            LinkFromAttributeName = "pluginassemblyid",
            LinkFromEntityName = EntityTypeNames.PluginAssembly,
            LinkToAttributeName = "objectid",
            LinkToEntityName = "solutioncomponent"
        };

        link.Columns.AddColumn("solutionid");

        FilterExpression filter = new();
        filter.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, name));
        filter.AddCondition(new ConditionExpression("version", ConditionOperator.Equal, version));

        QueryExpression query = new(EntityTypeNames.PluginAssembly)
        {
            ColumnSet = new ColumnSet(allColumns: true)
        };
        query.LinkEntities.Add(link);
        query.Criteria = filter;

        return RetrieveFirstOrDefault(query);
    }

    public Entity GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        LinkEntity link = new()
        {
            JoinOperator = JoinOperator.Inner,
            LinkFromAttributeName = "pluginassemblyid",
            LinkFromEntityName = EntityTypeNames.PluginAssembly,
            LinkToAttributeName = "objectid",
            LinkToEntityName = "solutioncomponent"
        };
        link.Columns.AddColumn("solutionid");
        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

        FilterExpression filter = new();
        filter.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, assemblyName));

        QueryExpression query = new(EntityTypeNames.PluginAssembly)
        {
            ColumnSet = new ColumnSet(allColumns: true)
        };
        query.LinkEntities.Add(link);
        query.Criteria = filter;
        return RetrieveFirstOrDefault(query);
    }

    public List<Entity> GetPluginTypes(Guid assemblyId)
    {
        FilterExpression filter = new();
        filter.AddCondition(new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assemblyId));
        QueryExpression query = new(EntityTypeNames.PluginType)
        {
            ColumnSet = new ColumnSet(allColumns: true),
            Criteria = filter
        };
        return RetrieveMultiple(query);
    }

    public List<Entity> GetPluginSteps(Guid solutionId)
    {
        LinkEntity link = new()
        {
            JoinOperator = JoinOperator.Inner,
            LinkFromAttributeName = "sdkmessageprocessingstepid",
            LinkFromEntityName = EntityTypeNames.PluginStep,
            LinkToAttributeName = "objectid",
            LinkToEntityName = "solutioncomponent"
        };
        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

        FilterExpression filter = new();
        QueryExpression query = new(EntityTypeNames.PluginStep)
        {
            ColumnSet = new ColumnSet(allColumns: true)
        };
        query.LinkEntities.Add(link);
        query.Criteria = filter;

        return RetrieveMultiple(query);
    }

    public List<Entity> GetPluginSteps(Guid solutionId, Guid pluginTypeId)
    {
        LinkEntity link = new()
        {
            JoinOperator = JoinOperator.Inner,
            LinkFromAttributeName = "sdkmessageprocessingstepid",
            LinkFromEntityName = EntityTypeNames.PluginStep,
            LinkToAttributeName = "objectid",
            LinkToEntityName = "solutioncomponent"
        };
        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

        FilterExpression filter = new();
        filter.AddCondition(new ConditionExpression("plugintypeid", ConditionOperator.Equal, pluginTypeId));
        QueryExpression query = new(EntityTypeNames.PluginStep)
        {
            ColumnSet = new ColumnSet(allColumns: true)
        };
        query.LinkEntities.Add(link);
        query.Criteria = filter;

        return RetrieveMultiple(query);
    }

    public List<Entity> GetPluginImages(Guid stepId)
    {
        FilterExpression filter = new();
        filter.AddCondition(new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.Equal, stepId));
        QueryExpression query = new(EntityTypeNames.PluginStepImage)
        {
            ColumnSet = new ColumnSet(allColumns: true),
            Criteria = filter
        };
        return RetrieveMultiple(query);
    }

    public IEnumerable<PluginStepEntity> GetMissingUserContexts(IEnumerable<PluginStepEntity> pluginSteps)
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

        var existingUserContexts = RetrieveMultiple(new QueryExpression("systemuser")
        {
            ColumnSet = new ColumnSet("systemuserid"),
            Criteria = new FilterExpression
            {
                Conditions = { new ConditionExpression("systemuserid", ConditionOperator.In, userContextIds) }
            }
        }).Select(x => x.GetAttributeValue<Guid>("systemuserid")).ToHashSet();

        var missingUserContextIds = userContextIds.Except(existingUserContexts).ToHashSet();

        return pluginSteps.Where(x => missingUserContextIds.Contains(x.UserContext));
    }
}
