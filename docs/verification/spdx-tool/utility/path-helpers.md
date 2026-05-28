## PathHelpers

### Verification Approach

`PathHelpers` is verified with focused unit tests in
`test/DemaConsulting.SpdxTool.Tests/Utility/PathHelpersTests.cs`. The tests verify the
containment checks that prevent directory traversal and absolute path injection.

### Test Environment

Tests execute in the xUnit v3 environment as part of `DemaConsulting.SpdxTool.Tests`. No
external service or file system access beyond in-process path manipulation is required.

### Acceptance Criteria

Verification is acceptable when path traversal and rooted-path inputs are rejected with
`ArgumentException`, and safe relative paths are preserved and combined correctly.

### Test Scenarios

**PathTraversalProtection**: path traversal inputs containing parent-directory segments are
rejected. This scenario is tested by
`PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException` and
`PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException`.

**AbsolutePathRejection**: absolute paths (Unix and Windows) are rejected. This scenario is
tested by `PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException`.

**ValidRelativePaths**: safe relative paths combine under the intended base directory. This
scenario is tested by `PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly`,
`PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly`,
`PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly`, and
`PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath`.
