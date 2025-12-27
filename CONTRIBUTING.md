# Contributing to SpdxTool

Thank you for your interest in contributing to SpdxTool! This document provides guidelines and instructions for
contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Submitting Changes](#submitting-changes)
- [Release Process](#release-process)

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please read
[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## Getting Started

### Prerequisites

- .NET SDK 8.0, 9.0, or 10.0
- Git
- A code editor (Visual Studio, VS Code, or Rider recommended)

### Setting Up Your Development Environment

1. Fork the repository on GitHub
2. Clone your fork locally:

   ```bash
   git clone https://github.com/YOUR-USERNAME/SpdxTool.git
   cd SpdxTool
   ```

3. Add the upstream repository as a remote:

   ```bash
   git remote add upstream https://github.com/demaconsulting/SpdxTool.git
   ```

4. Restore .NET tools:

   ```bash
   dotnet tool restore
   ```

5. Build the project:

   ```bash
   dotnet build
   ```

6. Run the tests:

   ```bash
   dotnet test
   ```

## Development Workflow

### Creating a Branch

Create a feature branch for your work:

```bash
git checkout -b feature/your-feature-name
```

Use descriptive branch names:

- `feature/` for new features
- `fix/` for bug fixes
- `docs/` for documentation changes
- `refactor/` for code refactoring
- `test/` for test improvements

### Making Changes

1. Make your changes in small, logical commits
2. Write clear, descriptive commit messages
3. Keep commits focused on a single change
4. Run tests frequently to catch issues early

### Running Quality Checks

Before submitting your changes, run the following checks:

```bash
# Build the project
dotnet build

# Run all tests
dotnet test

# Run code analysis (warnings will be treated as errors)
dotnet build /p:TreatWarningsAsErrors=true

# Run self-validation
dotnet run --project src/DemaConsulting.SpdxTool -- --validate

# Generate code coverage (optional)
dotnet test --collect:"XPlat Code Coverage"
```

The CI pipeline will also run markdown linting and spell checking on all documentation.

## Coding Standards

### C# Style Guidelines

- Follow the `.editorconfig` settings in the repository
- Use meaningful variable and method names
- Keep methods focused and concise (prefer < 50 lines)
- Add XML documentation comments for public APIs
- Use nullable reference types appropriately

### Code Organization

- One class per file
- Organize using directives (System.* first, then others alphabetically)
- Group related functionality together
- Keep file length reasonable (prefer < 500 lines)

### Documentation

- All public APIs must have XML documentation comments
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags as appropriate
- Write clear, concise documentation
- Update documentation when changing functionality

### Error Handling

- Use exceptions for exceptional conditions
- Throw specific exception types when appropriate
- Use `CommandUsageException` for command usage errors
- Use `CommandErrorException` for command execution errors
- Include helpful error messages

## Testing Guidelines

### Test Organization

- Tests are located in `test/DemaConsulting.SpdxTool.Tests/`
- Test files follow the naming convention: `[Component]Tests.cs`
- Use MSTest framework attributes: `[TestClass]`, `[TestMethod]`

### Writing Tests

- Follow the AAA (Arrange, Act, Assert) pattern
- Write descriptive test method names that explain what is being tested
- Each test should verify one specific behavior
- Use meaningful assertion messages
- Clean up resources in test cleanup methods if needed

### Test Coverage

- Aim for high test coverage (> 80%)
- All new features must include tests
- Bug fixes should include regression tests
- Test both success and failure scenarios

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific framework
dotnet test --framework net8.0

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Submitting Changes

### Before Submitting

1. Ensure all tests pass
2. Verify code builds without warnings
3. Update documentation if needed
4. Add tests for new functionality
5. Rebase your branch on the latest upstream main:

   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

### Creating a Pull Request

1. Push your branch to your fork:

   ```bash
   git push origin feature/your-feature-name
   ```

2. Create a pull request on GitHub
3. Fill out the pull request template completely
4. Link any related issues
5. Wait for review and address feedback

### Pull Request Guidelines

- Provide a clear description of the changes
- Include motivation and context
- Reference related issues
- Keep pull requests focused and reasonably sized
- Respond to review comments promptly
- Be open to suggestions and constructive feedback

## Release Process

Releases are managed by project maintainers following these steps:

1. Update version numbers in project files
2. Update CHANGELOG.md with release notes
3. Create a release branch
4. Tag the release
5. Build and publish NuGet packages
6. Create a GitHub release with release notes

Contributors do not need to worry about versioning or releases.

## Additional Resources

- [SPDX Specification](https://spdx.dev/)
- [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [MSTest Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)

## Questions?

If you have questions about contributing, please:

1. Check existing documentation
2. Search for similar issues or discussions
3. Open a new discussion on GitHub
4. Contact the maintainers

Thank you for contributing to SpdxTool!
