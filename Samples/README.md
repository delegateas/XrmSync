# Sample Plugin Projects

This directory contains three sample plugin projects that demonstrate different plugin development approaches and how XrmSync handles various scenarios for synchronizing plugins and custom APIs to Dataverse.

Each sample project is designed to work with XrmSync to showcase the tool's capabilities for analyzing, comparing, and synchronizing plugin assemblies with Dataverse environments.

## Project Overview

| Project | Base Class | Purpose |
|---------|------------|---------|
| **SamplePlugins** | Custom Plugin base class | Base implementation with standard plugin patterns |
| **SamplePlugins2** | XrmPluginCore | Demonstrates changes and XrmPluginCore usage |
| **SamplePlugins3** | Hybrid (Custom + XrmPluginCore) | Mixed approach with both frameworks |

## SamplePlugins (Base Implementation)

**Framework**: Custom plugin base class  
**Project Path**: `Samples/SamplePlugins/`

This is the baseline implementation that includes:

### Plugins:
- **AccountPlugin**: Basic plugin with two steps
  - Update operation (PostOperation) with filtered attributes (`Name`) and PreImage
  - Create operation (PostOperation) with no filters or images

### Custom APIs:
- **CustomAPI**: Base custom API implementation with comprehensive configuration support

### Key Features:
- Custom plugin base class with manual registration
- Manual step configuration
- Basic image handling
- Custom API configuration framework

## SamplePlugins2 (XrmPluginCore with Changes)

**Framework**: XrmPluginCore (Delegate.XrmPluginCore NuGet package)  
**Project Path**: `Samples/SamplePlugins2/`

This project demonstrates:

### Changes from SamplePlugins:
- **Modified Update Step**: Added `Telephone1` to filtered attributes and images
- **New Create PreOperation Step**: Added with `AccountNumber` and `WebsiteUrl` filters
- **New Delete Step**: Added PreOperation step with PreImage
- **New Update PreOperation Step**: Added with `Description` filter and PreImage
- **New Custom APIs**: Added `CreateAccountApi`, `DeleteAccountApi`, `UpdateAccountApi`

### Plugins:
- **AccountPlugin**: Extended with multiple new steps
- **AccountDuplicatePlugin**: Additional plugin for duplicate detection scenarios

### Custom APIs:
- **CreateAccountApi**: Account creation API with parameters and response properties
- **DeleteAccountApi**: Account deletion API
- **UpdateAccountApi**: Account update API with binding and custom processing steps

### Key Features:
- Uses XrmPluginCore for simplified plugin development
- Fluent API for step configuration
- Strongly-typed entity references
- ILMerge integration for deployment

## SamplePlugins3 (Hybrid Implementation)

**Framework**: Hybrid (Custom base class + XrmPluginCore patterns)  
**Project Path**: `Samples/SamplePlugins3/`

This project is functionally equivalent to SamplePlugins2 but uses a hybrid approach:

### Architecture:
- Custom `Plugin` base class (similar to SamplePlugins)
- XrmPluginCore patterns and conventions
- Manual registration with fluent configuration

### Key Features:
- Same plugin steps and Custom APIs as SamplePlugins2
- Custom plugin base implementation
- XrmPluginCore-style fluent configuration
- Demonstrates analyzer compatibility with mixed approaches

## Usage with XrmSync

These sample projects are designed to work with XrmSync for:

### Testing Synchronization Scenarios:
```bash
# Sync base implementation
xrmsync --assembly "Samples/SamplePlugins/bin/Debug/net462/SamplePlugins.dll" --solution-name "TestSolution"

# Sync with changes (demonstrates updates/additions)
xrmsync --assembly "Samples/SamplePlugins2/bin/Debug/net462/ILMerged.SamplePlugins.dll" --solution-name "TestSolution"

# Test hybrid approach
xrmsync --assembly "Samples/SamplePlugins3/bin/Debug/net462/ILMerged.SamplePlugins.dll" --solution-name "TestSolution"
```

### Analyzer Testing:
The projects test different analyzer scenarios:
- **CoreAnalyzer**: Handles custom plugin base classes (SamplePlugins, SamplePlugins3)
- **DAXIFAnalyzer**: Handles XrmPluginCore patterns (SamplePlugins2, SamplePlugins3)
- **Mixed Analysis**: SamplePlugins3 tests hybrid scenarios

### Difference Detection:
When syncing from SamplePlugins ? SamplePlugins2/3, XrmSync will detect:
- **Updates**: Modified filtered attributes and images on existing steps
- **Creates**: New plugin steps and custom APIs
- **Deletes**: Any removed registrations (when going backwards)

## Build and Deployment

All projects target **.NET Framework 4.6.2** and include:
- **ILMerge**: Combines dependencies into single assembly
- **Strong Naming**: Uses shared key for assembly signing
- **BusinessDomain**: Shared project reference for entity context

### Build Commands:
```bash
# Build individual projects
dotnet build Samples/SamplePlugins/
dotnet build Samples/SamplePlugins2/
dotnet build Samples/SamplePlugins3/

# Build all samples
dotnet build Samples/
```

## Best Practices Demonstrated

1. **Plugin Registration**: Declarative vs. programmatic approaches
2. **Image Configuration**: Efficient attribute selection for performance
3. **Custom API Design**: Parameter and response property configuration
4. **Assembly Packaging**: ILMerge for dependency management
5. **Framework Migration**: Moving between plugin base classes
6. **Hybrid Architectures**: Mixing different plugin frameworks

## Testing Scenarios

These samples enable testing of:
- Plugin step creation, updates, and deletion
- Custom API synchronization
- Framework compatibility
- Assembly analysis across different patterns
- Difference detection and resolution
- Solution-aware deployment

---

**Note**: These samples are for demonstration and testing purposes. In production scenarios, choose a consistent plugin framework approach rather than mixing patterns.