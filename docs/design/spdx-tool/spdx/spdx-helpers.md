### SpdxHelpers

#### Purpose

SpdxHelpers provides two shared document-level helpers used by all commands that read or
write SPDX JSON files: LoadJsonDocument and SaveJsonDocument. Centralizing these operations
ensures consistent error reporting, SPDX 2.x JSON format handling, and creator stamping
across every command that touches the file system.

#### Data Model

N/A — SpdxHelpers is a static class with no instance state.

#### Key Methods

**LoadJsonDocument(string)**: Verifies the file exists, reads its content, and deserializes
it into an SpdxDocument using Spdx2JsonDeserializer.

- *Parameters*: `string spdxFile` — path to the SPDX JSON file.
- *Returns*: `SpdxDocument`
- *Preconditions*: spdxFile must be a non-null path.
- *Post-conditions*: Returns a fully deserialized SpdxDocument.

**SaveJsonDocument(SpdxDocument, string)**: Appends the tool creator entry (if not already
present), serializes the document to JSON using Spdx2JsonSerializer, and writes the result
to the specified file path.

- *Parameters*: `SpdxDocument doc` — the document to serialize; `string spdxFile` — output
  file path.
- *Returns*: `void`
- *Preconditions*: doc must not be null; spdxFile must be a writable path.
- *Post-conditions*: The file at spdxFile contains the serialized document with the tool
  creator entry appended.

#### Error Handling

**ArgumentNullException** — thrown by LoadJsonDocument when spdxFile is null, and by
SaveJsonDocument when doc or spdxFile is null.

**CommandUsageException** — thrown by LoadJsonDocument when the specified file does not exist.

**IOException / UnauthorizedAccessException** — may be propagated by SaveJsonDocument if the
output file cannot be written.

#### Dependencies

- SpdxDocument (DemaConsulting.SpdxModel)
- Spdx2JsonDeserializer, Spdx2JsonSerializer (DemaConsulting.SpdxModel.IO)
- CommandUsageException (Commands — thrown on missing file)
- Program.Version (DemaConsulting.SpdxTool system unit — used to build the creator tool name)
- System.IO.File (ReadAllText, WriteAllText, Exists)

#### Callers

- All command implementations that load or save SPDX JSON files
- SelfTest subsystem units that exercise SPDX file operations
