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
}
