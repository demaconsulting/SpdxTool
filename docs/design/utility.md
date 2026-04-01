# DemaConsulting.SpdxTool Utility Subsystem Design

## Purpose

The Utility subsystem provides cross-cutting helper units used throughout the
DemaConsulting.SpdxTool system. It encompasses the `Utility` and `Spdx` namespaces,
which offer path-safety utilities, wildcard pattern matching, SPDX-domain helpers,
and SPDX relationship direction support.

## Units

### PathHelpers

`PathHelpers` provides safe file path operations that prevent path-traversal attacks.
The `SafePathCombine` method validates that a relative path contains no `..` components
or absolute path roots before combining it with a base path, throwing `ArgumentException`
when unsafe components are detected.

### Wildcard

`Wildcard` provides glob-style pattern matching for file names and paths. It converts
wildcard patterns (using `*` and `?`) into regular expressions and performs
case-insensitive matching. Used by commands that accept file patterns (e.g.,
`find-package`) to filter SPDX document contents.

### SpdxHelpers

`SpdxHelpers` provides SPDX-domain utility methods consumed across the Commands
subsystem, including relationship traversal helpers and package attribute manipulation.

### RelationshipDirection

`RelationshipDirection` is an enumeration expressing the traversal direction of an
SPDX relationship query: `Parent`, `Child`, or `Sibling`. Consumed by query and
find operations in the Commands subsystem.

## Design Constraints

- All utilities are stateless; no instance members are required.
- `PathHelpers.SafePathCombine` must reject any path component that is `..` or is
  an absolute path root, to prevent directory traversal vulnerabilities.
- `Wildcard` pattern matching is case-insensitive to ensure consistent cross-platform
  behavior on both case-sensitive (Linux) and case-insensitive (Windows, macOS)
  file systems.
