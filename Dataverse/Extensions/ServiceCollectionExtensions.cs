using DG.XrmSync.Dataverse.Interfaces;
using DG.XrmSync.Model;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverse(this IServiceCollection services)
    {
        DataverseConnection.ServiceCollectionExtensions.AddDataverse(services);

        services.AddSingleton<IDataverseReader, DataverseReader>();

        services.AddSingleton<IDataverseWriter>((sp) =>
        {
            var options = sp.GetRequiredService<XrmSyncOptions>();

            return options.DryRun
                ? ActivatorUtilities.CreateInstance<DryRunDataverseWriter>(sp)
                : ActivatorUtilities.CreateInstance<DataverseWriter>(sp);
        });

        services.AddSingleton<IMessageReader, MessageReader>();
        services.AddSingleton<ISolutionReader, SolutionReader>();

        services.AddSingleton<IPluginReader, PluginReader>();
        services.AddSingleton<IPluginWriter, PluginWriter>();

        services.AddSingleton<ICustomApiReader, CustomApiReader>();
        services.AddSingleton<ICustomApiWriter, CustomApiWriter>();

        return services;
    }
}
