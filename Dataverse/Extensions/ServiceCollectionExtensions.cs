using DataverseConnection;
using Microsoft.Extensions.DependencyInjection;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;

namespace XrmSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverseConnection(this IServiceCollection services, XrmSyncOptions syncOptions)
    {
        services.AddDataverse(options =>
        {
            if (syncOptions.DataverseUrl is not null)
            {
                options.DataverseUrl = syncOptions.DataverseUrl;
            }
        });

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
