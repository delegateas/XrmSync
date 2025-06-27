using DG.XrmPluginSync.SyncService.AssemblyReader;
using DG.XrmPluginSync.SyncService.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync.SyncService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncService(this IServiceCollection services)
    {
        return services.AddScoped<SyncService>()
            .AddScoped<IAssemblyReader, AssemblyReader.AssemblyReader>()
            .AddScoped<Message>()
            //.AddScoped<Plugin>()
            .AddScoped((_) => LoggerFactory.GetLogger<SyncService>());
    }
}
