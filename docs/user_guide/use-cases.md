# Use Cases and Best Practices

## Use Cases

### SBOM Generation and Validation

A common workflow for validating an SPDX document and generating a human-readable summary:

```yaml
parameters:
  spdx-file: manifest.spdx.json
  output-file: sbom-summary.md

steps:
# Validate the SPDX document
- command: validate
  inputs:
    spdx: ${{ spdx-file }}
    ntia: true

# Print validation success
- command: print
  inputs:
    text:
    - SPDX document is valid

# Generate markdown summary
- command: to-markdown
  inputs:
    spdx: ${{ spdx-file }}
    markdown: ${{ output-file }}
    title: Software Bill of Materials
```

### Dependency Version Tracking

Track and update dependency versions in an SPDX document:

```yaml
parameters:
  spdx-file: manifest.spdx.json

steps:
# Get current .NET SDK version
- command: query
  inputs:
    output: dotnet-version
    pattern: '(?<value>\d+\.\d+\.\d+)'
    program: dotnet
    arguments:
    - '--version'

# Find .NET SDK package in SPDX
- command: find-package
  inputs:
    output: dotnet-package-id
    spdx: ${{ spdx-file }}
    name: .NET SDK

# Update .NET SDK version
- command: update-package
  inputs:
    spdx: ${{ spdx-file }}
    package:
      id: ${{ dotnet-package-id }}
      version: ${{ dotnet-version }}
```

### Multi-Document SBOM Assembly

Combine packages from multiple SPDX documents into a single output document:

```yaml
parameters:
  component-spdx: component.spdx.json
  output-spdx: combined.spdx.json

steps:
# Copy component packages to output
- command: copy-package
  inputs:
    from: ${{ component-spdx }}
    to: ${{ output-spdx }}
    package: SPDXRef-Component
    recursive: true
    files: true
    relationships:
    - type: DEPENDS_ON
      element: SPDXRef-Document

# Validate combined document
- command: validate
  inputs:
    spdx: ${{ output-spdx }}
```

## Best Practices

### SPDX Document Organization

- **Consistent Naming**: Use consistent ID naming conventions (e.g., `SPDXRef-Component-Name`)
- **Document Structure**: Organize packages hierarchically with clear relationships
- **Version Control**: Keep SPDX documents in version control
- **Automation**: Use workflow files for repeatable SBOM operations

### Workflow Design

- **Modularity**: Break complex operations into reusable workflow files
- **Variables**: Use parameters for configurable values
- **Comments**: Add comments to explain workflow logic
- **Validation**: Always validate SPDX documents after modifications
- **Error Handling**: Check command outputs and handle failures

### CI/CD Best Practices

- **Automated Generation**: Generate SBOMs automatically in build pipelines
- **Validation Gates**: Fail builds on SBOM validation errors
- **Artifact Publishing**: Publish SBOMs as build artifacts
- **NTIA Compliance**: Validate for NTIA minimum elements when required
- **Documentation**: Generate markdown summaries for human review

### Security and Compliance

- **Regular Updates**: Keep SpdxTool updated to the latest version
- **License Compliance**: Ensure all package licenses are correctly specified
- **Vulnerability Tracking**: Integrate with vulnerability databases
- **Audit Trail**: Log all SBOM operations for audit purposes
- **Access Control**: Restrict SBOM modifications to authorized processes
