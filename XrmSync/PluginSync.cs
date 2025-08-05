using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XrmSync.AssemblyAnalyzer;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Dataverse.Extensions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService;
using XrmSync.SyncService.Extensions;
using DGLoggerFactory = XrmSync.LoggerFactory;

namespace XrmSync;

internal static class PluginSync
{
    public static async Task<bool> RunSync(XrmSyncOptions options)
    {
        var services = RegisterSyncServices(options);

        var log = services.GetRequiredService<ILogger>();
        var description = services.GetRequiredService<Description>();
        log.LogInformation("{header}", description.ToolHeader);

        if (options.DryRun)
        {
            log.LogInformation("***** DRY RUN *****");
            log.LogInformation("No changes will be made to Dataverse.");
        }

        if (options.DataverseUrl is not null)
        {
            log.LogInformation("Connecting to Dataverse at {dataverseUrl}", options.DataverseUrl);
        }

        try
        {
            var pluginSyncService = ActivatorUtilities.CreateInstance<PluginSyncService>(services);
            await pluginSyncService.Sync();
            return true;
        }
        catch (XrmSyncException ex)
        {
            var logger = services.GetRequiredService<ILogger>();
            logger.LogError("Error during synchronization: {message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger>();
            logger.LogCritical(ex, "An unexpected error occurred during synchronization");
            return false;
        }
    }

    public static bool RunAnalysis(string assemblyPath)
    {
        var services = RegisterAnalysisServices();

        try
        {
            var analyzer = ActivatorUtilities.CreateInstance<AssemblyAnalyzer.AssemblyAnalyzer>(services);
            var pluginDto = analyzer.AnalyzeAssembly(assemblyPath);
            var jsonOutput = JsonSerializer.Serialize(pluginDto);
            Console.WriteLine(jsonOutput);
            return true;
        }
        catch (AnalysisException ex)
        {
            Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
            return false;
        }
    }

    private static ServiceProvider RegisterSyncServices(XrmSyncOptions options)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);

        DGLoggerFactory.MinimumLevel = Enum.Parse<LogLevel>(options.LogLevel);
        services.AddSingleton((_) => DGLoggerFactory.GetLogger<ISyncService>());
        services.AddAssemblyReader();
        services.AddSyncService();
        services.AddDataverseConnection(options);

        return services.BuildServiceProvider();
    }

    private static ServiceProvider RegisterAnalysisServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton((_) => DGLoggerFactory.GetLogger<ISyncService>());
        serviceCollection.AddAssemblyAnalyzer();

        return serviceCollection.BuildServiceProvider();
    }
}
