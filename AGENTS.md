# Agent Quick Reference

Project-specific guidance for agents working on SpdxTool - a .NET CLI tool for
manipulating SPDX SBOM files.

## Available Specialized Agents

- **Requirements Agent** - Develops requirements and ensures test coverage linkage
- **Technical Writer** - Creates accurate documentation following regulatory best practices
- **Software Developer** - Writes production code and self-validation tests in literate style
- **Test Developer** - Creates unit and integration tests following AAA pattern
- **Code Quality Agent** - Enforces linting, static analysis, and security standards
- **Repo Consistency Agent** - Ensures SpdxTool remains consistent with TemplateDotNetTool patterns

## Tech Stack

- C# 12, .NET 8.0/9.0/10.0, dotnet CLI, NuGet

## Key Files

- **`.editorconfig`** - Code style (file-scoped namespaces, 4-space indent, UTF-8+BOM, LF endings)
- **`.cspell.json`, `.markdownlint-cli2.jsonc`, `.yamllint.yaml`** - Linting configs

## Testing

- **Test Naming**: `SpdxTool_FeatureBeingValidated` for self-validation tests
- **Self-Validation**: All tests run via `--validate` flag and can output TRX format
- **Test Framework**: Uses DemaConsulting.TestResults library for test result generation

## Code Style

- **XML Docs**: On ALL members (public/internal/private) with spaces after `///` in summaries
- **Errors**: `ArgumentException` for parsing, `InvalidOperationException` for runtime issues
- **Namespace**: File-scoped namespaces only
- **Using Statements**: Top of file only (no nested using declarations except for IDisposable)
- **String Formatting**: Use interpolated strings ($"") for clarity

## Project Structure

- **Context.cs**: Handles command-line argument parsing, logging, and output
- **Program.cs**: Main entry point with version/help/validation routing
- **SelfValidation/**: Self-validation tests with TRX output support

## Build and Test

```bash
# Build the project
dotnet build --configuration Release

# Run unit tests
dotnet test --configuration Release

# Run self-validation
dotnet run --project src/DemaConsulting.SpdxTool \
  --configuration Release --framework net10.0 --no-build -- --validate
```

## Documentation

- **User Guide**: `docs/guide/`
- **Command Reference**: `docs/spdx-tool-command-line.md`
- **CHANGELOG.md**: Not present - changes are captured in the auto-generated build notes

## Markdown Link Style

- **AI agent markdown files** (`.github/agents/*.md`): Use inline links `[text](url)` so URLs are visible in agent context
- **README.md**: Use absolute URLs (shipped in NuGet package)
- **All other markdown files**: Use reference-style links `[text][ref]` with `[ref]: url` at document end

## CI/CD

- **Quality Checks**: Markdown lint, spell check, YAML lint
- **Build**: Multi-platform (Windows/Linux)
- **CodeQL**: Security scanning
- **Integration Tests**: .NET 8/9/10 on Windows/Linux

## Common Tasks

```bash
# Format code
dotnet format

# Restore dotnet tools
dotnet tool restore

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Boundaries and Guardrails

- **NEVER** modify files within the `/obj/` or `/bin/` directories
- **NEVER** commit secrets, API keys, or sensitive configuration data
- **NEVER** disable code analysis warnings without proper justification and review
- **ASK FIRST** before making significant architectural changes to the core library logic

## Agent Report Files

When agents need to write report files to communicate with each other or the user, follow these guidelines:

- **Naming Convention**: Use the pattern `AGENT_REPORT_xxxx.md` (e.g., `AGENT_REPORT_analysis.md`, `AGENT_REPORT_results.md`)
- **Purpose**: These files are for temporary inter-agent communication and should not be committed
- **Exclusions**: Files matching `AGENT_REPORT_*.md` are automatically:
  - Excluded from git (via .gitignore)
  - Excluded from markdown linting
  - Excluded from spell checking
