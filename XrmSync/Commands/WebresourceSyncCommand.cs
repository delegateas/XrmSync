using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Commands
{
    internal class WebresourceSyncCommand : XrmSyncSyncCommandBase
    {
        private readonly Option<string> _webresourceRoot;

        public WebresourceSyncCommand() : base("webresources", "Synchronize webresources from a local folder with Dataverse")
        {
            _webresourceRoot = new(CliOptions.Webresource.Primary, CliOptions.Webresource.Aliases)
            {
                Description = CliOptions.Webresource.Description,
                Arity = ArgumentArity.ExactlyOne
            };

            Add(_webresourceRoot);

            AddSharedOptions();
            AddSyncSharedOptions();

            SetAction(ExecuteAsync);
        }

        private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            var folderPath = parseResult.GetValue(_webresourceRoot);
            var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
            var sharedOptions = GetSharedOptionValues(parseResult);

            // Build service provider
            var serviceProvider = GetWebresourceSyncServices()
                .AddXrmSyncConfiguration(sharedOptions)
                .AddOptions(
                    options => options with
                    {
                        Logger = options.Logger with
                        {
                            LogLevel = logLevel ?? options.Logger.LogLevel,
                            CiMode = ciMode ?? options.Logger.CiMode
                        },
                        Execution = options.Execution with
                        {
                            DryRun = dryRun ?? options.Execution.DryRun
                        },
                        Webresource = options.Webresource with
                        {
                            Sync = new(
                                string.IsNullOrWhiteSpace(folderPath) ? options.Webresource.Sync.FolderPath : folderPath,
                                string.IsNullOrWhiteSpace(solutionName) ? options.Webresource.Sync.SolutionName : solutionName
                            )
                        }
                    }
                )
                .AddCommandOptions(config => config.Webresource.Sync)
                .AddLogger()
                .BuildServiceProvider();

            return await RunAction(serviceProvider, ConfigurationScope.WebresourceSync, CommandAction, cancellationToken)
                ? E_OK
                : E_ERROR;
        }

        private static IServiceCollection GetWebresourceSyncServices(IServiceCollection? services = null)
        {
            services ??= new ServiceCollection();

            services.AddWebresourceSyncAction();

            return services;
        }
    }
}
