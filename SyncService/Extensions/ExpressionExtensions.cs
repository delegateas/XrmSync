using System.Linq.Expressions;
using XrmSync.Model.Extensions;

namespace XrmSync.SyncService.Extensions;

internal static class ExpressionExtensions
{
    public static string GetMemberName<T>(this Expression<Func<T, object>> lambda)
    {
        return lambda.GetMemberInfo().Name;
    }
}
