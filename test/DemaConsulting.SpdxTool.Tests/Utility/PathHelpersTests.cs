// Copyright (c) 2024 DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.SpdxTool.Utility;

namespace DemaConsulting.SpdxTool.Tests.Utility;

/// <summary>
///     Tests for the PathHelpers class.
/// </summary>
/// <remarks>
///     Unit tests for <see cref="PathHelpers"/>. Each test is self-contained and exercises
///     a single input scenario against SafePathCombine. Tests cover the happy path, path
///     traversal rejection, absolute path rejection, and null argument rejection.
/// </remarks>
public class PathHelpersTests
{
    /// <summary>
    ///     Test that SafePathCombine correctly combines valid paths.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly()
    {
        // Arrange: create a base path and a valid relative path
        var basePath = "/home/user/project";
        var relativePath = "subfolder/file.txt";

        // Act: invoke SafePathCombine with the test inputs
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: result equals Path.Combine output
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path traversal with double dots.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException()
    {
        // Arrange: relative path with parent-directory traversal segment
        var basePath = "/home/user/project";
        var relativePath = "../etc/passwd";

        // Act & Assert: path traversal attempt is rejected
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path with double dots in middle.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException()
    {
        // Arrange: relative path with embedded traversal segment
        var basePath = "/home/user/project";
        var relativePath = "subfolder/../../../etc/passwd";

        // Act & Assert: embedded traversal is rejected
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for absolute paths.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException()
    {
        // Arrange: Unix absolute path used as the relative argument
        var unixBasePath = "/home/user/project";
        var unixRelativePath = "/etc/passwd";

        // Act & Assert: Unix absolute path is rejected
        var unixException = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(unixBasePath, unixRelativePath));
        Assert.Contains("Invalid path component", unixException.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for Windows absolute paths.
    /// </summary>
    /// <remarks>
    ///     Windows drive-letter paths are only rooted on the Windows platform. This test is skipped
    ///     on non-Windows platforms where <c>Path.IsPathRooted</c> returns false for
    ///     <c>C:\...</c> strings, making the guard unreachable.
    /// </remarks>
    [Fact]
    public void PathHelpers_SafePathCombine_WindowsAbsolutePath_ThrowsArgumentException()
    {
        // Skip on non-Windows platforms where Windows drive-letter paths are not recognized as rooted
        if (!OperatingSystem.IsWindows())
        {
            throw Xunit.Sdk.SkipException.ForSkip("Windows absolute-path guard only applies on Windows");
        }

        // Arrange: Windows absolute path used as the relative argument
        var windowsBasePath = "C:\\Users\\project";
        var windowsRelativePath = "C:\\Windows\\System32\\file.txt";

        // Act & Assert: Windows absolute path is rejected
        var windowsException = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(windowsBasePath, windowsRelativePath));
        Assert.Contains("Invalid path component", windowsException.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles current directory reference.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly()
    {
        // Arrange: relative path starting with a current-directory reference
        var basePath = "/home/user/project";
        var relativePath = "./subfolder/file.txt";

        // Act: invoke SafePathCombine with the test inputs
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: result equals Path.Combine output
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles nested paths.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly()
    {
        // Arrange: deeply nested relative path
        var basePath = "/home/user/project";
        var relativePath = "level1/level2/level3/file.txt";

        // Act: invoke SafePathCombine with the test inputs
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: result equals Path.Combine output
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles empty relative path.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath()
    {
        // Arrange: empty relative path
        var basePath = "/home/user/project";
        var relativePath = "";

        // Act: invoke SafePathCombine with the test inputs
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: result equals Path.Combine output
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly combines multiple path segments in sequence.
    /// </summary>
    /// <remarks>
    ///     Verifies the params overload: each segment is validated and appended in order,
    ///     producing the same result as nested single-segment calls.
    /// </remarks>
    [Fact]
    public void PathHelpers_SafePathCombine_MultipleSegments_CombinesCorrectly()
    {
        // Arrange: base path and multiple valid relative segments
        var basePath = "/home/user/project";

        // Act: invoke SafePathCombine with multiple segments
        var result = PathHelpers.SafePathCombine(basePath, "level1", "level2", "file.txt");

        // Assert: result equals Path.Join output for the same segments
        Assert.Equal(Path.Join(basePath, "level1", "level2", "file.txt"), result);
    }

    /// <summary>
    ///     Test that SafePathCombine rejects traversal in a later segment of a multi-segment call.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_TraversalInLaterSegment_ThrowsArgumentException()
    {
        // Arrange: valid first segment, traversal in second
        var basePath = "/home/user/project";

        // Act & Assert: traversal in any segment is rejected
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, "level1", "../etc/passwd"));
        Assert.Contains("Invalid path component", exception.Message);
    }


    /// <remarks>
    ///     Verifies the documented null-argument contract: passing null for basePath must throw
    ///     ArgumentNullException before any path combination is attempted.
    /// </remarks>
    [Fact]
    public void PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException()
    {
        // Act & Assert: null basePath is rejected before any path operation
        Assert.Throws<ArgumentNullException>(() =>
            PathHelpers.SafePathCombine(null!, "relative/path"));
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentNullException when relativePath is null.
    /// </summary>
    /// <remarks>
    ///     Verifies the documented null-argument contract: passing null for relativePath must throw
    ///     ArgumentNullException before any path combination is attempted.
    /// </remarks>
    [Fact]
    public void PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException()
    {
        // Act & Assert: null relativePath is rejected before any path operation
        Assert.Throws<ArgumentNullException>(() =>
            PathHelpers.SafePathCombine("/home/user/project", null!));
    }
}
