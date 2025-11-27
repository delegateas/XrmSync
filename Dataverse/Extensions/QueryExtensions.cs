using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq.Expressions;

namespace XrmSync.Dataverse.Extensions;

internal static class QueryExtensions
{
	public static void AddConditions<TEntity>(this QueryExpression query, IEnumerable<(Expression<Func<TEntity, object?>> column, IEnumerable<object> values)> additionalConditions) where TEntity : Entity
	{
		foreach (var (column, values) in additionalConditions.Where(c => c.values.Any()))
		{
			query.Criteria.AddCondition(column.GetColumnName(), ConditionOperator.In, [.. values]);
		}
	}
}
