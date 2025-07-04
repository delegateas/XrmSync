using DG.XrmPluginSync.Model.Plugin;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class PluginImageComparer : BaseComparer<Image>
{
    protected override bool EqualsInternal(Image x, Image y)
    {
        return
            x.Name == y.Name &&
            x.EntityAlias == y.EntityAlias &&
            x.ImageType == y.ImageType &&
            x.Attributes == y.Attributes;
    }
}
