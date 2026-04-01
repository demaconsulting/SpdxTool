# DemaConsulting.SpdxTool Targets Subsystem Design

## Purpose

The Targets subsystem provides the `DemaConsulting.SpdxTool.Targets` NuGet package,
which integrates SPDX document decoration into the standard `dotnet pack` build workflow
via MSBuild targets. It allows projects to automatically decorate their NuGet-generated
SBOMs during the pack process without manual intervention.

## Architecture

### Build Target Integration

The subsystem consists of two MSBuild `.targets` files:

- `build/DemaConsulting.SpdxTool.Targets.targets` — injected for single-TFM projects
- `buildMultiTargeting/DemaConsulting.SpdxTool.Targets.targets` — injected for
  multi-TFM projects

Both files define the `DecorateNuGetSbom` target, which runs after the `Pack` target
in the MSBuild pipeline.

### Workflow Invocation

The `DecorateNuGetSbom` target conditionally invokes `spdx-tool run-workflow` with
a user-supplied workflow file. The workflow file path is specified via the
`SpdxToolWorkflow` MSBuild property. The `spdx-tool` global tool must be installed
and available on the system `PATH`.

### Configuration Properties

| MSBuild Property     | Default | Description                                          |
|----------------------|---------|------------------------------------------------------|
| `DecorateSBOM`       | `false` | Set to `true` to enable SBOM decoration during pack  |
| `GenerateSBOM`       | `true`  | When `false`, skips decoration (no SBOM to decorate) |
| `SpdxToolWorkflow`   | —       | Path to the workflow YAML file for decoration        |

## Conditional Execution

The `DecorateNuGetSbom` target is skipped when:

- `DecorateSBOM` is not set to `true` (opt-in required)
- `GenerateSBOM` is `false` (no SBOM generated to decorate)
- `SpdxToolWorkflow` path does not exist (build error reported)

## Data Flow

```text
dotnet pack
      │
      ▼
Pack target completes (NuGet .nupkg + embedded SBOM generated)
      │
      ▼
DecorateNuGetSbom target
      │
      ├─► Check DecorateSBOM == true  (skip if false)
      │
      ├─► Check GenerateSBOM == true  (skip if false)
      │
      ├─► Check SpdxToolWorkflow exists  (error if missing)
      │
      └─► Execute: spdx-tool run-workflow <SpdxToolWorkflow>
                        │
                        └─► Workflow modifies the SPDX JSON embedded in .nupkg
```

## Design Constraints

- The Targets subsystem has no direct dependency on the SpdxTool source code; it
  invokes `spdx-tool` as an external process via MSBuild `Exec` task.
- SBOM decoration is opt-in (`DecorateSBOM` must be explicitly set to `true`).
- The subsystem gracefully skips decoration when prerequisites are not met, rather
  than failing silently or producing incomplete output.
- Multi-TFM projects use a separate targets file to handle the outer build correctly.
