using Microsoft.Extensions.DependencyInjection;
using XrmSync.AssemblyAnalyzer.Analyzers;
using XrmSync.AssemblyAnalyzer.AssemblyReader;

namespace XrmSync.AssemblyAnalyzer.Extensions;

public static class ServiceColletionExtensions
{
    public static IServiceCollection AddAssemblyReader(this IServiceCollection services)
    {
        return services.AddSingleton<IAssemblyReader, AssemblyReader.AssemblyReader>();
    }

    public static IServiceCollection AddAssemblyAnalyzer(this IServiceCollection services)
    {
        return services
            .AddSingleton<IPluginAnalyzer, DAXIFPluginAnalyzer>()
            .AddSingleton<ICustomApiAnalyzer, DAXIFCustomApiAnalyzer>();
    }
}
