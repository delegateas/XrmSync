# XrmSync

[![Build Status](https://img.shields.io/github/actions/workflow/status/delegateas/XrmSync/ci.yml)](https://github.com/delegateas/XrmSync/actions)
[![NuGet](https://img.shields.io/nuget/v/XrmSync.svg)](https://www.nuget.org/packages/XrmSync)
[![GitHub release](https://img.shields.io/github/release/delegateas/XrmSync.svg)](https://github.com/delegateas/XrmSync/releases)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A dotnet tool for synchronizing Dataverse plugins, custom APIs, and webresources with your local code.

## Overview

XrmSync is a powerful tool that helps you manage and synchronize your Microsoft Dataverse plugins, custom APIs, and webresources. It analyzes your plugin assemblies and local webresource files, compares them with what's deployed in Dataverse, and performs the necessary create, update, or delete operations to keep them in sync.

## Features

- **Plugin Assembly Analysis**: Automatically analyzes .NET assemblies to discover plugin types, steps, and images
- **Intelligent Synchronization**: Compares local definitions with Dataverse and performs only necessary changes
- **Custom API Support**: Handles custom API definitions, request parameters, and response properties
- **Webresource Sync**: Synchronizes HTML, CSS, JavaScript, images, and other webresources from local folders
- **Dry Run Mode**: Preview changes without actually modifying your Dataverse environment
- **Solution-aware**: Deploys plugins and webresources to specific Dataverse solutions
- **Flexible Connection**: Supports connection string and URL-based Dataverse connections
- **Configuration Files**: Support for JSON configuration files to streamline repetitive operations
- **Named Configurations**: Manage multiple environments (dev, staging, prod) in a single config file
- **Comprehensive Logging**: Configurable logging levels for debugging and monitoring

## Installation

### As a .NET Tool (globally)
```bash
dotnet tool install --global XrmSync
```

### As a .NET Tool (locally)
```bash
dotnet new tool-manifest # if you don't have a manifest file yet
dotnet tool install --local XrmSync
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

3. Install as a global tool:
```bash
dotnet tool install --global --add-source ./XrmSync/nupkg XrmSync
```

## Usage

### Plugin Synchronization

```bash
xrmsync plugins --assembly "path/to/your/plugin.dll" --solution-name "YourSolutionName"
```

### Webresource Synchronization

```bash
xrmsync webresources --folder "path/to/webresources" --solution-name "YourSolutionName"
```

### Configuration File Usage

For repeated operations or complex configurations, you can read the configuration from the appsettings.json file:
```bash
# Run all configured commands (plugins, webresources, analysis)
xrmsync --profile default

# Run a specific command with configuration
xrmsync plugins --profile default
```

You can also override specific options when using a configuration file:
```bash
xrmsync plugins --dry-run --log-level Debug
```

### Command Line Options

#### Plugins Command

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--assembly` | `-a` | Path to the plugin assembly (*.dll) | Yes* |
| `--solution-name` | `-n` | Name of the target Dataverse solution | Yes* |
| `--dry-run` | | Perform a dry run without making changes | No |
| `--log-level` | `-l` | Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) | No |
| `--ci-mode` | `--ci` | Enable CI mode which prefixes all warnings and errors | No |
| `--profile` | `-p`, `--profile-name` | Name of the profile to load from appsettings.json | No |
| `--save-config` | `--sc` | Save current CLI options to appsettings.json | No |
| `--save-config-to` | | If `--save-config` is specified, override the filename to save to | No |

*Required when not present in appsettings.json

#### Webresources Command

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--folder` | `-w`, `--path` | Path to the root folder containing the webresources to sync | Yes* |
| `--solution-name` | `-n` | Name of the target Dataverse solution | Yes* |
| `--dry-run` | | Perform a dry run without making changes | No |
| `--log-level` | `-l` | Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) | No |
| `--ci-mode` | `--ci` | Enable CI mode which prefixes all warnings and errors | No |
| `--profile` | `-p`, `--profile-name` | Name of the profile to load from appsettings.json | No |
| `--save-config` | `--sc` | Save current CLI options to appsettings.json | No |
| `--save-config-to` | | If `--save-config` is specified, override the filename to save to | No |

*Required when not present in appsettings.json

**Supported Webresource Types:**
- HTML files (`.html`, `.htm`)
- CSS stylesheets (`.css`)
- JavaScript files (`.js`)
- XML data files (`.xml`)
- Image files (`.png`, `.jpg`, `.gif`, `.ico`, `.svg`)
- RESX string resources (`.resx`)
- XSL stylesheets (`.xsl`, `.xslt`)

The webresource name in Dataverse is determined by the file path relative to the specified folder root, prefixed with the solution name. For example:
- `wwwroot/js/script.js` → `[prefix]_[solution]/js/script.js`
- `wwwroot/css/styles.css` → `[prefix]_[solution]/css/styles.css`

#### Analyze Command

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--assembly` | `-a` | Path to the plugin assembly (*.dll) | Yes* |
| `--prefix` | `-p` | Publisher prefix for unique names | No (Default: "new") |
| `--pretty-print` | `--pp` | Pretty print the JSON output | No |
| `--save-config` | `--sc` | Save current CLI options to appsettings.json | No |
| `--save-config-to` | | If `--save-config` is specified, override the filename to save to | No |

*Required when not present in appsettings.json

#### Config Commands

**Config Validate Command**

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--profile` | `-p`, `--profile-name` | Name of the profile to validate | No (Default: "default") |

**Config List Command**

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| No options | | Lists all profiles from appsettings.json | N/A |

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

### Configuration Validation

You can validate your configuration files to ensure they are correctly set up:

```bash
# Validate the default configuration
xrmsync config validate

# Validate a specific named configuration
xrmsync config validate --profile dev
```

The `config validate` command shows:
- Which configuration file is being used (appsettings.json, appsettings.Development.json, etc.)
- Resolved configuration values for each section (Plugin Sync, Plugin Analysis, Webresource Sync, Logger, Execution)
- Validation status with specific errors if any
- Available commands based on the configuration

**Example output:**
```
Configuration: 'default' (from appsettings.json)

✓ Plugin Sync Configuration
  Assembly Path: C:\path\to\plugin.dll
  Solution Name: MySolution

✓ Webresource Sync Configuration
  Folder Path: C:\path\to\webresources
  Solution Name: MySolution

✓ Logger Configuration
  Log Level: Information
  CI Mode: false

✓ Execution Configuration
  Dry Run: false

Available Commands: plugins, webresources

Validation: PASSED
```

You can also list all available configurations:

```bash
# List all named configurations
xrmsync config list
```

**Example output:**
```
Available configurations (from appsettings.json):

  - default
    ✓ Configured: plugins, webresources

  - dev
    ✓ Configured: plugins

  - prod
    ✗ No valid configurations
```

### Configuration File Format

XrmSync supports JSON configuration files that contain all the necessary settings for synchronization and analysis. This is particularly useful for CI/CD pipelines or when you have consistent settings across multiple runs.

The configuration uses a hierarchical structure under the XrmSync section, separating sync and analysis options under Plugin.Sync and Plugin.Analysis respectively.

#### Generating Configuration Files

You can automatically generate configuration files using the `--save-config` option with any command:

##### Save sync options to appsettings.json (default)
```bash
xrmsync plugins --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --save-config
```

##### Save analysis options to appsettings.json
```bash
xrmsync analyze --assembly "MyPlugin.dll" --prefix "contoso" --pretty-print --save-config
```

##### Save to a custom file
```bash
xrmsync plugins --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --save-config --save-config-to "my-project.json"
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
    "default": {
      "Plugin": {
        "Sync": {
          "AssemblyPath": "path/to/your/plugin.dll",
          "SolutionName": "YourSolutionName"
        },
        "Analysis": {
          "AssemblyPath": "path/to/your/plugin.dll",
          "PublisherPrefix": "contoso",
          "PrettyPrint": true
        }
      },
      "Webresource": {
        "Sync": {
          "FolderPath": "path/to/webresources",
          "SolutionName": "YourSolutionName"
        }
      },
      "Logger": {
        "LogLevel": "Information",
        "CiMode": false
      },
      "Execution": {
        "DryRun": false
      }
    }
  }
}
```

#### Named Configurations

XrmSync supports multiple named configurations within a single appsettings.json file. This allows you to manage different environments (dev, staging, prod) or different projects in one configuration file.

**Using named configurations:**
```bash
# Use the 'default' configuration (or the only configuration if only one exists)
xrmsync --profile default

# Use a specific named configuration
xrmsync --profile dev

# If --config is not specified, 'default' is used, or the single config if only one exists
xrmsync
```

**Example with multiple named configurations:**
```json
{
  "XrmSync": {
    "default": {
      "Plugin": {
        "Sync": {
          "AssemblyPath": "bin/Debug/net462/MyPlugin.dll",
          "SolutionName": "DevSolution"
        }
      },
      "Execution": {
        "DryRun": true
      }
    },
    "prod": {
      "Plugin": {
        "Sync": {
          "AssemblyPath": "bin/Release/net462/MyPlugin.dll",
          "SolutionName": "ProdSolution"
        }
      },
      "Execution": {
        "DryRun": false
      }
    }
  }
}
```

#### Executing All Configured Sub-Commands

When you call the root command with a configuration name, XrmSync will automatically execute all configured sub-commands for that configuration:

```bash
# This will run plugin sync, plugin analysis, and webresource sync
# for all that are configured in the 'default' configuration
xrmsync --profile default
```

XrmSync will only execute sub-commands that have their required properties configured. For example:
- Plugin sync runs only if `AssemblyPath` and `SolutionName` are provided
- Plugin analysis runs only if `AssemblyPath` is provided
- Webresource sync runs only if `FolderPath` and `SolutionName` are provided

#### Plugin Sync Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `AssemblyPath` | string | Path to the plugin assembly (*.dll) | Required |
| `SolutionName` | string | Name of the target Dataverse solution | Required |

#### Plugin Analysis Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `AssemblyPath` | string | Path to the plugin assembly (*.dll) | Required |
| `PublisherPrefix` | string | Publisher prefix for unique names | "new" |
| `PrettyPrint` | boolean | Pretty print the JSON output | false |

#### Webresource Sync Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `FolderPath` | string | Path to the root folder containing webresources | Required |
| `SolutionName` | string | Name of the target Dataverse solution | Required |

#### Logger Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `LogLevel` | string | Log level (Trace, Debug, Information, Warning, Error, Critical) | "Information" |
| `CiMode` | boolean | Enable CI mode for easier parsing in CI systems | false |

#### Execution Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `DryRun` | boolean | Perform a dry run without making changes | false |

#### Example Configuration Files

**Basic sync configuration:**
```json
{
  "XrmSync": {
    "default": {
      "Plugin": {
        "Sync": {
          "AssemblyPath": "MyPlugin.dll",
          "SolutionName": "MyCustomSolution"
        }
      }
    }
  }
}
```

**Full configuration with plugins, webresources, and analysis:**

```json
{
  "XrmSync": {
    "default": {
      "Plugin": {
        "Sync": {
          "AssemblyPath": "bin/Release/net462/MyPlugin.dll",
          "SolutionName": "MyCustomSolution"
        },
        "Analysis": {
          "AssemblyPath": "bin/Release/net462/MyPlugin.dll",
          "PublisherPrefix": "contoso",
          "PrettyPrint": true
        }
      },
      "Webresource": {
        "Sync": {
          "FolderPath": "wwwroot",
          "SolutionName": "MyCustomSolution"
        }
      },
      "Logger": {
        "LogLevel": "Debug"
      },
      "Execution": {
        "DryRun": true
      }
    }
  }
}
```

**Webresource-only configuration:**
```json
{
  "XrmSync": {
    "default": {
      "Webresource": {
        "Sync": {
          "FolderPath": "src/webresources",
          "SolutionName": "MyCustomSolution"
        }
      },
      "Execution": {
        "DryRun": false
      }
    }
  }
}
```

**Analysis-only configuration:**
```json
{
  "XrmSync": {
    "default": {
      "Plugin": {
        "Analysis": {
          "AssemblyPath": "../../../bin/Debug/net462/ILMerged.SamplePlugins.dll",
          "PublisherPrefix": "contoso",
          "PrettyPrint": false
        }
      }
    }
  }
}
```

### Examples

#### Plugin synchronization:
```bash
xrmsync plugins --assembly "MyPlugin.dll" --solution-name "MyCustomSolution"
```

#### Webresource synchronization:
```bash
xrmsync webresources --folder "wwwroot" --solution-name "MyCustomSolution"
```

#### Dry run for webresources:
```bash
xrmsync webresources --folder "wwwroot" --solution-name "MyCustomSolution" --dry-run
```

#### Using a configuration file to run all configured commands:
```bash
# Runs all configured sub-commands (plugin sync, analysis, webresource sync)
# from the 'default' configuration
xrmsync --profile default

# Or simply (uses 'default' if it exists, or the only config if there's just one)
xrmsync
```

#### Using a specific named configuration:
```bash
xrmsync --profile prod
```

#### Running a specific sub-command with configuration:
```bash
# Uses the configuration but only runs the plugins command
xrmsync plugins --profile dev
```

#### Configuration file with CLI overrides:
```bash
xrmsync plugins --dry-run --log-level Debug
```

#### Dry run with debug logging:
```bash
xrmsync plugins --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --dry-run --log-level Debug
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

## Azure DevOps Pipeline support

It is possible to run XrmSync in an Azure DevOps pipeline.

Below is an example YAML snippet to get you started:
```yaml
parameters:
  - name: environment
    type: string
    default: Development

steps:
- task: NuGetToolInstaller@1
  displayName: 'NuGet tool installer'

- task: NuGetCommand@2
  displayName: 'NuGet restore'
  inputs:
    command: 'restore'
    restoreSolution: $(SolutionFile)

- task: DotNetCoreCLI@2
  displayName: 'Install dotnet tools'
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'restore'
    workingDirectory: '$(BasePath)/Tools'

- task: PowerPlatformToolInstaller@2
  displayName: 'Power Platform Tool Installer'

- task: PowerPlatformSetConnectionVariables@2
  displayName: 'Set Power Platform Connection Variables'
  name: connectionVariables
  inputs:
    authenticationType: 'PowerPlatformSPN'
    PowerPlatformSPN: ${{parameters.environment}}

- task: VSBuild@1
  displayName: 'Build solution'
  inputs:
    solution: $(SolutionFile)
    platform: '$(BuildPlatform)'
    configuration: '$(BuildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Validate Plugins'
  env:
    AZURE_CLIENT_ID: '$(connectionVariables.BuildTools.ApplicationId)'
    AZURE_CLIENT_SECRET: '$(connectionVariables.BuildTools.ClientSecret)'
    AZURE_TENANT_ID: '$(connectionVariables.BuildTools.TenantId)'
    DATAVERSE_URL: '$(BuildTools.EnvironmentUrl)'
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'run xrmsync plugins --dry-run --ci'
    workingDirectory: '$(BasePath)/Tools'
```

## Project Structure

The solution consists of several projects:

- **XrmSync**: Main command-line application
- **SyncService**: Core synchronization logic and services
- **Dataverse**: Dataverse connection and data access layer
- **AssemblyAnalyzer**: Assembly analysis and reflection utilities
- **Model**: Shared data models and entities

## How It Works

### Plugin Synchronization

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

### Webresource Synchronization

1. **Local File Discovery**: The tool scans your local folder to discover:
   - All files matching supported webresource types
   - File paths relative to the specified root folder
   - File content and MIME types

2. **Dataverse Comparison**: It connects to your Dataverse environment and compares:
   - Existing webresources in the solution
   - Content hashes to detect changes
   - Webresource names and types

3. **Synchronization**: Based on the comparison, it performs:
   - Creates new webresources
   - Updates modified webresources with new content
   - Removes webresources that no longer exist locally
   - Associates webresources with the specified solution

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
