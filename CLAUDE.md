# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

XrmSync is a .NET 10 command-line tool for synchronizing Microsoft Dataverse plugins, custom APIs, and webresources between local code and Dataverse environments. It's distributed as a .NET global/local tool via NuGet.

## Build & Development Commands

### Building
```bash
dotnet build
```

### Testing
```bash
# Run all tests
dotnet test

# Run sample analyzer tests (compares output from different plugin frameworks)
./scripts/Test-Samples.ps1 -SkipBuild

# Run tests with verbose output
./scripts/Test-Samples.ps1 -Verbose -OutputNormalizedJson
```

### Packaging
```bash
dotnet pack XrmSync/XrmSync.csproj
```

### Local Installation
```bash
dotnet tool install --global --add-source ./XrmSync/nupkg XrmSync
```

### Running the Tool Locally (Development)
```bash
# Plugin sync
dotnet run --project XrmSync -- plugins --assembly "path/to/plugin.dll" --solution-name "MySolution"

# Plugin sync, also ensuring a managed identity is bound to the assembly
dotnet run --project XrmSync -- plugins --assembly "path/to/plugin.dll" --solution-name "MySolution" --client-id "<app-id>" --tenant-id "<tenant-id>"

# Webresource sync
dotnet run --project XrmSync -- webresources --folder "path/to/webresources" --solution-name "MySolution"

# Webresource sync (only specific file types)
dotnet run --project XrmSync -- webresources --folder "path/to/webresources" --solution-name "MySolution" --file-extensions js css

# Plugin analysis
dotnet run --project XrmSync -- analyze --assembly "path/to/plugin.dll" --pretty-print

# Watch mode — sync once, then re-sync on every change (Ctrl+C to stop)
dotnet run --project XrmSync -- plugins --assembly "path/to/plugin.dll" --solution-name "MySolution" --watch
dotnet run --project XrmSync -- webresources --folder "path/to/webresources" --solution-name "MySolution" --watch

# Watch every watchable item in a profile (items with "Watch": true)
dotnet run --project XrmSync -- --profile dev
```

## Architecture

### Project Structure

The solution is organized into distinct layers with clear separation of concerns:

- **XrmSync**: CLI entry point using System.CommandLine. Contains command definitions and command-line parsing logic.
- **SyncService**: Core business logic for plugin and webresource synchronization. Orchestrates the sync workflow.
- **AssemblyAnalyzer**: Reflection-based assembly analysis supporting multiple plugin frameworks (DAXIF, XrmPluginCore, and custom patterns).
- **Dataverse**: Data access layer providing abstractions over the Dataverse SDK. Contains readers and writers for plugin assemblies, plugin types, steps, images, custom APIs, and webresources.
- **Model**: Shared domain models and DTOs used across all projects.

### Key Architectural Patterns

**Plugin Synchronization Flow**:
1. Read local assembly using `ILocalReader` and analyze it with `IAssemblyAnalyzer` to extract plugin/custom API metadata
2. Read remote Dataverse state via `IPluginAssemblyReader`, `IPluginReader`, and `ICustomApiReader`
3. Align IDs between local and remote entities by matching on unique names
4. Calculate differences using `IDifferenceCalculator` (creates, updates, deletes)
5. Execute operations in order: deletes → assembly upsert → updates → creates

**Webresource Synchronization Flow**:
1. Read local files from folder structure using `ILocalReader`
2. Read remote webresources from Dataverse solution via `IWebresourceReader`
3. Map IDs by matching webresource names (case-insensitive)
4. Calculate operations (create/update/delete) based on presence and content differences
5. Execute operations via `IWebresourceWriter`

**Managed Identity Handling**:
- A managed identity can be bound to a plugin assembly either as part of plugin sync or via the standalone `identity` command
- Plugin sync ensures the identity when `ManagedIdentityClientId`/`ManagedIdentityTenantId` are set on the `PluginSyncItem` (or `--client-id`/`--tenant-id` on the `plugins` command). The reconcile runs after the assembly upsert and regardless of whether the assembly binary changed, since the identity configuration can drift independently
- Reconcile semantics (shared `IManagedIdentityReconciler`):
  - **Ensure**: creates and links a new identity when none is bound, or **updates the existing record in place** (application id, tenant id, name) when it has drifted — it does not delete identities
  - **Remove** (standalone `identity --operation Remove`): deletes the linked identity; a missing assembly logs a warning instead of failing
- The standalone `identity` command remains available for explicit Ensure/Remove operations
- The identity is named `"{SolutionName} Managed Identity"` and is linked via the `PluginAssembly.ManagedIdentityId` lookup

