## YamlDotNet

### Purpose

YamlDotNet provides the YAML parsing infrastructure used by the RunWorkflow command to load and
traverse workflow YAML files. It was chosen because its RepresentationModel API gives direct,
schema-free access to the YAML node tree, which is required to support the flexible step argument
structure used in SpdxTool workflow files.

### Features Used

**RepresentationModel API** — `YamlStream`, `YamlDocument`, `YamlMappingNode`,
`YamlSequenceNode`, and `YamlScalarNode` are used to load a YAML document and navigate its node
graph without requiring a predefined schema class.

**String-keyed node access** — mapping nodes are accessed by string key using the YamlDotNet
implicit string-to-`YamlScalarNode` conversion, which simplifies workflow argument extraction.

### Integration Pattern

YamlDotNet is referenced as a NuGet package dependency in `DemaConsulting.SpdxTool`. It is
consumed by the `RunWorkflow` command unit, which instantiates a `YamlStream`, calls `Load` with
the workflow file reader, and traverses the resulting node graph to extract step definitions and
arguments. No explicit initialization or disposal is required beyond standard `TextReader` usage.
