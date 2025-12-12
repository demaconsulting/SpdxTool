# SpdxTool Architecture

This document describes the architecture and design of the SpdxTool project.

## Overview

SpdxTool is a .NET command-line tool for manipulating SPDX (Software Package Data Exchange) SBOM (Software Bill of Materials) files. It provides a flexible command-based architecture that supports both direct command-line usage and workflow-driven automation.

## Project Structure

```
SpdxTool/
├── src/
│   └── DemaConsulting.SpdxTool/          # Main tool implementation
│       ├── Commands/                      # Command implementations
│       ├── SelfValidation/                # Self-validation logic
│       ├── Spdx/                          # SPDX helper utilities
│       ├── Utility/                       # General utilities
│       ├── Context.cs                     # Execution context
│       └── Program.cs                     # Entry point
├── test/
│   └── DemaConsulting.SpdxTool.Tests/    # Unit tests
├── docs/                                  # User documentation
└── .github/workflows/                     # CI/CD pipelines
```

## Core Components

### Program Entry Point

**File**: `Program.cs`

The main entry point handles:
- Command-line argument parsing
- Context creation and lifecycle management
- Top-level exception handling
- Version and help display
- Self-validation orchestration

Key responsibilities:
- Creates a `Context` from command-line arguments
- Delegates execution to `Run()` method
- Handles standard error reporting and exit codes

### Context

**File**: `Context.cs`

The `Context` class encapsulates the execution environment:
- Command-line arguments
- Output handling (console and/or log file)
- Silent mode support
- Exit code tracking
- Validation result file path

Key features:
- Implements `IDisposable` for proper resource cleanup
- Provides `WriteLine()` and `WriteError()` for output
- Supports both console output and file logging simultaneously

### Command Architecture

**Directory**: `Commands/`

Commands follow a plugin-style architecture:

1. **Command Base Class** (`Command.cs`)
   - Abstract base for all commands
   - Defines two `Run()` methods:
     - Command-line mode: `Run(Context, string[])`
     - Workflow mode: `Run(Context, YamlMappingNode, Dictionary<string, string>)`
   - Provides utility methods for YAML processing and variable expansion

2. **Command Registry** (`CommandRegistry.cs`)
   - Static registry of all available commands
   - Maps command names to `CommandEntry` objects
   - Provides command lookup and enumeration

3. **Command Entry** (`CommandEntry.cs`)
   - Metadata about a command
   - Command name, usage string, summary, and help text
   - Reference to singleton command instance

4. **Individual Commands**
   - Each command is implemented as a sealed class with a singleton instance
   - Examples: `AddPackage`, `Validate`, `RunWorkflow`, `ToMarkdown`
   - Each command defines:
     - Command name constant
     - Singleton instance
     - Command entry metadata
     - Implementation of both `Run()` methods

### Command Types

Commands fall into several categories:

1. **Document Manipulation**
   - `AddPackage`: Add packages to SPDX documents
   - `UpdatePackage`: Modify existing packages
   - `CopyPackage`: Copy packages between documents
   - `RenameId`: Rename element IDs
   - `AddRelationship`: Create relationships between elements

2. **Query and Analysis**
   - `FindPackage`: Locate packages by criteria
   - `GetVersion`: Extract package versions
   - `Query`: Query external program output
   - `Validate`: Validate SPDX documents for correctness

3. **Reporting and Visualization**
   - `ToMarkdown`: Generate Markdown summaries
   - `Diagram`: Create Mermaid diagrams of relationships
   - `Print`: Output text (for workflow debugging)

4. **Workflow Support**
   - `RunWorkflow`: Execute workflow YAML files
   - `SetVariable`: Manage workflow variables (workflow-only)

5. **Utilities**
   - `Help`: Display extended command help
   - `Hash`: Generate and verify file hashes

### Workflow Engine

**File**: `Commands/RunWorkflow.cs`

The workflow engine enables automation through YAML-based workflows:

**Features**:
- Parameter substitution using `${{ variable }}` syntax
- Step-by-step command execution
- Variable management and propagation
- Support for local files and remote URLs
- Optional integrity checking (SHA256)
- Output capture to workflow variables

**Workflow Format**:
```yaml
parameters:
  param1: value1
  param2: value2

steps:
  - command: command-name
    inputs:
      input1: ${{ param1 }}
      input2: literal-value
    outputs:
      output1: variable-name
```

**Variable Expansion**:
- Supports nested expansion: `${{ var_${{ name }} }}`
- Environment variable access: `${{ environment.VAR_NAME }}`
- Runtime error if variable is undefined

### Self-Validation System

**Directory**: `SelfValidation/`

A unique feature that allows the tool to validate itself:

**Purpose**:
- Provides evidence of tool correctness for regulated environments
- Tests core functionality in production builds
- Generates validation reports in TRX format

**Components**:
- `Validate.cs`: Main validation orchestrator
- `Validate[Command].cs`: Per-command validation tests
- Each validator creates temporary files and executes commands

**Output**:
- Markdown report with system information
- Pass/fail status for each validated command
- Optional TRX file for CI/CD integration

### SPDX Helpers

**Directory**: `Spdx/`

Utilities for working with SPDX data:

- `SpdxHelpers.cs`: Common SPDX operations
  - Document loading and saving (JSON/YAML)
  - Package lookup by ID or criteria
  - Relationship creation and management
  - Version extraction

- `RelationshipDirection.cs`: Enum for relationship directionality

### Utility Classes

**Directory**: `Utility/`

General-purpose utilities:

