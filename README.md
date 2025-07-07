# XrmSync

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/XrmSync.svg)](https://www.nuget.org/packages/XrmSync)
[![GitHub release](https://img.shields.io/github/release/delegateas/XrmSync.svg)](https://github.com/delegateas/XrmSync/releases)
[![Build Status](https://img.shields.io/github/actions/workflow/status/delegateas/XrmSync/build.yml)](https://github.com/delegateas/XrmSync/actions)

A .NET 8 command-line tool for synchronizing Dataverse plugins and custom APIs with your local plugin assemblies.

## Overview

XrmSync is a powerful tool that helps you manage and synchronize your Microsoft Dataverse plugins and custom APIs. It analyzes your plugin assemblies, compares them with what's deployed in Dataverse, and performs the necessary create, update, or delete operations to keep them in sync.

## Features

- **Plugin Assembly Analysis**: Automatically analyzes .NET assemblies to discover plugin types, steps, and images
- **Intelligent Synchronization**: Compares local assembly definitions with Dataverse and performs only necessary changes
- **Custom API Support**: Handles custom API definitions, request parameters, and response properties
- **Dry Run Mode**: Preview changes without actually modifying your Dataverse environment
- **Solution-aware**: Deploys plugins to specific Dataverse solutions
- **Flexible Connection**: Supports connection string and URL-based Dataverse connections
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
3. Build the project:
```bash
dotnet build
```
5. Install as a local tool:
```bash
dotnet pack XrmSync/XrmSync.csproj
dotnet tool install --global --add-source ./XrmSync/nupkg XrmSync
```
## Usage

### Basic Sync Command
```bash
XrmSync --assembly "path/to/your/plugin.dll" --solution-name "YourSolutionName"
```
### Command Line Options

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--assembly` | `-a` | Path to the plugin assembly (*.dll) | Yes |
| `--solution-name` | `-n` | Name of the target Dataverse solution | Yes |
| `--dry-run` | | Perform a dry run without making changes | No |
| `--log-level` | `-l` | Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) | No |
| `--dataverse` | | The Dataverse URL to connect to | No |

### Assembly Analysis

You can analyze an assembly without connecting to Dataverse:
```bash
XrmSync analyze --assembly "path/to/your/plugin.dll"
```
This outputs JSON information about the plugin types, steps, and images found in the assembly.

### Examples

#### Basic synchronization:

```bash
dotnet tool XrmSync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution"
```

#### Dry run with debug logging:

```bash
dotnet tool XrmSync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --dry-run --log-level Debug
```

#### Specify Dataverse environment:

```bash
dotnet tool XrmSync --assembly "MyPlugin.dll" --solution-name "MyCustomSolution" --dataverse "https://myorg.crm.dynamics.com"
```

## Configuration

### Dataverse Connection

XrmSync utilizes the Dataverse Connection NuGet package to manage connections to your Dataverse environment.
See the [Dataverse Connection documentation](https://github.com/delegateas/DataverseConnection) for more details on how to configure connections.

To override the connection URL, you can use the `--dataverse` option, which will override the `DATAVERSE_URL` environment variable.

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
dotnet build
### Testing
dotnet test
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

**Copyright (c) 2025 Delegate A/S**
