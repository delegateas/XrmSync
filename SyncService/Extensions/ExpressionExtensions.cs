using System.Linq.Expressions;
using XrmSync.Model.Exceptions;

namespace XrmSync.SyncService.Extensions
{
    internal static class ExpressionExtensions
    {

        public static string GetMemberName<T>(this Expression<Func<T, object>> lambda)
        {
            var body = lambda.Body as MemberExpression;
            if (body == null)
            {
                var ubody = lambda.Body as UnaryExpression ?? throw new XrmSyncException("Expression is not a member access");
                body = ubody.Operand as MemberExpression;
            }

            if (body == null)
                throw new XrmSyncException("Expression is not a member access");

            return body.Member.Name;
        }
    }
}