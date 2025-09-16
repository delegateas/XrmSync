using DataverseConnection;
using Microsoft.Extensions.DependencyInjection;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverseConnection(this IServiceCollection services)
    {
        services.AddDataverse();
        services.AddSingleton<IDataverseReader, DataverseReader>();
        services.AddSingleton<IDataverseWriter>((sp) =>
        {
            var options = sp.GetRequiredService<XrmSyncConfiguration>();

            return (options.Plugin?.Sync?.DryRun ?? throw new XrmSyncException("Cannot determine dry-run mode - check configuration"))
                ? ActivatorUtilities.CreateInstance<DryRunDataverseWriter>(sp)
                : ActivatorUtilities.CreateInstance<DataverseWriter>(sp);
        });

        services.AddSingleton<IMessageReader, MessageReader>();
        services.AddSingleton<ISolutionReader, SolutionReader>();

        services.AddSingleton<IPluginAssemblyReader, PluginAssemblyReader>();
        services.AddSingleton<IPluginAssemblyWriter, PluginAssemblyWriter>();

        services.AddSingleton<IPluginReader, PluginReader>();
        services.AddSingleton<IPluginWriter, PluginWriter>();

        services.AddSingleton<ICustomApiReader, CustomApiReader>();
        services.AddSingleton<ICustomApiWriter, CustomApiWriter>();

        return services;
    }
}
