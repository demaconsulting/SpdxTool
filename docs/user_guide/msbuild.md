# MSBuild Integration

## Overview

The `DemaConsulting.SpdxTool.Targets` NuGet package provides seamless integration of SpdxTool SBOM
decoration directly into the `dotnet pack` build pipeline. This targets-only package works alongside
`Microsoft.Sbom.Targets` to automatically enhance generated SBOMs with additional metadata, validation,
or custom processing steps defined in a workflow file.

### What It Provides

- **Automatic SBOM Decoration**: Runs SpdxTool workflows on SBOMs during package creation
- **Build Pipeline Integration**: Hooks into MSBuild after SBOM generation
- **Zero Code Overhead**: Targets-only package with no runtime dependencies
- **Multi-Targeting Support**: Works correctly with both single and multi-targeted projects
- **Configurable Behavior**: Control decoration through MSBuild properties

### How It Works

The targets package implements an "unzip-decorate-rezip" pattern:

1. After `Microsoft.Sbom.Targets` generates an SBOM and embeds it in the .nupkg
2. The targets package extracts the .nupkg to a temporary directory
3. Runs `spdx-tool run-workflow` on the SBOM at `_manifest/spdx_2.2/manifest.spdx.json`
4. Repacks the modified contents back into the .nupkg file

This seamless integration ensures your decorated SBOM is included in the final package without manual
post-processing.

## Prerequisites

Before using MSBuild integration, ensure you have:

1. **Microsoft.Sbom.Targets Package**: Required to generate the initial SBOM that will be decorated
2. **SpdxTool Installation**: The `spdx-tool` command must be available, either:
   - **Local Tool** (Recommended): Installed via `.config/dotnet-tools.json` manifest

     ```bash
     dotnet new tool-manifest  # if first tool
     dotnet tool install --local DemaConsulting.SpdxTool
     ```

   - **Global Tool**: Installed globally (use `SpdxToolCommand` property to set to `spdx-tool`)

     ```bash
     dotnet tool install --global DemaConsulting.SpdxTool
     ```

## Quick Start

### Step 1: Add Package References

Add both packages to your project file:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Sbom.Targets" Version="4.1.5" PrivateAssets="All" />
  <PackageReference Include="DemaConsulting.SpdxTool.Targets" Version="1.0.0" PrivateAssets="All" />
</ItemGroup>
```

### Step 2: Enable SBOM Generation and Decoration

Configure MSBuild properties:

```xml
<PropertyGroup>
  <GenerateSBOM>true</GenerateSBOM>
  <DecorateSBOM>true</DecorateSBOM>
</PropertyGroup>
```

### Step 3: Create Workflow File

Create an `spdx-workflow.yaml` file in your project directory. This workflow defines the decoration
steps to apply to your SBOM:

```yaml
# Example SBOM decoration workflow
parameters:
  spdx-file: _manifest/spdx_2.2/manifest.spdx.json

steps:
# Validate the generated SBOM
- command: validate
  inputs:
    spdx: ${{ spdx-file }}
    ntia: true

# Add custom metadata
- command: update-package
  inputs:
    spdx: ${{ spdx-file }}
    package:
      id: SPDXRef-RootPackage
      comment: Decorated during build

# Print completion message
- command: print
  inputs:
    text:
    - SBOM decoration completed successfully
```

### Step 4: Build Your Package

Run the standard pack command:

```bash
dotnet pack
```

The SBOM decoration will occur automatically during the packaging process.

## MSBuild Properties

The targets package supports three configurable MSBuild properties:

| Property           | Default Value        | Description                                            |
| :----------------- | :------------------- | :----------------------------------------------------- |
| `DecorateSBOM`     | `false`              | Opt-in flag to enable SBOM decoration.                 |
| `SpdxWorkflowFile` | `spdx-workflow.yaml` | Path to the workflow file (relative to project root).  |
| `SpdxToolCommand`  | `dotnet spdx-tool`   | Command to invoke SpdxTool.                            |

### Example: Custom Configuration

```xml
<PropertyGroup>
  <GenerateSBOM>true</GenerateSBOM>
  <DecorateSBOM>true</DecorateSBOM>
  <SpdxWorkflowFile>build/custom-sbom-workflow.yaml</SpdxWorkflowFile>
  <SpdxToolCommand>spdx-tool</SpdxToolCommand>
</PropertyGroup>
```

## Complete Project Example

A complete project file showing typical MSBuild integration:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
    <OutputType>Library</OutputType>

    <!-- Enable packaging -->
    <IsPackable>true</IsPackable>
    <PackageId>MyCompany.MyPackage</PackageId>
    <Version>1.0.0</Version>

    <!-- Enable SBOM generation and decoration -->
    <GenerateSBOM>true</GenerateSBOM>
    <DecorateSBOM>true</DecorateSBOM>
  </PropertyGroup>

  <ItemGroup>
    <!-- SBOM generation and decoration packages -->
    <PackageReference Include="Microsoft.Sbom.Targets" Version="4.1.5" PrivateAssets="All" />
    <PackageReference Include="DemaConsulting.SpdxTool.Targets" Version="1.0.0" PrivateAssets="All" />
  </ItemGroup>

</Project>
```