**Watch Mode**:
- Lives entirely in the CLI layer (`XrmSync/Watch/`). `SyncService`, `Dataverse`, `AssemblyAnalyzer`, `ISyncService`, `IXrmSyncCommand.ExecuteFromProfile` and each command's private `RunCore` stay single-shot, so a watch-triggered run can never start a nested watch and always builds a fresh DI container (required because `LocalReader` caches `AssemblyInfo` per DLL path)
- Enabled per sync item via `"Watch": true`, or for the whole run via `--watch` (which overrides the per-item flags). Only Plugin and Webresource items are watchable; other types run once in the initial pass and log a warning if they request watching
- `WatchSettings.Resolve` is the single decision point: CLI flag → per-item flags → CI mode always wins (watch is suppressed with a warning so a pipeline cannot hang)
- `WatchTargetResolver` turns a sync item into a `WatchTarget`: the assembly's own directory (non-recursive, file name filter) for plugins, the folder recursively for webresources. Each target's `Accept` predicate mirrors exactly what the sync reads — the assembly file name (not `.pdb`/sibling assemblies), and the same supported-extension/`FileExtensions` check as `LocalReader.ReadWebResourceFolder`
- `WatchLoop` coalesces file system events: each accepted event sets a per-target dirty flag and pushes a token onto a bounded `Channel<int>` (`DropWrite`); a single consumer waits out `WatchDebounceMs`, drains the tokens, snapshots the flags, and runs the due syncs one at a time. Runs are therefore strictly sequential, an event storm is O(1), and a change arriving during a sync queues exactly one follow-up run
- Before re-running a plugin sync, `IWatchFileSystem.WaitUntilReadableAsync` waits (10 s) for the build to release the DLL; on timeout it warns and syncs anyway
- Watcher errors (internal buffer overflow, the folder disappearing) warn, mark the target dirty and re-subscribe with 1/2/4 s backoff; a target that cannot be restored is dropped, and if all are dropped the loop logs critical and returns
- Webresource syncs that belong to a watch session publish their created and updated webresources (`IWebresourceWriter.Publish` → `IDataverseWriter.PublishXml` → a single `PublishXmlRequest`); this is the only place XrmSync publishes. The signal is `WebresourceSyncCommandOptions.PublishAfterSync`, set from `watchSettings.Enabled` on the sub-command path and from `ExecutionContext.WatchSession` (per item: `watchSettings.Enabled && syncItem.Watch`) on the root profile path. It covers the whole session including the initial pass. Deletes are excluded (the records no longer exist) and an empty change set issues no request. `DryRunDataverseWriter.PublishXml` no-ops, and CI mode suppresses watch entirely, so neither a dry run nor a pipeline can publish
- A failed run is logged and watching continues. The watch session's exit code is the initial pass's exit code
- `IWatchFileSystem` and the injectable `delay` delegate on `WatchLoop` are the test seams — `Tests/Watch` drives the loop with no real files and no real time

**Configuration System**:
- Profile-based configuration under `XrmSync` section in `appsettings.json`
- Global settings (DryRun, LogLevel, CiMode, WatchDebounceMs) apply to all profiles
- Each profile contains a list of sync items (Plugin, PluginAnalysis, Webresource) and an optional shared solution name. The profile-level `SolutionName` is only required when a solution-targeting item (Plugin, Webresource, Identity) needs it and doesn't set its own — analysis-only profiles can omit it
- A profile can also declare a shared `AssemblyPath` reused by every assembly-based sync item (Plugin, PluginAnalysis, Identity)
- Sync items may override the profile-level `AssemblyPath` and/or `SolutionName` individually. Effective-value resolution precedence is: CLI override → sync-item value → profile-level value, centralized in `ProfileConfiguration.ResolveAssemblyPath`/`ResolveSolutionName`
- Sync items may opt into watch mode with `"Watch": true` (Plugin and Webresource only); the global `WatchDebounceMs` (default 500, valid 50–60000) tunes the quiet period
- Sync items may opt into create/update-only syncing with `"NoDelete": true` (Plugin and Webresource only), or `--no-delete` on the `plugins`/`webresources`/root commands. It suppresses **orphan** deletes only — remote records with no local counterpart. Deletes that pair with a create (a recreate, forced by an immutable property change) still run, as do the children of a recreated step or custom API, whose IDs were reset by `ResetChildIdsForRecreated*`. `DifferenceExtensions.WithoutOrphanDeletes` is the single decision point on the plugin side (applied per phase in `DifferenceCalculator.FinalizeDiff`); `WebresourceSyncService.ToDelete` handles the webresource side. For plugin types this supersedes `AllowEmptyTypes`
- Profile support (e.g., "default", "dev", "prod") via `--profile` flag
- CLI options override configuration file values for standalone execution
- Root command can execute all sync items in a profile sequentially

**Configuration Format**:
```json
{
  "XrmSync": {
    "DryRun": false,
    "LogLevel": "Information",
    "CiMode": false,
    "WatchDebounceMs": 500,
    "Profiles": [
      {
        "Name": "dev",
        "SolutionName": "MySolution",
        "AssemblyPath": "../path/to/plugin.dll",
        "Sync": [
          {
            "Type": "Plugin",
            "ManagedIdentityClientId": "00000000-0000-0000-0000-000000000000",
            "ManagedIdentityTenantId": "00000000-0000-0000-0000-000000000000",
            "Watch": true
          },
          {
            "Type": "Webresource",
            "FolderPath": "../path/to/webresources",
            "FileExtensions": ["js", "css"],
            "SolutionName": "MyWebresourceSolution",
            "Watch": true
          },
          {
            "Type": "PluginAnalysis",
            "PublisherPrefix": "new",
            "PrettyPrint": true
          }
        ]
      }
    ]
  }
}
```

