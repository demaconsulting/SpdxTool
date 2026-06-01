## Utility

### Overview

The Utility subsystem provides stateless cross-cutting helper units shared across the
DemaConsulting.SpdxTool system (namespace DemaConsulting.SpdxTool.Utility). It contains three
units:

- TemporaryDirectory - disposable self-validation temporary folders rooted under the current
  working directory.
- PathHelpers - safe path combining that rejects path-traversal sequences.
- Wildcard - glob-style pattern matching via compiled regular expressions.

TemporaryDirectory is consumed by the SelfTest subsystem and by the test suite. PathHelpers and
Wildcard are consumed by the Commands subsystem. The subsystem
has no internal dispatcher; units are invoked directly by their consumers through static method
calls.

### Interfaces

**TemporaryDirectory**: Creates a unique temporary directory under the current working directory,
exposes safe file-path resolution within that directory, and deletes the directory when disposed.

- *Type*: Internal sealed class implementing `IDisposable`.
- *Role*: Provider.
- *Contract*: Creates a unique directory under `Environment.CurrentDirectory` on construction,
  returns file paths under that directory via `GetFilePath`, and removes the directory tree on
  disposal without surfacing cleanup failures.
- *Constraints*: The directory is intentionally process-relative to avoid symlink-resolution
  issues on platforms whose temp paths resolve to a different physical location.

**PathHelpers.SafePathCombine**: Combines a base path with a relative path after validating that
the relative path contains no ".." components and is not rooted. A secondary check using
Path.GetFullPath confirms the resolved path remains under the base directory.

- *Type*: Static method (internal).
- *Role*: Provider.
- *Contract*: Returns the combined path when the relative path is safe; throws ArgumentException
  when the path contains ".." sequences, is rooted, or resolves outside the base directory.
- *Constraints*: Used exclusively for NuGet-embedded workflow paths in the RunWorkflow command;
  not a general-purpose path utility.

**Wildcard.IsMatch**: Determines whether an input string matches a wildcard pattern that may
contain * (any sequence of characters) and ? (any single character). Matching is case-insensitive
and uses a regular expression derived from the wildcard pattern, with a 100-millisecond
evaluation timeout to guard against catastrophic backtracking.

- *Type*: Static method (public within `internal static class Wildcard`; the test assembly
  accesses this via `InternalsVisibleTo` which grants access to the internal class).
- *Role*: Provider.
- *Contract*: Returns true when the input matches the entire pattern from start to end; returns
  false otherwise.
- *Constraints*: Matching is always case-insensitive; the regex evaluation timeout is fixed at
  100 milliseconds.

### Design

PathHelpers is invoked by the RunWorkflow command when resolving workflow file paths within a
NuGet package cache directory. It ensures that no step in an embedded workflow can reference a
file outside the package directory by applying both an upfront string check (rejects ".." and
rooted paths) and a defense-in-depth check using Path.GetFullPath to confirm the resolved path
stays under the base directory.

TemporaryDirectory is invoked by the self-test orchestration path and by test code that needs an
isolated, disposable working directory for temporary fixtures. It creates a unique subdirectory
under the current working directory, uses GUID-based names to avoid collisions, and deletes the
directory tree on disposal.

Wildcard.IsMatch is invoked by the FindPackage command and other commands that accept glob-style
name filters to match against SPDX package names or element IDs. The conversion of a wildcard
pattern to a regular expression is performed inline on each call; patterns are not cached.
