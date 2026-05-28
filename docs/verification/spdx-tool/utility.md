## Utility

### Verification Approach

The Utility subsystem is verified with focused unit tests in
`test/DemaConsulting.SpdxTool.Tests/Utility/`. The tests verify the containment checks used by
`PathHelpers` and the wildcard-to-regular-expression behavior implemented by `Wildcard`.

### Test Environment

N/A - the subsystem is verified in the standard xUnit v3 environment with no special setup beyond
the test runner because both utility units are pure in-process helpers.

### Acceptance Criteria

Verification is acceptable when path traversal and rooted-path inputs are rejected, safe relative
paths are preserved, and wildcard matching behaves correctly for exact, `*`, and `?` patterns.

### Test Scenarios

**PathTraversalProtection**: path traversal inputs containing parent-directory segments are
rejected. This scenario is tested by
`PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException`.

**NestedRelativePaths**: nested relative paths remain valid and combine under the intended base
directory. This scenario is tested by `PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly`.

**AsteriskWildcard**: asterisk wildcards match variable-length substrings without losing
case-insensitive behavior. This scenario is tested by
`Wildcard_AsteriskPattern_MatchesMultipleChars`.

**QuestionMarkWildcard**: question mark wildcards match exactly one character. This scenario is
tested by `Wildcard_QuestionMarkPattern_MatchesSingleChar`.
