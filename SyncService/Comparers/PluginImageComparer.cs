using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Comparers;

public class PluginImageComparer : BaseComparer<Image>
{
    public override IEnumerable<Expression<Func<Image, object>>> GetDifferentPropertyNames(Image x, Image y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.EntityAlias != y.EntityAlias) yield return x => x.EntityAlias;
        if (x.ImageType != y.ImageType) yield return x => x.ImageType;
        if (x.Attributes != y.Attributes) yield return x => x.Attributes;
    }
}
