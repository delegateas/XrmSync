# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

XrmSync is a .NET 8 command-line tool for synchronizing Microsoft Dataverse plugins, custom APIs, and webresources between local code and Dataverse environments. It's distributed as a .NET global/local tool via NuGet.

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

# Webresource sync
dotnet run --project XrmSync -- webresources --folder "path/to/webresources" --solution-name "MySolution"

# Plugin analysis
dotnet run --project XrmSync -- analyze --assembly "path/to/plugin.dll" --pretty-print
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

**Configuration System**:
- Hierarchical configuration under `XrmSync` section in `appsettings.json`
- Named configurations support (e.g., "default", "dev", "prod")
- CLI options override configuration file values
- `--save-config` flag generates/updates configuration files from CLI arguments
- Root command can execute multiple sub-commands from a single configuration

**Command Architecture**:
- All commands implement `IXrmSyncCommand` and extend `XrmSyncCommandBase`
- Commands registered via `CommandLineBuilder` pattern
- Root command handler (`XrmSyncRootCommand`) can execute all configured sub-commands in sequence
- Dependency injection container built per command execution

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
2. Define options using System.CommandLine `Option<T>` instances
3. Implement execution logic by building DI container with required services
4. Register command in `Program.cs` via `CommandLineBuilder.AddCommand()`

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

## Codebase Conventions

- `InternalsVisibleTo` attributes expose internals to `Tests` and `DynamicProxyGenAssembly2` (NSubstitute)
- Logging uses `ILogger<T>` from Microsoft.Extensions.Logging
- CI mode (`--ci-mode`) prefixes warnings/errors for easier parsing in build pipelines
- Dry run mode (`--dry-run`) previews changes without modifying Dataverse
- Hash checking uses SHA1 for assembly content comparison
- Version comparisons determine if assembly updates are needed
