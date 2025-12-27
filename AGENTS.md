# AI Instructions for SpdxTool

This file provides specific context and instructions for AI coding agents to
interact effectively with this C# project.

## Project Overview

SpdxTool is a C# .NET tool for manipulating SPDX SBOM files.

## Technologies and Dependencies

* **Language**: C# 12
* **.NET Frameworks**: .NET 8, 9, and 10
* **Primary Dependencies**: [DemaConsulting.SpdxModel, YamlDotNet]

## Project Structure

The repository is organized as follows:

* `/.config/`: Contains the .NET Tool configuration.
* `/.github/workflows/`: Contains the CI/CD pipeline configurations.
* `/docs/`: Contains usage documentation.
* `/src/DemaConsulting.SpdxTool/`: Contains the library source code.
* `/test/DemaConsulting.SpdxTool.Tests/`: Contains the library unit tests.
* `/DemaConsulting.SpdxTool.sln`: The main Visual Studio solution file.

## Development Commands

Use these commands to perform common development tasks:

* **Restore DotNet Tools**:

  ```bash
  dotnet tool restore
  ```

* **Build the Project**:

  ```bash
  dotnet build
  ```

* **Run All Tests**:

  ```bash
  dotnet test
  ```

## Testing Guidelines

* Tests are located under the `/test/DemaConsulting.SpdxTool.Tests/` folder and use the MSTest framework.
* Test files should end with `.cs` and adhere to the naming convention `[Component]Tests.cs`.
* All new features should be tested with comprehensive unit tests.
* The build must pass all tests and static analysis warnings before merging.
* Tests should be written using the AAA (Arrange, Act, Assert) pattern.

## Code Style and Conventions

* Follow standard C# naming conventions (PascalCase for classes/methods/properties, camelCase for local variables).
* Nullable reference types are enabled at the project level (`<Nullable>enable</Nullable>` in .csproj files). Do
  not use file-level `#nullable enable` directives.
* Warnings are treated as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
* Avoid public fields; prefer properties.

## Quality Tools and Practices

* **Code Analysis**: The project uses multiple analyzers configured via Directory.Build.props:
  * Microsoft.CodeAnalysis.NetAnalyzers for .NET best practices
  * SonarAnalyzer.CSharp for additional code quality rules
  * All warnings are treated as errors
* **EditorConfig**: The .editorconfig file enforces consistent code style across IDEs
* **Code Coverage**: Use `dotnet test --collect:"XPlat Code Coverage"` to generate coverage reports
* **SonarCloud**: The CI pipeline integrates with SonarCloud for continuous quality monitoring
* **Static Analysis**: Run `dotnet build` to perform compile-time static analysis

### Running Quality Checks Locally

Before committing code, developers should run:

```bash
# Restore dependencies
dotnet restore

# Build with all analysis enabled (will fail on warnings)
dotnet build

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run self-validation
dotnet run --project src/DemaConsulting.SpdxTool -- --validate
```

## Boundaries and Guardrails

* **NEVER** modify files within the `/obj/` or `/bin/` directories.
* **NEVER** commit secrets, API keys, or sensitive configuration data.
* **NEVER** disable code analysis warnings without proper justification and review.
* **ASK FIRST** before making significant architectural changes to the core library logic.
