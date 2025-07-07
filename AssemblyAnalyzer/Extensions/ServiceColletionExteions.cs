using DG.XrmSync.SyncService.AssemblyReader;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmSync.AssemblyAnalyzer.Extensions;

public static class ServiceColletionExteions
{
    public static IServiceCollection AddAssemblyAnalyzer(this IServiceCollection services)
    {
        return services.AddSingleton<IAssemblyReader, AssemblyReader.AssemblyReader>();
    }
}