- `Wildcard.cs`: Wildcard pattern matching using regular expressions
  - Converts `*` and `?` patterns to regex
  - Case-insensitive matching
  - Timeout protection (100ms)

### Exception Handling

Custom exception types for clear error reporting:

1. **CommandUsageException**
   - Thrown when command is used incorrectly
   - Results in usage information being displayed
   - Non-zero exit code

2. **CommandErrorException**
   - Thrown when command execution fails
   - Error message displayed to user
   - No usage information shown
   - Non-zero exit code

## Design Patterns

### Singleton Pattern

Each command is implemented as a singleton:
```csharp
public sealed class MyCommand : Command
{
    public static readonly MyCommand Instance = new();
    private MyCommand() { }
}
```

**Benefits**:
- Single instance per command type
- Stateless command objects
- Easy registration in CommandRegistry

### Command Pattern

Commands encapsulate operations:
- Each command is self-contained
- Commands can be executed independently
- Commands support both CLI and workflow modes

### Factory Pattern

The CommandRegistry acts as a factory:
- Maps command names to instances
- Provides command lookup
- Centralizes command registration

### Strategy Pattern

Two execution strategies per command:
- Command-line mode: direct argument parsing
- Workflow mode: YAML-based configuration

## Data Flow

### Command-Line Execution

```
User Input
    ↓
Program.Main()
    ↓
Context.Create()
    ↓
Program.Run()
    ↓
CommandRegistry.Lookup()
    ↓
Command.Run(context, args)
    ↓
SPDX Operations
    ↓
Console/Log Output
```

### Workflow Execution

```
Workflow YAML
    ↓
RunWorkflow.Run()
    ↓
Parse YAML
    ↓
Initialize Variables
    ↓
For Each Step:
    ↓
    Expand Variables
    ↓
    Lookup Command
    ↓
    Command.Run(context, step, variables)
    ↓
    Capture Outputs
    ↓
Console/Log Output
```

## Dependencies

### External Libraries

1. **DemaConsulting.SpdxModel**
   - SPDX data model implementation
   - Document serialization/deserialization
   - SPDX specification compliance

2. **YamlDotNet**
   - YAML parsing and manipulation
   - Workflow file processing
   - Configuration handling

3. **DemaConsulting.TestResults** (production)
   - TRX (Test Results XML) file generation
   - Self-validation reporting

### Test Dependencies

- Microsoft.NET.Test.Sdk
- MSTest.TestAdapter
- MSTest.TestFramework
- coverlet.collector (code coverage)

## Build and Deployment

### Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

Multi-targeting ensures broad compatibility while using latest language features (C# 12).

### NuGet Package

The tool is distributed as a .NET Tool package:
- Package ID: `DemaConsulting.SpdxTool`
- Tool command: `spdx-tool`
- Includes symbol packages (.snupkg) for debugging

### Continuous Integration

GitHub Actions workflows:
- Build on Windows, Linux, and macOS
- Multi-framework testing
- SonarCloud analysis for quality metrics
- Code coverage reporting
- SBOM generation using Microsoft SBOM Tool
- Self-validation execution
- Package creation and publishing

## Testing Strategy

### Unit Tests

Located in `test/DemaConsulting.SpdxTool.Tests/`:
- One test file per command/component
- MSTest framework
- AAA (Arrange-Act-Assert) pattern
- High coverage target (>80%)

### Test Utilities

- `Runner.cs`: Helper for running external processes
- Temporary file creation for testing
- Assertion helpers

### Integration Testing

- Self-validation serves as integration tests
- Workflow tests verify end-to-end scenarios
- Tests run against all target frameworks

## Error Handling Strategy

1. **Validation Errors**: Use `CommandUsageException` with clear messages
2. **Execution Errors**: Use `CommandErrorException` with helpful context
3. **System Errors**: Allow exceptions to propagate (caught at top level)
4. **File Errors**: Provide specific error messages about missing/invalid files
5. **YAML Errors**: Include location information when available

## Extensibility

### Adding New Commands

1. Create command class implementing `Command` base
2. Define command singleton and entry
3. Implement both `Run()` methods
4. Register in `CommandRegistry`
5. Add tests in test project
6. Update documentation

### Adding Self-Validation

1. Create `Validate[Command].cs` in `SelfValidation/`
2. Implement validation logic
3. Register in main `Validate.cs` orchestrator
4. Validate creates files, runs command, verifies results

## Security Considerations

- No external network calls (except workflow URLs)
- File system access is explicit (user-provided paths)
- No privilege escalation
- Input validation on all user-provided data
- Regular dependency updates through CI
- SonarCloud security scanning

## Performance Characteristics

- Fast startup: < 1 second typical
- Memory-efficient: SPDX documents processed in-memory
- I/O bound: Performance limited by file operations
- No caching: Stateless operation model
- Timeout protection: Regex operations have timeouts

## Future Considerations

Potential areas for enhancement:
- Plugin architecture for custom commands
- Remote SPDX repository integration
- Batch processing capabilities
- Interactive mode
- Advanced query language
- Caching for repeated operations
- Parallel workflow execution

## Conclusion

SpdxTool's architecture prioritizes:
- **Simplicity**: Clear command structure
- **Extensibility**: Easy to add new commands
- **Testability**: Comprehensive test coverage
- **Reliability**: Self-validation and CI/CD
- **Maintainability**: Well-organized codebase
- **Standards Compliance**: Follows SPDX specification

The modular design allows independent development and testing of commands while maintaining a consistent user experience.
