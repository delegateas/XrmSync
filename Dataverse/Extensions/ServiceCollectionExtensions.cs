using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverse(this IServiceCollection services)
    {
        DataverseConnection.ServiceCollectionExtensions.AddDataverse(services);

        services.AddSingleton<IDataverseReader, DataverseReader>();

        services.AddSingleton<IDataverseWriter>((sp) =>
        {
            var options = sp.GetRequiredService<XrmPluginSyncOptions>();

            return options.DryRun
                ? ActivatorUtilities.CreateInstance<DryRunDataverseWriter>(sp)
                : ActivatorUtilities.CreateInstance<DataverseWriter>(sp);
        });

        services.AddSingleton<IMessageReader, MessageReader>();
        services.AddSingleton<ISolutionReader, SolutionReader>();
        services.AddSingleton<IPluginReader, PluginReader>();
        services.AddSingleton<IPluginWriter, PluginWriter>();

        return services;
    }
}
