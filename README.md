# XrmSync

[![Build Status](https://img.shields.io/github/actions/workflow/status/delegateas/XrmSync/ci.yml)](https://github.com/delegateas/XrmSync/actions)
[![NuGet](https://img.shields.io/nuget/v/XrmSync.svg)](https://www.nuget.org/packages/XrmSync)
[![GitHub release](https://img.shields.io/github/release/delegateas/XrmSync.svg)](https://github.com/delegateas/XrmSync/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A command-line tool for synchronizing Dataverse plugins and custom APIs with your local plugin assemblies.

## Overview

XrmSync is a powerful tool that helps you manage and synchronize your Microsoft Dataverse plugins and custom APIs. It analyzes your plugin assemblies, compares them with what's deployed in Dataverse, and performs the necessary create, update, or delete operations to keep them in sync.

## Features

- **Plugin Assembly Analysis**: Automatically analyzes .NET assemblies to discover plugin types, steps, and images
- **Intelligent Synchronization**: Compares local assembly definitions with Dataverse and performs only necessary changes
- **Custom API Support**: Handles custom API definitions, request parameters, and response properties
- **Dry Run Mode**: Preview changes without actually modifying your Dataverse environment
- **Solution-aware**: Deploys plugins to specific Dataverse solutions
- **Flexible Connection**: Supports connection string and URL-based Dataverse connections
- **Configuration Files**: Support for JSON configuration files to streamline repetitive operations
- **Comprehensive Logging**: Configurable logging levels for debugging and monitoring

## Installation

### As a .NET Tool
```bash
dotnet tool install --global XrmSync
```
### From Source

1. Clone the repository:
```bash
git clone https://github.com/delegateas/XrmSync.git
cd XrmSync
```
2. Build and pack the project:
```bash
dotnet build
dotnet pack XrmSync/XrmSync.csproj
```

3. Install as a local tool:
```bash
dotnet tool install --global --add-source ./XrmSync/nupkg XrmSync
```

## Usage


### Basic Sync Command

```bash
xrmsync --assembly "path/to/your/plugin.dll" --solution-name "YourSolutionName"
```

### Configuration File Usage

For repeated operations or complex configurations, you can read the configuration from the appsettings.json file:
```bash
xrmsync
```
You can also override specific options when using a configuration file:
```bash
xrmsync --dry-run --log-level Debug
```
### Command Line Options

#### Sync Command

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--assembly` | `-a` | Path to the plugin assembly (*.dll) | Yes* |
| `--solution-name` | `-n` | Name of the target Dataverse solution | Yes* |
| `--dry-run` | | Perform a dry run without making changes | No |
| `--log-level` | `-l` | Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) | No |
| `--save-config` | `--sc` | Save current CLI options to appsettings.json | No |
| `--save-config-to` | | If `--save-config` is specified, override the filename to save to | No |

*Required when not present in appsettings.json

#### Analyze Command

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--assembly` | `-a` | Path to the plugin assembly (*.dll) | Yes* |
| `--prefix` | `-p` | Publisher prefix for unique names | No (Default: "new") |
| `--pretty-print` | `--pp` | Pretty print the JSON output | No |
| `--save-config` | `--sc` | Save current CLI options to appsettings.json | No |
| `--save-config-to` | | If `--save-config` is specified, override the filename to save to | No |

*Required when not present in appsettings.json
### Assembly Analysis

You can analyze an assembly without connecting to Dataverse:
```bash
xrmsync analyze --assembly "path/to/your/plugin.dll" --pretty-print
```

You can also save analysis configurations:
```bash
xrmsync analyze --assembly "path/to/your/plugin.dll" --prefix "contoso" --pretty-print --save-config
```
This outputs JSON information about the plugin types, steps, and images found in the assembly.

### Configuration File Format

XrmSync supports JSON configuration files that contain all the necessary settings for synchronization and analysis. This is particularly useful for CI/CD pipelines or when you have consistent settings across multiple runs.

The configuration uses a hierarchical structure under the XrmSync section, separating sync and analysis options under Plugin.Sync and Plugin.Analysis respectively.

#### Generating Configuration Files

You can automatically generate configuration files using the `--save-config` option with any command:

# Save sync options to appsettings.json (default)
```bash
xrmsync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --save-config
```

# Save analysis options to appsettings.json
```bash
xrmsync analyze --assembly "MyPlugin.dll" --prefix "contoso" --pretty-print --save-config
```

# Save to a custom file
```bash
xrmsync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --save-config --save-config-to "my-project.json"
```

When using `--save-config`, XrmSync will:
1. Take all the provided CLI options
2. Create or update the target configuration file
3. Merge with existing content if the file already exists
4. Save the configuration in the proper JSON format

#### JSON Schema

```json
{
  "XrmSync": {
    "Plugin": {
      "Sync": {
        "AssemblyPath": "path/to/your/plugin.dll",
        "SolutionName": "YourSolutionName",
        "DryRun": false,
        "LogLevel": "Information"
      },
      "Analysis": {
        "AssemblyPath": "path/to/your/plugin.dll",
        "PublisherPrefix": "contoso",
        "PrettyPrint": true
      }
    }
  }
}
```

#### Sync Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `AssemblyPath` | string | Path to the plugin assembly (*.dll) | Required |
| `SolutionName` | string | Name of the target Dataverse solution | Required |
| `DryRun` | boolean | Perform a dry run without making changes | false |
| `LogLevel` | string | Log level (Trace, Debug, Information, Warning, Error, Critical) | "Information" |

#### Analysis Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `AssemblyPath` | string | Path to the plugin assembly (*.dll) | Required |
| `PublisherPrefix` | string | Publisher prefix for unique names | "new" |
| `PrettyPrint` | boolean | Pretty print the JSON output | false |

#### Example Configuration Files

**Basic sync configuration:**
```json
{
  "XrmSync": {
    "Plugin": {
      "Sync": {
        "AssemblyPath": "MyPlugin.dll",
        "SolutionName": "MyCustomSolution"
      }
    }
  }
}
```

**Full configuration with both sync and analysis:**

```json
{
  "XrmSync": {
    "Plugin": {
      "Sync": {
        "AssemblyPath": "bin/Release/net462/MyPlugin.dll",
        "SolutionName": "MyCustomSolution",
        "DryRun": true,
        "LogLevel": "Debug"
      },
      "Analysis": {
        "AssemblyPath": "bin/Release/net462/MyPlugin.dll",
        "PublisherPrefix": "contoso",
        "PrettyPrint": true
      }
    }
  }
}
```

**Analysis-only configuration:**
```json
{
  "XrmSync": {
    "Plugin": {
      "Analysis": {
        "AssemblyPath": "../../../bin/Debug/net462/ILMerged.SamplePlugins.dll",
        "PublisherPrefix": "contoso",
        "PrettyPrint": false
      }
    }
  }
}
```

### Examples

#### Basic synchronization:
```bash
xrmsync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution"
```

#### Using a configuration file:
```bash
xrmsync
```

#### Configuration file with CLI overrides:
```bash
xrmsync --dry-run --log-level Debug
```

#### Dry run with debug logging:
```bash
xrmsync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --dry-run --log-level Debug
```

## Configuration

### Dataverse Connection

XrmSync utilizes the Dataverse Connection NuGet package to manage connections to your Dataverse environment.
See the [Dataverse Connection documentation](https://github.com/delegateas/DataverseConnection) for more details on how to configure connections.

### Option Priority

When using configuration files with CLI options, the following priority order applies:

1. **CLI arguments** (highest priority) - Override everything
2. **Configuration file values** - Used when CLI arguments are not provided
3. **Default values** (lowest priority) - Used when neither CLI nor config file specify a value

### Logging

Configure logging levels using the `--log-level` option:

- `Trace`: Most detailed logging
- `Debug`: Detailed information for debugging
- `Information`: General information (default)
- `Warning`: Warning messages only
- `Error`: Error messages only
- `Critical`: Critical errors only

## Project Structure

The solution consists of several projects:

- **XrmSync**: Main command-line application
- **SyncService**: Core synchronization logic and services
- **Dataverse**: Dataverse connection and data access layer
- **AssemblyAnalyzer**: Assembly analysis and reflection utilities
- **Model**: Shared data models and entities

## How It Works

1. **Assembly Analysis**: The tool analyzes your plugin assembly to discover:
   - Plugin types and their attributes
   - Plugin steps (event handlers)
   - Plugin images (entity snapshots)
   - Custom API definitions

2. **Dataverse Comparison**: It connects to your Dataverse environment and compares:
   - Existing plugin assemblies and their metadata
   - Current plugin registrations
   - Custom API configurations

3. **Synchronization**: Based on the comparison, it performs:
   - Creates new plugin types and steps
   - Updates existing configurations
   - Removes obsolete registrations
   - Manages custom API definitions

## Development

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Access to a Dataverse environment for testing

### Building
```bash
dotnet build
```

### Testing
```bash
dotnet test
./scripts/Test-Samples.ps1 -SkipBuild
```

### Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/delegateas/XrmSync).

---

**Copyright (c) 2025 Context& A/S**
