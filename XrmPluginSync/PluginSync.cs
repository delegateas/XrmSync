using DG.XrmPluginSync.SyncService.Models.Requests;
using System.Reflection;

namespace DG.XrmPluginSync;

internal class PluginSync(SyncService.SyncService syncService)
{
    public void Run(string[] args)
    {
        if (args.Length == 0)
        {
            var versionString = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();

            Console.WriteLine($"xrmpluginsync v{versionString}");
            Console.WriteLine("-------------");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  xrmpluginsync <message>");
            return;
        }

        var assemblyLocation = Path.GetFullPath(args[0]);

        SyncRequest req = new()
        {
            AssemblyPath = assemblyLocation,
            ProjectPath = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine project path"),
            SolutionName = Path.GetFileNameWithoutExtension(assemblyLocation),
            DryRun = true
        };

        syncService.SyncPlugins(req).Wait();
    }
}
