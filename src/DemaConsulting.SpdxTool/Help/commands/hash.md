---
title: hash command
summary: Write the SHA-256 hash of a file to a sidecar file, or verify a file against it.
tags:
  - reference
see-also:
  - commands
examples:
  - name: generate
    kind: command
    language: shell
    description: Write the SHA-256 hash of sbom.spdx.json to sbom.spdx.json.sha256.
    content: '{{tool}} hash generate sha256 sbom.spdx.json'
  - name: verify
    kind: command
    language: shell
    description: Check sbom.spdx.json against the hash that generate wrote.
    content: '{{tool}} hash verify sha256 sbom.spdx.json'
    expect: "Sha256 Digest OK for 'sbom.spdx.json'"
---
`hash` generates or verifies the hash of any file. On the command line
it takes exactly three arguments:

```text
{{tool}} hash generate sha256 <file>
{{tool}} hash verify sha256 <file>
```

As a workflow step:

```yaml
- command: hash
  inputs:
    operation: generate           # generate or verify
    algorithm: sha256
    file: <file>
```

`sha256` is the only algorithm. `generate` writes the hash as lowercase
hexadecimal text to `<file>.sha256`, as the command that [[#generate]]
prints shows. `verify` reads that file, in either case, hashes the file
again, and prints `Sha256 Digest OK for '<file>'`, as in the command
that [[#verify]] prints.

**Errors.** A missing file or sidecar file, or a hash that does not
match (`Sha256 hash mismatch for '<file>'`), exits 1. Another algorithm
or operation is a usage error.
