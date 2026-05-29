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

*Algorithm*:

1. **Null guard** — `ArgumentNullException.ThrowIfNull` is called on both parameters; a null
   argument is rejected immediately before any path operation.
2. **Upfront string check** — `relativePath.Contains("..")` rejects any path whose string
   representation contains `..` as a substring (a conservative check that also catches filenames
   like `"file..txt"`); `Path.IsPathRooted` rejects absolute paths. Either condition throws
   `ArgumentException`.
3. **Defense-in-depth check** — `Path.GetFullPath` resolves both paths and
   `Path.GetRelativePath` confirms the combined result stays under basePath. This guards against
   platform-specific normalization edge cases that could bypass step 2.

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
