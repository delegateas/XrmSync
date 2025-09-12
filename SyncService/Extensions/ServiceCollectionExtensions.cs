using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.PluginValidator;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Difference;
using XrmSync.SyncService.PluginValidator.Rules;

namespace XrmSync.SyncService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncService(this IServiceCollection services)
    {
        return services
            .AddSingleton<PluginSyncService>()
            .AddSingleton<Description>()
            .AddSingleton<IDifferenceCalculator, DifferenceCalculator>()
            .AddSingleton<IPrintService, PrintService>()
            .AddSingleton<IPluginValidator, PluginValidator.PluginValidator>()
            .AddValidationRules() // Auto-discover validation rules
            .AddSingleton<IEntityComparer<PluginDefinition>, PluginDefinitionComparer>()
            .AddSingleton<IEntityComparer<Step>, PluginStepComparer>()
            .AddSingleton<IEntityComparer<Image>, PluginImageComparer>()
            .AddSingleton<IEntityComparer<CustomApiDefinition>, CustomApiComparer>()
            .AddSingleton<IEntityComparer<RequestParameter>, RequestParameterComparer>()
            .AddSingleton<IEntityComparer<ResponseProperty>, ResponsePropertyComparer>();
    }

    public static IServiceCollection AddValidationRules(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Find all types that implement IValidationRule<T>
        var validationRuleTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidationRule<>)))
            .ToList();

        foreach (var ruleType in validationRuleTypes)
        {
            // Get the specific IValidationRule<T> interface this type implements
            var validationInterface = ruleType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidationRule<>));

            services.AddTransient(validationInterface, ruleType);
        }

        return services;
    }
}
