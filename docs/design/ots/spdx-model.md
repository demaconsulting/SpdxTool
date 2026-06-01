## SpdxModel

### Purpose

DemaConsulting.SpdxModel provides the SPDX document object model types and JSON serialization
used throughout SpdxTool. It was chosen because it is the canonical model library for the
SpdxTool program and defines the `SpdxDocument`, `SpdxPackage`, `SpdxRelationship`, and related
types that all commands operate on.

### Features Used

**Document model types** — `SpdxDocument`, `SpdxPackage`, `SpdxRelationship`, `SpdxFile`,
`SpdxSnippet`, and related value types are used by all commands that read, modify, or create SPDX
content.

**Spdx2JsonDeserializer** — deserializes SPDX 2.x JSON files into `SpdxDocument` instances; used
by `SpdxHelpers.LoadJsonDocument`.

**Spdx2JsonSerializer** — serializes `SpdxDocument` instances to SPDX 2.x JSON output; used by
`SpdxHelpers.SaveJsonDocument`, which also stamps the creator tool field before writing.

**Validation API** — the SPDX model's package and relationship validation helpers are used by the
Validate command to enforce NTIA minimum-element and SPDX-schema compliance.

### Integration Pattern

DemaConsulting.SpdxModel is referenced as a NuGet package dependency in
`DemaConsulting.SpdxTool`. All document load and save operations pass through the `SpdxHelpers`
utility unit, which centralizes creation and disposal of serializer instances. Individual command
units depend on `SpdxHelpers` rather than calling the serializers directly.
