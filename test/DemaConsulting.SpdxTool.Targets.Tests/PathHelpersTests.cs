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

namespace DemaConsulting.SpdxTool.Targets.Tests;

/// <summary>
///     Unit tests for the PathHelpers class.
/// </summary>
[TestClass]
public class PathHelpersTests
{
    /// <summary>
    ///     Test that SafePathCombine successfully combines valid paths.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_ValidPaths_CombinesSuccessfully()
    {
        // Arrange
        var basePath = "/home/user";
        var relativePath = "documents/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual($"/home/user{Path.DirectorySeparatorChar}documents/file.txt", result);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path with parent directory traversal.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_PathWithParentDirectory_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user";
        var relativePath = "../etc/passwd";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
        Assert.AreEqual("relativePath", exception.ParamName);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path with double dots in middle.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_PathWithDoubleDots_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user";
        var relativePath = "documents/../../../etc/passwd";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for absolute paths.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException()
    {
        // Test Unix absolute path
        var unixBasePath = "/home/user";
        var unixRelativePath = "/etc/passwd";
        var unixException = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(unixBasePath, unixRelativePath));
        Assert.Contains("Invalid path component", unixException.Message);

        // Test Windows absolute path (only on Windows since Windows paths may not be rooted on Unix)
        if (OperatingSystem.IsWindows())
        {
            var windowsBasePath = "C:\\Users\\User";
            var windowsRelativePath = "C:\\Windows\\System32";
            var windowsException = Assert.Throws<ArgumentException>(() =>
                PathHelpers.SafePathCombine(windowsBasePath, windowsRelativePath));
            Assert.Contains("Invalid path component", windowsException.Message);
        }
    }

    /// <summary>
    ///     Test that SafePathCombine accepts simple filename.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_SimpleFilename_CombinesSuccessfully()
    {
        // Arrange
        var basePath = "/home/user/documents";
        var relativePath = "file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual($"/home/user/documents{Path.DirectorySeparatorChar}file.txt", result);
    }

    /// <summary>
    ///     Test that SafePathCombine accepts path with subdirectories.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_PathWithSubdirectories_CombinesSuccessfully()
    {
        // Arrange
        var basePath = "/home/user";
        var relativePath = "documents/work/report.pdf";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual($"/home/user{Path.DirectorySeparatorChar}documents/work/report.pdf", result);
    }

    /// <summary>
    ///     Test that SafePathCombine accepts GUID-based filename.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_GuidBasedFilename_CombinesSuccessfully()
    {
        // Arrange
        var basePath = "/tmp/test-dir";
        var relativePath = "test-a1b2c3d4.tmp";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual($"/tmp/test-dir{Path.DirectorySeparatorChar}test-a1b2c3d4.tmp", result);
    }
}
