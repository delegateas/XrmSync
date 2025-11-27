using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Comparers;

internal class PluginImageComparer : BaseComparer<Image>
{
	public override IEnumerable<Expression<Func<Image, object?>>> GetDifferentPropertyNames(Image local, Image remote)
	{
		if (local.Name != remote.Name)
			yield return x => x.Name;
		if (local.Attributes != remote.Attributes)
			yield return x => x.Attributes;
	}

	public override IEnumerable<Expression<Func<Image, object?>>> GetRequiresRecreate(Image local, Image remote)
	{
		if (local.EntityAlias != remote.EntityAlias)
			yield return x => x.EntityAlias;
		if (local.ImageType != remote.ImageType)
			yield return x => x.ImageType;
	}
}
