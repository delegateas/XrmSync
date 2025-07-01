using DG.XrmPluginSync.Dataverse.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverse(this IServiceCollection services)
    {
        DataverseConnection.ServiceCollectionExtensions.AddDataverse(services);
        services.AddSingleton<IMessageReader, MessageReader>();
        services.AddSingleton<ISolutionReader, SolutionReader>();
        services.AddSingleton<IPluginReader, PluginReader>();
        services.AddSingleton<IPluginWriter, PluginWriter>();

        return services;
    }
}