## Example Workflow File

A comprehensive example workflow that validates, enhances, and documents the SBOM:

```yaml
# SBOM decoration workflow
parameters:
  spdx-file: _manifest/spdx_2.2/manifest.spdx.json

steps:
# Step 1: Validate the generated SBOM
- command: validate
  inputs:
    spdx: ${{ spdx-file }}
    ntia: true

# Step 2: Find the root package
- command: find-package
  inputs:
    spdx: ${{ spdx-file }}
    name: MyCompany.MyPackage
    output: root-package-id

# Step 3: Update package metadata
- command: update-package
  inputs:
    spdx: ${{ spdx-file }}
    package:
      id: ${{ root-package-id }}
      supplier: Organization: MyCompany
      originator: Organization: MyCompany

# Step 4: Add build information relationship
- command: add-relationship
  inputs:
    spdx: ${{ spdx-file }}
    id: ${{ root-package-id }}
    type: BUILD_TOOL_OF
    element: SPDXRef-Tool-dotnet

# Step 5: Print completion message
- command: print
  inputs:
    text:
    - SBOM validated and decorated successfully
```

> **Note**: `SPDXRef-Tool-dotnet` in Step 4 is an example element identifier. The referenced element
> must already exist in the SPDX document before `add-relationship` is called. If your SPDX document
> does not contain this element, add it first using `add-package`, or replace `SPDXRef-Tool-dotnet`
> with an element ID that is already present in your document.

## Multi-Targeting Projects

The targets package automatically handles multi-targeting projects correctly. When your project targets
multiple frameworks (e.g., `<TargetFrameworks>net8.0;net9.0</TargetFrameworks>`), the decoration:

- Runs **once** after all framework-specific builds complete
- Uses the `buildMultiTargeting` targets to prevent duplicate execution
- Applies the same workflow to the single generated NuGet package

No special configuration is required for multi-targeting scenarios.

## Disabling SBOM Decoration

SBOM decoration is opt-in by default. To disable decoration:

### Option 1: Set Property to False

```xml
<PropertyGroup>
  <DecorateSBOM>false</DecorateSBOM>
</PropertyGroup>
```

### Option 2: Remove the Property Entirely

Simply omit the `DecorateSBOM` property — it defaults to `false`.

### Option 3: Conditional Decoration

Disable decoration for specific build configurations:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DecorateSBOM>false</DecorateSBOM>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <DecorateSBOM>true</DecorateSBOM>
</PropertyGroup>
```

## Error Handling

The targets package provides clear error messages for common issues.

### Missing Workflow File

If the specified workflow file does not exist:

```text
error : SpdxTool workflow file not found: spdx-workflow.yaml.
        Create the file or set the SpdxWorkflowFile property to the correct path.
```

**Solution**: Create the workflow file at the specified path or adjust the `SpdxWorkflowFile` property.

### Workflow Execution Failure

If the workflow fails during execution:

```text
error : SBOM decoration workflow failed with exit code 1
[SpdxTool error output appears here]
```

**Solution**: Review the spdx-tool error output and fix the workflow file or SBOM content issues.

### SpdxTool Not Found

If the `spdx-tool` command is not available, the build will report an error indicating that
the `dotnet spdx-tool` command failed or was not found. The exact message depends on the
operating system and shell environment. Install SpdxTool as a local or global tool to resolve
this (see the Prerequisites section).

## Troubleshooting

### Decoration Not Running

Check the following prerequisites:

1. Verify `IsPackable=true` (project generates a package)
2. Verify `GenerateSBOM=true` (Microsoft.Sbom.Targets is enabled)
3. Verify `DecorateSBOM=true` (decoration is explicitly enabled)
4. Check that the workflow file exists at the specified path

### Workflow File Not Found

- Verify the file path is relative to the project directory
- Check file name spelling and capitalization
- Set `SpdxWorkflowFile` property to the correct path

### Decoration Runs Multiple Times

This can occur with multi-targeting if the wrong targets are imported:

- Ensure you have the latest version of `DemaConsulting.SpdxTool.Targets`
- The package should automatically handle multi-targeting via `buildMultiTargeting`

## Integration with CI/CD

The MSBuild targets integrate seamlessly with CI/CD pipelines. The following GitHub Actions example
ensures `dotnet tool restore` runs before `dotnet pack` so that `spdx-tool` is available:

```yaml
name: Build and Pack

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'

      # Restore tools from manifest (includes spdx-tool)
      - name: Restore tools
        run: dotnet tool restore

      # Pack with automatic SBOM decoration
      - name: Build and pack
        run: dotnet pack -c Release

      - name: Upload package
        uses: actions/upload-artifact@v4
        with:
          name: packages
          path: '**/*.nupkg'
```
