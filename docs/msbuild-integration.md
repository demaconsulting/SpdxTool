# MSBuild Integration

The `DemaConsulting.SpdxTool.Targets` NuGet package integrates SpdxTool SBOM
decoration directly into the `dotnet pack` pipeline. When used alongside
[Microsoft.Sbom.Targets][ms-sbom-targets], it automatically runs an SpdxTool
workflow to decorate the generated SBOM inside the NuGet package.

## How It Works

The integration follows this sequence during `dotnet pack`:

1. **Pack** — MSBuild creates the `.nupkg` file
2. **GenerateSbomTarget** — `Microsoft.Sbom.Targets` unzips the `.nupkg`,
   generates the SBOM, and re-zips the package
3. **DecorateSbomTarget** — `DemaConsulting.SpdxTool.Targets` unzips the
   `.nupkg`, runs the SpdxTool workflow to decorate the SBOM, and re-zips the
   package

## Installation

Add both `Microsoft.Sbom.Targets` and `DemaConsulting.SpdxTool.Targets` to
your project:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Sbom.Targets" Version="4.1.5" PrivateAssets="All" />
  <PackageReference Include="DemaConsulting.SpdxTool.Targets" Version="1.0.0" PrivateAssets="All" />
</ItemGroup>
```

## Configuration

Enable SBOM generation and decoration in your project file:

```xml
<PropertyGroup>
  <!-- Enable Microsoft SBOM generation -->
  <GenerateSBOM>true</GenerateSBOM>

  <!-- Enable DEMA SBOM decoration -->
  <DecorateSBOM>true</DecorateSBOM>
</PropertyGroup>
```

### Properties

| Property           | Default                                          | Description                            |
| :----------------- | :----------------------------------------------- | :------------------------------------- |
| `DecorateSBOM`     | `false`                                          | Enable SBOM decoration during pack     |
| `SpdxWorkflowFile` | `$(MSBuildProjectDirectory)/spdx-workflow.yaml`  | Path to the SpdxTool workflow file     |
| `SpdxToolCommand`  | `dotnet spdx-tool`                               | Command to invoke SpdxTool             |

### SpdxToolCommand

The `SpdxToolCommand` property controls how SpdxTool is invoked. The default
value `dotnet spdx-tool` works when the tool is managed as a local tool via
`.config/dotnet-tools.json`. If you have installed spdx-tool globally, you
can override this to use the bare command:

```xml
<PropertyGroup>
  <SpdxToolCommand>spdx-tool</SpdxToolCommand>
</PropertyGroup>
```

## Workflow File

Create an `spdx-workflow.yaml` file in your project directory. This workflow
runs with the working directory set to the unzipped NuGet package contents,
where the SBOM is located at `_manifest/spdx_2.2/manifest.spdx.json`.

### Example Workflow

```yaml
parameters:
  sbom: _manifest/spdx_2.2/manifest.spdx.json

steps:
- command: update-package
  inputs:
    spdx: ${{ sbom }}
    package:
      id: SPDXRef-RootPackage
      supplier: "Organization: My Company"
      download: https://example.com/my-package
```

## Complete Example

### Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GenerateSBOM>true</GenerateSBOM>
    <DecorateSBOM>true</DecorateSBOM>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Sbom.Targets" Version="4.1.5" PrivateAssets="All" />
    <PackageReference Include="DemaConsulting.SpdxTool.Targets" Version="1.0.0" PrivateAssets="All" />
  </ItemGroup>
</Project>
```

### spdx-workflow.yaml

```yaml
parameters:
  sbom: _manifest/spdx_2.2/manifest.spdx.json

steps:
- command: print
  inputs:
    text:
    - Decorating SBOM

- command: validate
  inputs:
    spdx: ${{ sbom }}
```

### Build

```bash
dotnet pack --configuration Release
```

The resulting `.nupkg` will contain a decorated SBOM.

## Multi-Targeting Projects

For multi-targeting projects, the decoration runs exactly once during the
outer build — not once per target framework:

```xml
<PropertyGroup>
  <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
  <GenerateSBOM>true</GenerateSBOM>
  <DecorateSBOM>true</DecorateSBOM>
</PropertyGroup>
```

## Opt-Out

SBOM decoration is disabled by default. To explicitly disable it:

```xml
<PropertyGroup>
  <DecorateSBOM>false</DecorateSBOM>
</PropertyGroup>
```

When `DecorateSBOM` is `false` or not set, SpdxTool is not invoked and no
decoration occurs.

## Error Handling

If the workflow file is not found, the build produces a clear error message:

```text
error : SpdxTool workflow file not found: /path/to/spdx-workflow.yaml.
Create the file or set the SpdxWorkflowFile property to the correct path.
```

## Conditions

The `DecorateSbomTarget` only runs when all of the following are true:

- `IsPackable` is `true`
- `DecorateSBOM` is `true`
- `GenerateSBOM` is `true`

This ensures the target is skipped for non-packable projects and when SBOM
generation itself is disabled.

[ms-sbom-targets]: https://github.com/microsoft/sbom-tool
