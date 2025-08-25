using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
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
            .AddSingleton<IAssemblyAnalyzer, AssemblyAnalyzer>()
            .AddAnalyzers();
    }

    public static IServiceCollection AddAnalyzers(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Find all types that implement IAnalyzer<T>
        var analyzerTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IAnalyzer<>)))
            .ToList();

        foreach (var analyzerType in analyzerTypes)
        {
            // Get the specific IValidationRule<T> interface this type implements
            var analyzerInterface = analyzerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAnalyzer<>));

            services.AddSingleton(analyzerInterface, analyzerType);
        }

        return services;
    }
}
