# Sample Plugin Projects

This directory contains four sample plugin projects that demonstrate different plugin development approaches and how XrmSync handles various scenarios for synchronizing plugins and custom APIs to Dataverse.

Each sample project is designed to work with XrmSync to showcase the tool's capabilities for analyzing, comparing, and synchronizing plugin assemblies with Dataverse environments.

## Project Overview

| Project | Base Class | Purpose |
|---------|------------|---------|
| **1-DAXIF** | Custom Plugin base class | Base implementation with standard plugin patterns |
| **2-Hybrid** | Hybrid (Custom + XrmPluginCore patterns) | Mixed approach with custom base class and fluent configuration |
| **3-XrmPluginCore** | XrmPluginCore | Demonstrates XrmPluginCore usage with changes |
| **4-Full-DAXIF** | Custom Plugin base class | Extended implementation with comprehensive features |

## 1-DAXIF (Base Implementation)

**Framework**: Custom plugin base class  
**Project Path**: `Samples/1-DAXIF/`

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

## 2-Hybrid (Custom Base with Fluent Configuration)

**Framework**: Hybrid (Custom base class + XrmPluginCore patterns)  
**Project Path**: `Samples/2-Hybrid/`

This project demonstrates a hybrid approach:

### Changes from 1-DAXIF:
- **Modified Update Step**: Added `Telephone1` to filtered attributes and images
- **New Create PreOperation Step**: Added with `AccountNumber` and `WebsiteUrl` filters
- **New Delete Step**: Added PreOperation step with PreImage
- **New Update PreOperation Step**: Added with `Description` filter and PreImage

### Architecture:
- Custom `Plugin` base class (similar to 1-DAXIF)
- XrmPluginCore-style fluent configuration patterns
- Manual registration with enhanced configuration options

### Key Features:
- Custom plugin base implementation
- Fluent API for step configuration
- Strongly-typed entity references
- Demonstrates analyzer compatibility with mixed approaches

## 3-XrmPluginCore (Pure XrmPluginCore)

**Framework**: XrmPluginCore (Delegate.XrmPluginCore NuGet package)  
**Project Path**: `Samples/3-XrmPluginCore/`

This project demonstrates pure XrmPluginCore usage:

### Changes from 1-DAXIF:
- **Modified Update Step**: Added `Telephone1` to filtered attributes and images
- **New Create PreOperation Step**: Added with `AccountNumber` and `WebsiteUrl` filters
- **New Delete Step**: Added PreOperation step with PreImage
- **New Update PreOperation Step**: Added with `Description` filter and PreImage

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

## 4-Full-DAXIF (Extended Implementation)

**Framework**: Custom plugin base class  
**Project Path**: `Samples/4-Full-DAXIF/`

This project provides the most comprehensive implementation:

### Features:
- All plugin steps from 2-Hybrid and 3-XrmPluginCore
- Extended custom API implementations
- Comprehensive plugin registration patterns
- Advanced configuration scenarios

### Plugins:
- **AccountPlugin**: Full feature set with all operation types
- **AccountDuplicatePlugin**: Duplicate detection and prevention

### Custom APIs:
- **CreateAccountApi**: Full account creation workflow
- **DeleteAccountApi**: Account deletion with validation
- **UpdateAccountApi**: Comprehensive account updates

### Key Features:
- Custom plugin base implementation with full feature set
- Demonstrates complex plugin scenarios
- Comprehensive custom API configurations
- Tests advanced synchronization scenarios

## Usage with XrmSync

These sample projects are designed to work with XrmSync for:

### Testing Synchronization Scenarios:# Sync base implementation
```bash
xrmsync --assembly "Samples/1-DAXIF/bin/Debug/net462/SamplePlugins.dll" --solution-name "TestSolution"
```

# Sync hybrid approach
```bash
xrmsync --assembly "Samples/2-Hybrid/bin/Debug/net462/ILMerged.SamplePlugins.dll" --solution-name "TestSolution"
```

# Sync XrmPluginCore implementation
```bash
xrmsync --assembly "Samples/3-XrmPluginCore/bin/Debug/net462/ILMerged.SamplePlugins.dll" --solution-name "TestSolution"
```

# Sync full implementation
```bash
xrmsync --assembly "Samples/4-Full-DAXIF/bin/Debug/net462/ILMerged.SamplePlugins.dll" --solution-name "TestSolution"
```

### Analyzer Testing:
The projects test different analyzer scenarios:
- **CoreAnalyzer**: Handles custom plugin base classes (1-DAXIF, 2-Hybrid, 4-Full-DAXIF)
- **DAXIFAnalyzer**: Handles XrmPluginCore patterns (3-XrmPluginCore)
- **Mixed Analysis**: 2-Hybrid tests hybrid scenarios

### Difference Detection:
Progressive synchronization testing:
- **1-DAXIF > 2-Hybrid**: Modified steps, new plugin registrations
- **1-DAXIF > 3-XrmPluginCore**: Framework change, new features
- **Any > 4-Full-DAXIF**: Comprehensive feature additions

XrmSync will detect:
- **Updates**: Modified filtered attributes and images on existing steps
- **Creates**: New plugin steps and custom APIs
- **Deletes**: Any removed registrations (when going backwards)

## Build and Deployment

All projects target **.NET Framework 4.6.2** and include:
- **ILMerge**: Combines dependencies into single assembly
- **Strong Naming**: Uses shared key for assembly signing
- **BusinessDomain**: Shared project reference for entity context

### Build Commands:# Build individual projects
```bash
dotnet build Samples/1-DAXIF/
dotnet build Samples/2-Hybrid/
dotnet build Samples/3-XrmPluginCore/
dotnet build Samples/4-Full-DAXIF/
```

# Build all samples
```bash
dotnet build Samples/
```

## Progressive Testing Scenarios

The samples are designed for progressive testing:

1. **Start with 1-DAXIF**: Base functionality
2. **Progress to 2-Hybrid**: Test custom base class with enhanced patterns
3. **Move to 3-XrmPluginCore**: Test framework migration
4. **Advance to 4-Full-DAXIF**: Test comprehensive scenarios

## Best Practices Demonstrated

1. **Plugin Registration**: Declarative vs. programmatic approaches
2. **Image Configuration**: Efficient attribute selection for performance
3. **Custom API Design**: Parameter and response property configuration
4. **Assembly Packaging**: ILMerge for dependency management
5. **Framework Migration**: Moving between plugin base classes
6. **Hybrid Architectures**: Mixing different plugin frameworks
7. **Progressive Enhancement**: Building from simple to complex implementations

## Testing Scenarios

These samples enable testing of:
- Plugin step creation, updates, and deletion
- Custom API synchronization
- Framework compatibility
- Assembly analysis across different patterns
- Difference detection and resolution
- Solution-aware deployment
- Progressive feature enhancement
- Framework migration scenarios

---

**Note**: These samples are for demonstration and testing purposes. In production scenarios, choose a consistent plugin framework approach rather than mixing patterns. The numbered sequence (1-4) represents increasing complexity and feature richness.