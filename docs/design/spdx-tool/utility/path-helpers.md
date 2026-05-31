### PathHelpers

#### Purpose

PathHelpers provides a single safe path-combination utility for commands that accept
user-supplied file paths. It enforces that the combined path never escapes the base
directory, protecting against directory traversal attacks.

#### Data Model

N/A — PathHelpers is a static class with no instance state.

#### Key Methods

**SafePathCombine(string, params string[])**: Combines a base path with one or more
caller-supplied relative path segments, validating each segment before appending.

- *Parameters*: `string basePath` — the base directory path; `params string[] relativePaths` —
  one or more caller-supplied relative path segments to append in order. Neither the array nor
  any individual segment may be null.
- *Returns*: `string` — the combined path.
- *Preconditions*: `basePath` and `relativePaths` must not be null; each element of
  `relativePaths` must not be null.
- *Post-conditions*: Returns the result of combining `basePath` with each segment in order;
  the returned path is always within `basePath`.

*Algorithm*:

1. **Null guard** — `ArgumentNullException.ThrowIfNull` is called on `basePath` and
   `relativePaths`; a null argument is rejected immediately.
2. **Per-segment loop** — for each segment: (a) null-check the segment; (b) upfront string
   check — `Contains("..")` rejects traversal sequences; `Path.IsPathRooted` rejects absolute
   paths; (c) defense-in-depth check — `Path.GetFullPath` resolves both paths and
   `Path.GetRelativePath` confirms the combined result stays under `basePath`; (d) the running
   combined path is updated for the next iteration.

#### Error Handling

**ArgumentNullException** — thrown when `basePath`, `relativePaths`, or any individual
segment within `relativePaths` is null.

**ArgumentException** — thrown when any segment contains ".." components or is an absolute
path; also thrown when the fully resolved combined path escapes `basePath` (defense-in-depth
check).

#### Dependencies

- System.IO.Path (Combine, IsPathRooted, GetFullPath, GetRelativePath)

#### Callers

- RunWorkflow — calls SafePathCombine to validate NuGet-embedded workflow file paths before
  resolving them against the package cache root
