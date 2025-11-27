using System.Linq.Expressions;
using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.Comparers;

internal class RequestParameterComparer : BaseComparer<RequestParameter>
{
	public override IEnumerable<Expression<Func<RequestParameter, object?>>> GetDifferentPropertyNames(RequestParameter local, RequestParameter remote)
	{
		if (local.Name != remote.Name)
			yield return x => x.Name;
		if (local.DisplayName != remote.DisplayName)
			yield return x => x.DisplayName;
	}

	public override IEnumerable<Expression<Func<RequestParameter, object?>>> GetRequiresRecreate(RequestParameter local, RequestParameter remote)
	{
		if (local.UniqueName != remote.UniqueName)
			yield return local => local.UniqueName;
		if (local.IsCustomizable != remote.IsCustomizable)
			yield return local => local.IsCustomizable;
		if (local.Type != remote.Type)
			yield return x => x.Type;
		if (local.IsOptional != remote.IsOptional)
			yield return x => x.IsOptional;
		if (local.LogicalEntityName != remote.LogicalEntityName)
			yield return x => x.LogicalEntityName;
	}
}