**Command Architecture**:
- All commands implement `IXrmSyncCommand` and extend `XrmSyncCommandBase`
- Commands registered via `CommandLineBuilder` pattern
- Root command handler (`XrmSyncRootCommand`) can execute all configured sub-commands in sequence
- Dependency injection container built per command execution
- `CliOptionDescriptor` (in `Constants/`) is the single source of truth for each option's primary name, aliases, and description; it exposes `CreateOption<T>()` to eliminate duplication across command constructors
- Commands advertise their runtime-overridable options to the root command via `IXrmSyncCommand.GetProfileOverrides(assembly, solution)`, which returns a `ProfileOverrideProvider` containing the options to add to the root command and a callback to merge CLI values into a profile sync item before execution

**Multi-Framework Plugin Support**:
The analyzer supports three plugin attribute patterns through strategy pattern:
- **DAXIF framework**: Uses tuples of simple types to define plugin attributes
- **XrmPluginCore framework**: Core attributes from XrmPluginCore library
- **Hybrid**: Can analyze assemblies using multiple patterns simultaneously

Each framework has dedicated analyzers (`DAXIFPluginAnalyzer`, `CorePluginAnalyzer`, etc.) implementing `IAnalyzer<T>`.

### Validation Rules

Plugin validation is rules-based via `IValidationRule` implementations in `SyncService/PluginValidator/Rules`. Examples:
- `CreatePreImageRule`: Pre-images not allowed on Create events
- `DeletePostImageRule`: Post-images not allowed on Delete events
- `DuplicateRegistrationRule`: Prevents duplicate step registrations
- `BoundApiEntityRule`: Validates entity binding for bound custom APIs

All validation rules are executed before synchronization begins. Validation failures abort the sync operation.

### Dataverse Connection

Uses the `DataverseConnection` NuGet package for authentication. Supports:
- Environment variables: `DATAVERSE_URL`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID`
- Connection strings
- Interactive authentication

See [DataverseConnection docs](https://github.com/delegateas/DataverseConnection) for details.

## Development Guidelines

### Testing

- Unit tests use NSubstitute for mocking
- Tests follow AAA pattern (Arrange, Act, Assert)
- Sample projects in `Samples/` validate that different plugin frameworks produce equivalent analyzer output
- `Test-Samples.ps1` verifies analyzer consistency across frameworks

### Adding New Commands

1. Create command class implementing `IXrmSyncCommand` extending `XrmSyncCommandBase`
2. Define options using `CliOptions.<Group>.CreateOption<T>()` (add a new `CliOptionDescriptor` field to `CliOptions` if needed)
3. Implement execution logic by building DI container with required services
4. Register command in `Program.cs` via `CommandLineBuilder.AddCommand()`
5. Override `GetProfileOverrides(assembly, solution)` to advertise any options that should be settable on the root command when running via `--profile`, and provide a merge callback that applies those values into the relevant `SyncItem` subtype

### Adding Validation Rules

1. Create class implementing `IValidationRule<TEntity>` in `SyncService/PluginValidator/Rules`
2. Implement `Validate` method to check condition
3. Throw `ValidationException` with descriptive message on rule violation
4. Rule is automatically discovered and executed via dependency injection

### Extending Plugin Framework Support

1. Create new analyzer implementing `IAnalyzer<PluginDefinition>` or `IAnalyzer<CustomApiDefinition>`
2. Implement attribute recognition logic in `AnalyzeTypes` method
3. Register analyzer via dependency injection in `ServiceCollectionExtensions`
4. Add sample project demonstrating the framework for testing

### Webresource Naming Convention

Webresources are named using: `{publisherPrefix}_{solutionName}/{relativePath}`

Example: For file `wwwroot/js/script.js` with publisher prefix `abc` and solution `CustomSolution`:
- Webresource name: `abc_CustomSolution/js/script.js`

Supported file types: `.html`, `.htm`, `.css`, `.js`, `.xml`, `.png`, `.jpg`, `.gif`, `.ico`, `.svg`, `.resx`, `.xsl`, `.xslt`

The `--file-extensions` (`--ext` / `-e`) option filters which file types to sync, both locally and from Dataverse. When omitted, all supported types are synced.

## Codebase Conventions

- `InternalsVisibleTo` attributes expose internals to `Tests` and `DynamicProxyGenAssembly2` (NSubstitute)
- Logging uses `ILogger<T>` from Microsoft.Extensions.Logging
- CI mode (`--ci-mode`) prefixes warnings/errors for easier parsing in build pipelines
- Dry run mode (`--dry-run`) previews changes without modifying Dataverse
- Hash checking uses SHA1 for assembly content comparison
- Version comparisons determine if assembly updates are needed
