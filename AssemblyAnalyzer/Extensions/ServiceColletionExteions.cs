using Microsoft.Extensions.DependencyInjection;
using XrmSync.AssemblyAnalyzer.AssemblyReader;

namespace XrmSync.AssemblyAnalyzer.Extensions;

public static class ServiceColletionExteions
{
    public static IServiceCollection AddAssemblyAnalyzer(this IServiceCollection services)
    {
        return services.AddSingleton<IAssemblyReader, AssemblyReader.AssemblyReader>();
    }
}
