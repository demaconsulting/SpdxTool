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

**AbsolutePathRejection**: absolute path inputs are rejected regardless of content. This scenario
is tested by `PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException`.

**NullArgumentRejection**: null base-path or relative-path arguments are rejected with
ArgumentNullException. This scenario is tested by
`PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException` and
`PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException`.

**ValidRelativePaths**: nested relative paths remain valid and combine under the intended base
directory. This scenario is tested by `PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly`,
`PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly`,
`PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly`, and
`PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath`.

**AsteriskWildcard**: asterisk wildcards match variable-length substrings without losing
case-insensitive behavior. This scenario is tested by
`Wildcard_IsMatch_AsteriskPattern_MatchesMultipleChars`.

**QuestionMarkWildcard**: question mark wildcards match exactly one character. This scenario is
tested by `Wildcard_IsMatch_QuestionMarkPattern_MatchesSingleChar`.

**ExactMatch**: exact patterns match equal strings case-insensitively and do not match
strings that differ in content or length. This scenario is tested by
`Wildcard_IsMatch_ExactMatch_ReturnsTrue`.

**NullArguments**: null input or pattern arguments throw ArgumentNullException immediately.
This scenario is tested by `Wildcard_IsMatch_NullInput_ThrowsArgumentNullException` and
`Wildcard_IsMatch_NullPattern_ThrowsArgumentNullException`.

**EmptyStringBoundary**: empty input and empty pattern boundary conditions are handled
correctly — an empty input matches an empty pattern, a non-empty input does not match an
empty pattern, and an asterisk matches the empty string. This scenario is tested by
`Wildcard_IsMatch_EmptyInputs_BehavesCorrectly`.
