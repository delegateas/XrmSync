# Contributing to XrmSync

Thank you for your interest in contributing to XrmSync! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Building and Testing](#building-and-testing)
- [Making Changes](#making-changes)
- [Submitting Contributions](#submitting-contributions)
- [Code Conventions](#code-conventions)
- [Project Architecture](#project-architecture)

## Getting Started

XrmSync is a .NET 8 command-line tool for synchronizing Microsoft Dataverse plugins, custom APIs, and webresources. Before contributing, familiarize yourself with:

- The [README.md](README.md) for basic usage
- The [CLAUDE.md](CLAUDE.md) for detailed architecture and development guidelines
- The project's [GitHub Issues](https://github.com/delegateas/XrmSync/issues) for open tasks

## Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A code editor (Visual Studio, VS Code, or Rider recommended)
- Git for version control

### Clone the Repository

```bash
git clone https://github.com/delegateas/XrmSync.git
cd XrmSync
```

### Build the Project

```bash
dotnet build
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run sample analyzer tests
./scripts/Test-Samples.ps1 -SkipBuild

# Run tests with verbose output
./scripts/Test-Samples.ps1 -Verbose -OutputNormalizedJson
```

### Run the Tool Locally

```bash
# Plugin sync example
dotnet run --project XrmSync -- plugins --assembly "path/to/plugin.dll" --solution-name "MySolution"

# Webresource sync example
dotnet run --project XrmSync -- webresources --folder "path/to/webresources" --solution-name "MySolution"

# Plugin analysis example
dotnet run --project XrmSync -- analyze --assembly "path/to/plugin.dll" --pretty-print
```

## Building and Testing

### Build Commands

```bash
# Standard build
dotnet build

# Release build
dotnet build -c Release

# Package for NuGet
dotnet pack XrmSync/XrmSync.csproj
```

### Testing Guidelines

- All new features should include unit tests
- Tests use NSubstitute for mocking
- Follow the AAA pattern (Arrange, Act, Assert)
- Sample projects in `Samples/` validate different plugin frameworks
- Ensure all tests pass before submitting a PR

### Local Installation

To test the tool as an installed global tool:

```bash
dotnet tool install --global --add-source ./XrmSync/nupkg XrmSync
```

## Making Changes

### Before You Start

1. Check [existing issues](https://github.com/delegateas/XrmSync/issues) to avoid duplicate work
2. For major changes, open an issue first to discuss the approach
3. Fork the repository and create a feature branch from `main`

### Branch Naming

Use descriptive branch names:
- `feature/add-custom-api-validation`
- `fix/webresource-sync-error`
- `refactor/improve-plugin-analyzer`

### Commit Messages

Write clear, descriptive commit messages:
- Use the imperative mood ("Add feature" not "Added feature")
- Start with a capital letter
- Keep the first line under 50 characters
- Add detailed description in the body if needed

Examples:
```
ADD: Validate webresource dependencies before removal
FIX: Improve output when failing due to webresource dependencies
REFACTOR: Move validation to Validation namespace
```

## Submitting Contributions

### Pull Request Process

1. **Update your fork**: Sync with the latest `main` branch
2. **Create a feature branch**: Branch from `main`
3. **Make your changes**: Follow code conventions and add tests
4. **Test thoroughly**: Run all tests and verify your changes
5. **Update documentation**: Update README.md, CLAUDE.md, or XML docs as needed
6. **Submit a PR**: Open a pull request with a clear description

### Pull Request Guidelines

- Provide a clear title and description
- Reference related issues (e.g., "Fixes #123" or "Relates to #456")
- Ensure all tests pass
- Keep PRs focused on a single feature or fix
- Be responsive to review feedback

### PR Description Template

```markdown
## Description
Brief description of changes

## Motivation
Why is this change needed?

## Changes Made
- Change 1
- Change 2

## Testing
How was this tested?

## Related Issues
Fixes #123
```

## Code Conventions

### General Guidelines

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Keep methods focused and single-purpose
- Add XML documentation comments for public APIs
- Use `ILogger<T>` for logging

### Project-Specific Conventions

- Use `InternalsVisibleTo` to expose internals to test projects
- Implement validation rules via `IValidationRule<T>`
- Follow the existing patterns for analyzers and readers/writers
- Commands should implement `IXrmSyncCommand` and extend `XrmSyncCommandBase`

### Code Organization

- Keep business logic in `SyncService`
- Place data access code in `Dataverse` project
- Shared models belong in `Model` project
- CLI-specific code stays in `XrmSync` project

## Project Architecture

### Key Projects

- **XrmSync**: CLI entry point and command definitions
- **SyncService**: Core synchronization business logic
- **AssemblyAnalyzer**: Reflection-based assembly analysis
- **Dataverse**: Data access layer for Dataverse SDK
- **Model**: Shared domain models and DTOs

### Adding New Features

#### Adding a New Command

1. Create a command class implementing `IXrmSyncCommand`
2. Define options using `System.CommandLine.Option<T>`
3. Implement execution logic with DI container
4. Register in `Program.cs` via `CommandLineBuilder`

#### Adding a Validation Rule

1. Create class implementing `IValidationRule<TEntity>` in `SyncService/PluginValidator/Rules`
2. Implement `Validate` method
3. Throw `ValidationException` on rule violation
4. Rule is auto-discovered via DI

#### Extending Plugin Framework Support

1. Create analyzer implementing `IAnalyzer<PluginDefinition>`
2. Implement attribute recognition in `AnalyzeTypes`
3. Register via DI in `ServiceCollectionExtensions`
4. Add sample project for testing

## Questions or Issues?

- Open an [issue](https://github.com/delegateas/XrmSync/issues) for bugs or feature requests
- Check the [CLAUDE.md](CLAUDE.md) for detailed architecture documentation
- Review existing issues and PRs for similar discussions

## License

By contributing to XrmSync, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to XrmSync!
