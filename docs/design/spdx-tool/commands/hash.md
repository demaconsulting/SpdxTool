### Hash

#### Purpose

Hash generates or verifies a SHA-256 hash for a file. In generate mode it computes the SHA-256
digest and writes it to a sidecar file with the ".sha256" extension. In verify mode it reads the
sidecar file and compares its stored digest against the freshly computed one. It is available from
both the CLI and workflow YAML files.

#### Data Model

**Instance**: `Hash` — the singleton instance registered with CommandsRegistry.

**Entry**: `CommandEntry` — the CommandEntry record for Hash.

#### Key Methods

**Run(Context, string[])**: Validates that exactly three arguments are provided and calls
DoHashOperation.

- *Parameters*: `Context context` — execution context; `string[] args` — [operation, algorithm,
  file].
- *Returns*: `void`
- *Preconditions*: args.Length must be exactly 3.
- *Post-conditions*: The hash operation is performed.

**Run(Context, YamlMappingNode, Dictionary)**: Parses operation, algorithm, and file inputs from
the YAML step node and calls DoHashOperation.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: operation, algorithm, and file inputs are required.
- *Post-conditions*: The hash operation is performed.

**DoHashOperation(Context, string, string, string)**: Validates the algorithm (only "sha256" is
supported), then dispatches to GenerateSha256 or VerifySha256 based on the operation.

- *Parameters*: `Context context` — execution context; `string operation` — "generate" or "verify";
  `string algorithm` — hash algorithm name; `string file` — target file path.
- *Returns*: `void`
- *Preconditions*: algorithm must be "sha256"; operation must be "generate" or "verify".
- *Post-conditions*: The sidecar .sha256 file is written (generate) or verified (verify).

**GenerateSha256(string)**: Computes the SHA-256 digest of the file and writes it to file + ".sha256".

- *Parameters*: `string file` — path to the file to hash.
- *Returns*: `void`
- *Preconditions*: file must exist.
- *Post-conditions*: A sidecar file (file + ".sha256") is written containing the lowercase hex digest.

**VerifySha256(Context, string)**: Reads the sidecar .sha256 file, computes the current digest, and
compares them. Writes a confirmation message to context on success.

- *Parameters*: `Context context` — execution context; `string file` — path to the file to verify.
- *Returns*: `void`
- *Preconditions*: file must exist. A sidecar file (file + ".sha256") must exist.
- *Post-conditions*: Confirms or rejects the file integrity.

**CalculateSha256(string)**: Opens the file as a stream and computes its SHA-256 digest using
System.Security.Cryptography.SHA256. Returns the digest as a lowercase hex string.

- *Parameters*: `string file` — file path.
- *Returns*: `string` — lowercase hexadecimal SHA-256 digest.
- *Preconditions*: file must exist.
- *Post-conditions*: Returns the digest string without side effects.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when the argument count is not exactly
3; thrown by DoHashOperation for an unsupported algorithm or unrecognized operation.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when operation, algorithm,
or file inputs are missing.

**CommandErrorException** — thrown by VerifySha256 when the sidecar file does not exist or the
digest does not match; thrown by CalculateSha256 when the file does not exist or an IO exception
occurs during hashing; thrown by GenerateSha256 indirectly via CalculateSha256.

#### Dependencies

- Command (abstract base class)
- System.Security.Cryptography.SHA256
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: hash
