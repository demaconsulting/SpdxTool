### PathHelpers

#### Purpose

PathHelpers provides a single safe path-combination utility for commands that accept
user-supplied file paths. It enforces that the combined path never escapes the base
directory, protecting against directory traversal attacks.

#### Data Model

N/A — PathHelpers is a static class with no instance state.

#### Key Methods

**SafePathCombine(string, string)**: Combines a base path and a relative path after
validating that the relative path contains no traversal sequences and is not absolute.

- *Parameters*: `string basePath` — the base directory path; `string relativePath` — the
  caller-supplied relative path.
- *Returns*: `string` — the combined path.
- *Preconditions*: Neither parameter may be null.
- *Post-conditions*: Returns `Path.Combine(basePath, relativePath)` when the relative path is
  safe; the returned path is always within basePath.

#### Error Handling

**ArgumentNullException** — thrown when basePath or relativePath is null.

**ArgumentException** — thrown when relativePath contains ".." components or is an absolute
path; also thrown when the fully resolved combined path escapes basePath (defense-in-depth
check).

#### Dependencies

- System.IO.Path (Combine, IsPathRooted, GetFullPath, GetRelativePath)

#### Callers

- RunWorkflow — calls SafePathCombine to validate NuGet-embedded workflow file paths before
  resolving them against the package cache root
