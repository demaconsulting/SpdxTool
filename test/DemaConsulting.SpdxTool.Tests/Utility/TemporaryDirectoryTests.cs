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
///     Unit tests for the TemporaryDirectory class.
/// </summary>
public class TemporaryDirectoryTests
{
    /// <summary>
    ///     Test that the constructor creates the directory on disk.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Constructor_CreatesDirectory()
    {
        using var tempDirectory = new TemporaryDirectory();

        Assert.True(Directory.Exists(tempDirectory.DirectoryPath));
    }

    /// <summary>
    ///     Test that two instances produce distinct directory paths.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Constructor_CreatesUniqueDirectories()
    {
        using var tempDirectory1 = new TemporaryDirectory();
        using var tempDirectory2 = new TemporaryDirectory();

        Assert.NotEqual(tempDirectory1.DirectoryPath, tempDirectory2.DirectoryPath);
    }

    /// <summary>
    ///     Test that GetFilePath returns a path located under the temporary directory.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory()
    {
        using var tempDirectory = new TemporaryDirectory();

        var filePath = tempDirectory.GetFilePath("output.md");

        Assert.StartsWith(tempDirectory.DirectoryPath, filePath, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("output.md", filePath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Test that GetFilePath with a nested relative path creates intermediate subdirectories.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories()
    {
        using var tempDirectory = new TemporaryDirectory();

        var filePath = tempDirectory.GetFilePath(Path.Combine("sub", "nested", "output.md"));

        Assert.True(Directory.Exists(Path.GetDirectoryName(filePath)));
    }

    /// <summary>
    ///     Test that GetFilePath rejects a path-traversal attempt with ArgumentException.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException()
    {
        using var tempDirectory = new TemporaryDirectory();

        Assert.Throws<ArgumentException>(() => tempDirectory.GetFilePath("../escaped.txt"));
    }

    /// <summary>
    ///     Test that Dispose deletes the temporary directory and its contents.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Dispose_DeletesDirectory()
    {
        string directoryPath;
        using (var tempDirectory = new TemporaryDirectory())
        {
            directoryPath = tempDirectory.DirectoryPath;
            File.WriteAllText(tempDirectory.GetFilePath("file.txt"), "content");
        }

        Assert.False(Directory.Exists(directoryPath));
    }

    /// <summary>
    ///     Test that Dispose is safe to call when the directory has already been deleted.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow()
    {
        var tempDirectory = new TemporaryDirectory();
        Directory.Delete(tempDirectory.DirectoryPath, recursive: true);

        var exception = Record.Exception(() => tempDirectory.Dispose());
        Assert.Null(exception);
    }
}
