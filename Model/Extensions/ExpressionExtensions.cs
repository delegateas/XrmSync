using System.Linq.Expressions;
using System.Reflection;
using XrmSync.Model.Exceptions;

namespace XrmSync.Model.Extensions;

public static class ExpressionExtensions
{
	public static MemberInfo GetMemberInfo<T>(this Expression<Func<T, object?>> lambda)
	{
		var body = lambda.Body as MemberExpression;
		if (body == null)
		{
			var ubody = lambda.Body as UnaryExpression ?? throw new XrmSyncException("Expression is not a member access");
			body = ubody.Operand as MemberExpression;
		}

		if (body == null)
			throw new XrmSyncException("Expression is not a member access");

		return body.Member;
	}

	public static MemberInfo GetMemberInfo<T, TValue>(this Expression<Func<T, TValue?>> lambda)
	{
		var body = lambda.Body as MemberExpression;
		if (body == null)
		{
			var ubody = lambda.Body as UnaryExpression ?? throw new XrmSyncException("Expression is not a member access");
			body = ubody.Operand as MemberExpression;
		}

		if (body == null)
			throw new XrmSyncException("Expression is not a member access");

		return body.Member;
	}
}
