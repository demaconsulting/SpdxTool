### TemporaryDirectory

#### Verification Approach

`TemporaryDirectory` is verified with focused unit tests in
`test/DemaConsulting.SpdxTool.Tests/Utility/TemporaryDirectoryTests.cs`. The tests verify
construction, uniqueness, safe file-path resolution, intermediate directory creation, traversal
rejection, and cleanup behavior.

#### Test Environment

Tests execute in the standard xUnit v3 environment with no external dependencies. The helper
creates and deletes local temporary directories only.

#### Acceptance Criteria

Verification is acceptable when the constructor creates a directory, distinct instances have
distinct paths, file paths stay within the temporary directory, intermediate subdirectories are
created on demand, traversal attempts are rejected, and disposal removes the directory tree
without throwing when cleanup has already occurred.

#### Test Scenarios

**DirectoryCreation**: construction creates the directory on disk. This scenario is tested by
`TemporaryDirectory_Constructor_CreatesDirectory`.

**UniquePaths**: multiple instances receive distinct directory paths. This scenario is tested by
`TemporaryDirectory_Constructor_CreatesUniqueDirectories`.

**SafeFileResolution**: file paths remain under the temporary directory and nested paths create
intermediate subdirectories. This scenario is tested by
`TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory` and
`TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories`.

**TraversalProtection**: path-traversal attempts are rejected. This scenario is tested by
`TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException`.

**CleanupOnDispose**: disposal removes the directory tree, and disposing after external cleanup is
non-fatal. This scenario is tested by
`TemporaryDirectory_Dispose_DeletesDirectory` and
`TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow`.
