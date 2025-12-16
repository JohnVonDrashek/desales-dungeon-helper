# Contributing to MyProject

Thank you for your interest in contributing! This document provides guidelines and information about contributing to this project.

## Code of Conduct

Please be respectful and constructive in all interactions.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/yourproject.git`
3. Create a branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Run tests: `dotnet test`
6. Commit your changes: `git commit -m 'Add some feature'`
7. Push to your fork: `git push origin feature/your-feature-name`
8. Open a Pull Request

## Development Setup

### Prerequisites
- .NET 10.0 SDK or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Building
```bash
dotnet restore
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Code Formatting
```bash
# Check formatting
dotnet format --verify-no-changes

# Auto-fix formatting
dotnet format
```

## Coding Standards

### C# Conventions
- Use file-scoped namespaces
- Enable nullable reference types
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes

### Naming Conventions
- `PascalCase` for public members, types, and namespaces
- `camelCase` with underscore prefix for private fields (`_privateField`)
- `Async` suffix for async methods
- `I` prefix for interfaces

### Documentation
- Add XML documentation comments to all public APIs
- Update README.md if adding new features
- Update CHANGELOG.md for notable changes

## Pull Request Process

1. Ensure all tests pass
2. Update documentation as needed
3. Update CHANGELOG.md under `[Unreleased]`
4. Fill out the pull request template completely
5. Request review from maintainers

### PR Checklist
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] No new warnings
- [ ] Code follows project style

## Reporting Bugs

Use the [Bug Report template](.github/ISSUE_TEMPLATE/bug_report.md) when creating issues for bugs.

Include:
- Clear description of the bug
- Steps to reproduce
- Expected vs actual behavior
- Environment details

## Requesting Features

Use the [Feature Request template](.github/ISSUE_TEMPLATE/feature_request.md) when suggesting new features.

Include:
- Problem description
- Proposed solution
- Alternative approaches considered

## Questions?

Feel free to open a discussion or issue if you have questions about contributing.
