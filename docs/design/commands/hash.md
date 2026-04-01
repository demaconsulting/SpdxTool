# DemaConsulting.SpdxTool hash Command Design

## Purpose

The `hash` command generates or verifies SHA-256 hash files. It is available from
the command-line and from workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool hash generate sha256 <file>
spdx-tool hash verify sha256 <file>
```

- `generate` — Computes the SHA-256 hash of `<file>` and writes it to `<file>.sha256`.
- `verify` — Reads the expected hash from `<file>.sha256`, recomputes, and compares.

### Workflow YAML usage

```yaml

- command: hash

  inputs:
    operation: generate | verify  # Required
    algorithm: sha256             # Required (currently only sha256 supported)
    file: <file>                  # Required: file to hash or verify
```

## Implementation

1. Reads `operation`, `algorithm`, and `file` inputs.
2. Validates `algorithm`; only `"sha256"` is currently accepted.
3. Dispatches to `GenerateSha256` or `VerifySha256` based on `operation`.

### `GenerateSha256(file)`

1. Calls `CalculateSha256(file)` to compute the digest.
2. Writes the hex digest to `file + ".sha256"`.

### `VerifySha256(context, file)`

1. Checks that `file.sha256` exists; raises `CommandErrorException` if not.
2. Reads the stored digest (trimmed).
3. Calls `CalculateSha256(file)` to recompute.
4. Compares digests; raises `CommandErrorException` on mismatch.
5. Writes a success message to the context on match.

### `CalculateSha256(file)`

1. Verifies the file exists; raises `CommandErrorException` if not.
2. Opens a `FileStream` and uses `SHA256.ComputeHash` to compute the digest.
3. Returns the lowercase hex string.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Not exactly 3 CLI arguments | `CommandUsageException` |
| Missing `operation` input (workflow) | `YamlException` |
| Missing `algorithm` input (workflow) | `YamlException` |
| Missing `file` input (workflow) | `YamlException` |
| Unsupported algorithm | `CommandUsageException` |
| Unknown operation | `CommandUsageException` |
| File not found | `CommandErrorException` |
| Hash file not found (verify) | `CommandErrorException` |
| Hash mismatch | `CommandErrorException` |
| I/O error computing hash | `CommandErrorException` |

## Constraints

- Currently only `sha256` is supported as an algorithm.
- The hash file is always `<filename>.sha256` (appended, not replaced).
- Variable expansion is applied to all string inputs via `GetMapString`.
