using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Dataverse;

internal sealed class DataverseReader(ServiceClient serviceClient) : IDataverseReader
{
    private readonly Lazy<DataverseContext> _lazyContext = new(() => new DataverseContext(serviceClient));
    private readonly Lazy<string> _lazyConnectedHost = new(serviceClient.ConnectedOrgUriActual.GetLeftPart(UriPartial.Authority));

    private DataverseContext DataverseContext => _lazyContext.Value;

    public string ConnectedHost => _lazyConnectedHost.Value;

    public IQueryable<SolutionComponent> SolutionComponents => DataverseContext.SolutionComponentSet;

    public IQueryable<PluginAssembly> PluginAssemblies => DataverseContext.PluginAssemblySet;

    public IQueryable<CustomAPI> CustomApis => DataverseContext.CustomAPISet;

    public IQueryable<CustomAPIRequestParameter> CustomApiRequestParameters => DataverseContext.CustomAPIRequestParameterSet;

    public IQueryable<CustomAPIResponseProperty> CustomApiResponseProperties => DataverseContext.CustomAPIResponsePropertySet;

    public IQueryable<PluginType> PluginTypes => DataverseContext.PluginTypeSet;

    public IQueryable<SdkMessage> SdkMessages => DataverseContext.SdkMessageSet;

    public IQueryable<SdkMessageFilter> SdkMessageFilters => DataverseContext.SdkMessageFilterSet;

    public IQueryable<Solution> Solutions => DataverseContext.SolutionSet;

    public IQueryable<Publisher> Publishers => DataverseContext.PublisherSet;

    public IQueryable<SystemUser> SystemUsers => DataverseContext.SystemUserSet;

    public IQueryable<WebResource> WebResources => DataverseContext.WebResourceSet;

    public IQueryable<Dependency> Dependencies => DataverseContext.DependencySet;

    public List<TEntity> RetrieveByColumn<TEntity, TValue>(
        Expression<Func<TEntity, TValue?>> inColumn,
        IEnumerable<TValue> values,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity
    {
        return RetrieveByColumn(inColumn, values, [], columns);
    }

    public List<TEntity> RetrieveByColumn<TEntity>(
        Expression<Func<TEntity, Guid?>> inColumn,
        IEnumerable<Guid> ids,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity
    {
        return RetrieveByColumn(inColumn, ids, [], columns);
    }

    public List<TEntity> RetrieveByColumn<TEntity>(
        Expression<Func<TEntity, EntityReference?>> inColumn,
        IEnumerable<Guid> ids,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity
    {
        return RetrieveByColumn(inColumn, ids, [], columns);
    }

    public List<TEntity> RetrieveByColumn<TEntity>(
        Expression<Func<TEntity, EntityReference?>> inColumn,
        IEnumerable<Guid> ids,
        IEnumerable<(Expression<Func<TEntity, object?>> column, IEnumerable<object> values)> additionalConditions,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity
    {
        return RetrieveByColumn<TEntity, EntityReference?, Guid>(inColumn, ids, additionalConditions, columns);
    }

    public List<TEntity> RetrieveByColumn<TEntity, TInValue, TValue>(
        Expression<Func<TEntity, TInValue?>> inColumn,
        IEnumerable<TValue> values,
        IEnumerable<(Expression<Func<TEntity, object?>> column, IEnumerable<object> values)> additionalConditions,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity
    {
        if (!values.Any())
        {
            return [];
        }

        var query = GetFilterByValuesQueryExpresion(inColumn, values, additionalConditions, columns);

        var result = serviceClient.RetrieveMultiple(query);
        return [.. result.Entities.Select(e => e.ToEntity<TEntity>())];
    }

    private static QueryExpression GetFilterByValuesQueryExpresion<TEntity, TInValue, TValue>(
        Expression<Func<TEntity, TInValue?>> inColumn,
        IEnumerable<TValue?> values,
        IEnumerable<(Expression<Func<TEntity, object?>> column, IEnumerable<object> values)> additionalConditions,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity
    {
        var instance = Activator.CreateInstance<TEntity>();
        var query = new QueryExpression(instance.LogicalName);
        query.ColumnSet.AddColumns([.. columns.Select(c => c.GetColumnName())]);
        query.Criteria.AddCondition(inColumn.GetColumnName(), ConditionOperator.In, [.. values]);
        query.AddConditions(additionalConditions);

        return query;
    }
}
