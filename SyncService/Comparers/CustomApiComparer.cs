using System.Linq.Expressions;
using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.Comparers;

public class CustomApiComparer(Description description) : BaseComparer<ApiDefinition>
{
    private bool DescriptionEquals(ApiDefinition x, ApiDefinition y)
    {
        return x.Description.StartsWith($"Synced with {description.ToolHeader}") ||
               y.Description.StartsWith($"Synced with {description.ToolHeader}") ||
               x.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase) ||
               y.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase) ||
               x.Description == y.Description;
    }

    public override IEnumerable<Expression<Func<ApiDefinition, object>>> GetDifferentPropertyNames(ApiDefinition x, ApiDefinition y)
    {
        // TODO: Verify that these are the fields that can be updated
        // TODO: If changing un-changable fields, we should delete and recreate instead
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.DisplayName != y.DisplayName) yield return x => x.DisplayName;
        if (!DescriptionEquals(x, y))
            yield return x => x.Description ;
        if (x.PluginTypeName != y.PluginTypeName) yield return x => x.PluginTypeName;
        if (x.IsCustomizable != y.IsCustomizable) yield return x => x.IsCustomizable;
        if (x.IsPrivate != y.IsPrivate) yield return x => x.IsPrivate;
        if (x.ExecutePrivilegeName != y.ExecutePrivilegeName) yield return x => x.ExecutePrivilegeName;
    }
}
