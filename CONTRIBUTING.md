# Contributing to SpdxTool

Thank you for your interest in contributing to SpdxTool! This document provides guidelines and instructions for
contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Requirements Management](#requirements-management)
- [Submitting Changes](#submitting-changes)
- [Release Process](#release-process)

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please read
[CODE_OF_CONDUCT.md][code-of-conduct] before contributing.

## Getting Started

### Prerequisites

- .NET SDK 10.0 and 11.0 (both are required: the 11.0 SDK builds the `net11.0` target and the
  .NET 10 runtime runs the `net10.0` tests)
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

The `build.ps1`, `lint.ps1`, and `fix.ps1` scripts require a scope; run `pwsh ./<script> -Help` for the
switches. While iterating, scope the work to your changes:

```pwsh
pwsh ./build.ps1 -Changed    # build and test only the projects the changes affect
pwsh ./fix.ps1 -Changed      # auto-fix only the changed file types
pwsh ./lint.ps1 -Changed     # run only the lint checks whose inputs changed
```

Before submitting your changes, run the full gates once:

```pwsh
pwsh ./build.ps1 -All        # build, unit tests, and self-validation
pwsh ./fix.ps1 -All          # apply every auto-fixer
pwsh ./lint.ps1 -All         # every lint check (what CI runs)
```

The individual steps are:

```bash
# Build the project
dotnet build --configuration Release

# Run unit tests
dotnet test --configuration Release

# Run self-validation tests
dotnet run --project src/DemaConsulting.SpdxTool \
  --configuration Release --framework net10.0 --no-build -- --validate

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

### Help Cards

The built-in help (`spdx-tool help`) is served from Markdown help cards in
`src/DemaConsulting.SpdxTool/Help/`, which the DemaConsulting.HelpCards package embeds in the tool assembly.
`HelpCommand.cs` is the only code that uses the package.

- **One file per topic.** The path below `Help/`, without `.md`, is the card id. `_source.md` names the source
  (`name:`) and sets `schema: 2`.
- **Front matter.** Every card has a `title`, a `summary` (at most 80 characters for a topic, 100 otherwise), and
  exactly one purpose tag: `tutorial`, `reference`, or `troubleshooting`. Add `aliases` for topic names that users
  commonly guess, such as `examples`, so `help <name>` finds the card.
- **New topics.** Add the id of every new top-level card to `HelpCommand.RootCardIds`, the one list of topics; a
  missing or extra topic fails the tests.
- **Commands.** Every command registered in `CommandsRegistry` has a card `Help/commands/<command>.md` with its
  command-line and workflow-step usage; a test fails when one is missing. `spdx-tool help <command>` prints that
  card: HelpCards resolves the first path word only against the topics, so `HelpCommand` inserts `commands`
  before a first path word that names a registered command. Do not add command names as aliases of a card.
- **Examples.** Give every task a runnable `command` example, one command per example: no pipes, redirection, or
  `&&`. Wrap `content` in single quotes and use double quotes inside the command, which bash, PowerShell, and cmd
  read alike. State the result when it matters with `exit:` (default 0) and `expect:`, a short fragment that must
  appear somewhere in the console output; `expect` cannot check a file that the command writes. Keep each
  `description` to about 50 characters.
- **Getting started.** Say how to install the tool for CI: the .NET version it needs (from `TargetFrameworks`), a
  local tool manifest with `dotnet tool restore` before `dotnet spdx-tool`, and the global install as the
  alternative. Mark the install commands as `snippet` examples so that the example runner does not run them.
- **Input files.** Show complete files only for formats that users author, such as workflow files, and describe
  SPDX documents in prose. The SPDX documents that the examples read live in
  `test/DemaConsulting.SpdxTool.Tests/TestData/HelpExamples/`; the example-runner test copies them into
  `invocation.WorkingDirectory` before every command.
- **Width.** A printed card may not be wider than 120 characters. Links print as full commands (`[[#name]]` as
  about 50 characters) and `{{version}}` of a development build as about 46, so keep body lines near 75 characters
  with at most one link each.
- **Text.** Cards are linted and spell-checked like any Markdown file: no body line starting with `#`, no wide
  tables, and every fenced block names its language.

Run `pwsh ./build.ps1 -Changed` after changing a card. The `HelpCommand` unit tests validate every card, with
warnings treated as errors, and check its printed width; an integration test runs every command example against
the built tool and compares the result with the card.

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

#### Unit Tests

```bash
# Run all unit tests
dotnet test --configuration Release

# Run tests for a specific framework
dotnet test --configuration Release --framework net10.0

# Run specific unit test
dotnet test --filter "FullyQualifiedName~YourTestName"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Self-Validation Tests

```bash
# Run self-validation tests
dotnet run --project src/DemaConsulting.SpdxTool \
  --configuration Release --framework net10.0 --no-build -- --validate
```

## Requirements Management

SpdxTool uses [DemaConsulting.ReqStream][reqstream] for requirements traceability:

- All requirements are defined in `requirements.yaml`
- Each requirement must be linked to test cases
- Run `dotnet reqstream` to generate requirements documentation
- Use the `--enforce` flag to ensure all requirements have test coverage

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
2. Create a release branch
3. Tag the release
4. Build and publish NuGet packages
5. Create a GitHub release with auto-generated build notes

Contributors do not need to worry about versioning or releases.

## Additional Resources

- [SPDX Specification][spdx-spec]
- [.NET Coding Conventions][dotnet-conventions]
- [MSTest Documentation][mstest-docs]

## Questions?

If you have questions about contributing, please:

1. Check existing documentation
2. Search for similar issues or discussions
3. Open a new discussion on GitHub
4. Contact the maintainers

Thank you for contributing to SpdxTool!

[code-of-conduct]: CODE_OF_CONDUCT.md
[reqstream]: https://github.com/demaconsulting/ReqStream
[spdx-spec]: https://spdx.dev/
[dotnet-conventions]: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
[mstest-docs]: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest
