# Development Scripts

This directory contains helper scripts for local development and quality assurance.

## Quality Check Script

The quality check script runs a comprehensive set of checks before committing code.

### Usage

**Linux/macOS:**
```bash
./scripts/quality-check.sh
```

**Windows (PowerShell):**
```powershell
.\scripts\quality-check.ps1
```

### What It Checks

1. **Clean**: Removes all build artifacts
2. **Restore**: Restores NuGet dependencies
3. **Build**: Compiles the solution with all code analyzers enabled
4. **Test**: Runs all unit tests across all target frameworks
5. **Code Analysis**: Checks for any compiler warnings
6. **Self-Validation**: Runs the tool's self-validation suite

### Exit Codes

- `0`: All checks passed
- `1`: One or more checks failed

### Best Practices

- Run this script before creating a pull request
- Run this script after making significant changes
- Fix any issues reported before committing
- The CI pipeline runs similar checks, so passing locally increases the chance of CI success

## Future Scripts

Additional scripts may be added here for:
- Code formatting
- Documentation generation
- Release automation
- Dependency updates
