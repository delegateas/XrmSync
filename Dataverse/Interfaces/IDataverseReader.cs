using Microsoft.Xrm.Sdk;
using System.Linq.Expressions;
using XrmSync.Dataverse.Context;

namespace XrmSync.Dataverse.Interfaces;

public interface IDataverseReader
{
    string ConnectedHost { get; }

    IQueryable<Solution> Solutions { get; }
    IQueryable<SolutionComponent> SolutionComponents { get; }
    IQueryable<Publisher> Publishers { get; }
    IQueryable<PluginAssembly> PluginAssemblies { get; }
    IQueryable<CustomApi> CustomApis { get; }
    IQueryable<CustomApiRequestParameter> CustomApiRequestParameters { get; }
    IQueryable<CustomApiResponseProperty> CustomApiResponseProperties { get; }
    IQueryable<PluginType> PluginTypes { get; }
    IQueryable<SdkMessage> SdkMessages { get; }
    IQueryable<SdkMessageFilter> SdkMessageFilters { get; }
    IQueryable<SystemUser> SystemUsers { get; }

    List<TEntity> RetrieveByColumn<TEntity, TValue>(
            Expression<Func<TEntity, TValue?>> inColumn,
            IEnumerable<TValue> values,
            params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity;

    List<TEntity> RetrieveByColumn<TEntity>(
        Expression<Func<TEntity, EntityReference?>> inColumn,
        IEnumerable<Guid> ids,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity;

    List<TEntity> RetrieveByColumn<TEntity, TInValue, TValue>(
        Expression<Func<TEntity, TInValue?>> inColumn,
        IEnumerable<TValue> values,
        IEnumerable<(Expression<Func<TEntity, object?>> column, IEnumerable<object> values)> additionalConditions,
        params Expression<Func<TEntity, object?>>[] columns) where TEntity : Entity;
}
