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

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Unit tests for the Context class.
/// </summary>
public class ContextTests
{
    /// <summary>
    ///     Test that Context.Create with no --depth flag defaults the depth to 1.
    /// </summary>
    [Fact]
    public void Context_Create_NoDepthFlag_DefaultsDepthToOne()
    {
        // Arrange: N/A — no fixture required

        // Act: create a context with no flags
        using var context = Context.Create([]);

        // Assert: default depth is 1
        Assert.Equal(1, context.Depth);
    }

    /// <summary>
    ///     Test that Context.Create with --silent flag sets Silent to true.
    /// </summary>
    [Fact]
    public void Context_Create_SilentFlag_SetsSilentToTrue()
    {
        // Arrange: N/A — no fixture required

        // Act: create a context with the --silent flag
        using var context = Context.Create(["--silent"]);

        // Assert: Silent property reflects the flag
        Assert.True(context.Silent);
    }

    /// <summary>
    ///     Test that Context.WriteError increments the error count and sets exit code to 1.
    /// </summary>
    [Fact]
    public void Context_WriteError_SingleCall_IncrementsErrorCount()
    {
        // Arrange: create a context with silent output to suppress console noise
        using var context = Context.Create(["--silent"]);

        // Act: report a single error
        context.WriteError("test error");

        // Assert: error count incremented and exit code is 1
        Assert.Equal(1, context.Errors);
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Test that Context.Create with a negative depth value throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Context_Create_NegativeDepth_ThrowsInvalidOperationException()
    {
        // Arrange: N/A — no fixture required

        // Act / Assert: negative depth must be rejected with a controlled error, not a crash
        var ex = Assert.Throws<InvalidOperationException>(
            () => Context.Create(["--depth", "-1"]));

        // Assert: error message is user-friendly and mentions depth
        Assert.Contains("depth", ex.Message);
    }

    /// <summary>
    ///     Test that Context.Create with an invalid log file path throws InvalidOperationException
    /// </summary>
    [Fact]
    public void Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException()
    {
        // Arrange: use an empty string as an invalid path (triggers ArgumentException in StreamWriter)
        // Act/Assert: creating context with invalid log path throws InvalidOperationException
        Assert.Throws<InvalidOperationException>(
            () => Context.Create(["-l", ""]));
    }

    /// <summary>
    ///     Test that Context.WriteLine with a log file writes the line to the file.
    /// </summary>
    [Fact]
    public void Context_WriteLine_WithLogFile_WritesLineToFile()
    {
        // Arrange: create a temporary file path for the log
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".log");

        try
        {
            // Act: create a context with a log file, write a line, then dispose
            using (var context = Context.Create(["--log", tempFilePath, "some-command"]))
            {
                context.WriteLine("test output");
            }

            // Assert: the log file contains the written line
            var contents = File.ReadAllText(tempFilePath);
            Assert.Contains("test output", contents);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    /// <summary>
    ///     Test that Context.Create with --log flag but no following filename throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Context_Create_WithLogFlagMissingFilename_ThrowsInvalidOperationException()
    {
        // Arrange: N/A — no fixture required

        // Act / Assert: --log without a filename argument must be rejected
        var ex = Assert.Throws<InvalidOperationException>(
            () => Context.Create(["--log"]));

        // Assert: error message is user-friendly
        Assert.NotNull(ex.Message);
    }

    /// <summary>
    ///     Test that Context.Create with a non-integer depth value throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Context_Create_WithNonIntegerDepth_ThrowsInvalidOperationException()
    {
        // Arrange: N/A — no fixture required

        // Act / Assert: non-integer depth must be rejected with a controlled error
        var ex = Assert.Throws<InvalidOperationException>(
            () => Context.Create(["--depth", "abc"]));

        // Assert: error message mentions the invalid value
        Assert.Contains("abc", ex.Message);
    }

    /// <summary>
    ///     Test that Context.WriteWarning does not increment the error counter.
    /// </summary>
    [Fact]
    public void Context_WriteWarning_DoesNotIncrementErrors()
    {
        // Arrange: create a context with silent output to suppress console noise
        using var context = Context.Create(["--silent"]);

        // Act: write a warning (not an error)
        context.WriteWarning("test warning");

        // Assert: error count remains zero and exit code is 0
        Assert.Equal(0, context.Errors);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that positional arguments after global flags are exposed via Context.Arguments.
    /// </summary>
    [Fact]
    public void Context_Create_PositionalArgsAfterFlags_ExposedAsArguments()
    {
        // Arrange: N/A — no fixture required

        // Act: create a context where a global flag precedes positional arguments
        using var context = Context.Create(["--silent", "run-workflow", "workflow.yaml"]);

        // Assert: Arguments contains exactly the positional tokens that follow the flag
        Assert.Equal(["run-workflow", "workflow.yaml"], context.Arguments);
    }

    /// <summary>
    ///     Test that Context.Create with --depth 3 sets the Depth property to 3.
    /// </summary>
    [Fact]
    public void Context_Create_WithDepthFlag_SetsDepth()
    {
        // Arrange: N/A — no fixture required

        // Act: create a context with --depth 3
        using var context = Context.Create(["--depth", "3"]);

        // Assert: Depth property reflects the flag value
        Assert.Equal(3, context.Depth);
    }
}
