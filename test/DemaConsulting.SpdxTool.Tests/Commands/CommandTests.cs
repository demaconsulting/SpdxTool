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

using DemaConsulting.SpdxTool.Commands;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the <see cref="Command" /> class.
/// </summary>
public class CommandTests
{
    /// <summary>
    ///     Test that Command.Expand with missing variable throws InvalidOperationException
    /// </summary>
    [Fact]
    public void Command_Expand_MissingVariable_ThrowsInvalidOperationException()
    {
        // Arrange: prepare text with an undefined variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string>();
        // Act/Assert: expanding the undefined variable throws
        Assert.Throws<InvalidOperationException>(() => Command.Expand(text, variables));
    }

    /// <summary>
    ///     Test that Command.Expand with no variables returns the original string
    /// </summary>
    [Fact]
    public void Command_Expand_NoVariables_ReturnsOriginal()
    {
        // Arrange/Act: Expand text with no variable references
        const string text = "Hello, world!";
        var variables = new Dictionary<string, string>();
        var result = Command.Expand(text, variables);

        // Assert:
        Assert.Equal(text, result);
    }

    /// <summary>
    ///     Test that Command.Expand with basic variable returns expanded string
    /// </summary>
    [Fact]
    public void Command_Expand_BasicVariable_ReturnsExpanded()
    {
        // Arrange:
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string> { { "name", "world" } };

        // Act:
        var result = Command.Expand(text, variables);

        // Assert:
        Assert.Equal("Hello, world!", result);
    }

    /// <summary>
    ///     Test that Command.Expand with nested variable returns fully expanded string
    /// </summary>
    [Fact]
    public void Command_Expand_NestedVariable_ReturnsFullyExpanded()
    {
        // Arrange:
        const string text = "Hello, ${{ variable_${{ test }} }}!";
        var variables = new Dictionary<string, string> { { "variable_foo", "world" }, { "test", "foo" } };

        // Act:
        var result = Command.Expand(text, variables);

        // Assert:
        Assert.Equal("Hello, world!", result);
    }

    /// <summary>
    ///     Test that Command.GetMapString with missing entry returns null
    /// </summary>
    [Fact]
    public void Command_GetMapString_MissingEntry_ReturnsNull()
    {
        // Arrange:
        var map = new YamlMappingNode();
        var variables = new Dictionary<string, string>();

        // Act/Assert:
        Assert.Null(Command.GetMapString(map, "parameter", variables));
    }

    /// <summary>
    ///     Test that Command.GetMapString with variable expansion returns expanded value
    /// </summary>
    [Fact]
    public void Command_GetMapString_WithVariableExpansion_ReturnsExpanded()
    {
        // Arrange:
        var map = new YamlMappingNode { { "parameter", "Hello, ${{ name }}!" } };
        var variables = new Dictionary<string, string> { { "name", "world" } };

        // Act/Assert:
        Assert.Equal("Hello, world!", Command.GetMapString(map, "parameter", variables));
    }

    /// <summary>
    ///     Test that Command.Expand with environment variable returns environment value
    /// </summary>
    [Fact]
    public void Command_Expand_EnvironmentVariable_ReturnsEnvironmentValue()
    {
        // Arrange: Set an environment variable
        const string varName = "SPDXTOOL_TEST_VAR";
        const string varValue = "test-env-value";
        Environment.SetEnvironmentVariable(varName, varValue);

        try
        {
            // Act: Expand a template referencing the environment variable
            const string text = "Value: ${{ environment.SPDXTOOL_TEST_VAR }}";
            var variables = new Dictionary<string, string>();
            var result = Command.Expand(text, variables);

            // Assert: Verify environment variable was expanded
            Assert.Equal("Value: test-env-value", result);
        }
        finally
        {
            // Cleanup: Remove the test environment variable
            Environment.SetEnvironmentVariable(varName, null);
        }
    }

    /// <summary>
    ///     Test that CommandsRegistry.Commands all expected command names are present in the registry
    /// </summary>
    [Fact]
    public void CommandsRegistry_Commands_AllExpectedNames_ArePresent()
    {
        // Arrange: Expected command names (from CommandsRegistry)
        var expectedCommands = new[]
        {
            "help", "add-package", "add-relationship", "copy-package", "diagram",
            "find-package", "get-version", "hash", "print", "query", "rename-id",
            "run-workflow", "set-variable", "to-markdown", "update-package", "validate"
        };

        // Act: Get the actual registry
        var commands = CommandsRegistry.Commands;

        // Assert: All expected commands are present
        foreach (var name in expectedCommands)
        {
            Assert.True(commands.ContainsKey(name), $"Expected command '{name}' not found in registry");
        }
    }

    /// <summary>
    ///     Test that CommandsRegistry.Commands with a known command name resolves to a valid entry
    /// </summary>
    [Fact]
    public void CommandsRegistry_Commands_KnownCommandName_ResolvesEntry()
    {
        // Arrange:
        const string commandName = "validate";

        // Act:
        var found = CommandsRegistry.Commands.TryGetValue(commandName, out var entry);

        // Assert:
        Assert.True(found);
        Assert.NotNull(entry);
        Assert.NotNull(entry.Instance);
    }

    /// <summary>
    ///     Test that Command.Expand with unmatched '}}' throws InvalidOperationException
    /// </summary>
    [Fact]
    public void Command_Expand_UnmatchedClose_ThrowsInvalidOperationException()
    {
        // Arrange:
        const string text = "Hello, }} world!";
        var variables = new Dictionary<string, string>();

        // Act/Assert:
        var ex = Assert.Throws<InvalidOperationException>(() => Command.Expand(text, variables));
        Assert.Contains("Unmatched '}}'", ex.Message);
    }

    /// <summary>
    ///     Test that Command.Expand with unmatched '${{' throws InvalidOperationException
    /// </summary>
    [Fact]
    public void Command_Expand_UnmatchedOpen_ThrowsInvalidOperationException()
    {
        // Arrange:
        const string text = "Hello, ${{ world!";
        var variables = new Dictionary<string, string>();

        // Act/Assert:
        var ex = Assert.Throws<InvalidOperationException>(() => Command.Expand(text, variables));
        Assert.Contains("Unmatched '${{", ex.Message);
    }

    /// <summary>
    ///     Test that Command.Expand with an empty variable name throws InvalidOperationException
    /// </summary>
    [Fact]
    public void Command_Expand_EmptyVariableName_ThrowsInvalidOperationException()
    {
        // Arrange:
        const string text = "Hello, ${{  }} world!";
        var variables = new Dictionary<string, string>();

        // Act/Assert:
        var ex = Assert.Throws<InvalidOperationException>(() => Command.Expand(text, variables));
        Assert.Contains("Empty variable name", ex.Message);
    }

    /// <summary>
    ///     Test that Command.GetMapMap with null map returns null
    /// </summary>
    [Fact]
    public void Command_GetMapMap_NullMap_ReturnsNull()
    {
        // Arrange: no setup required — null map is passed directly
        // Act/Assert:
        Assert.Null(Command.GetMapMap(null, "any-key"));
    }

    /// <summary>
    ///     Test that Command.GetMapSequence with null map returns null
    /// </summary>
    [Fact]
    public void Command_GetMapSequence_NullMap_ReturnsNull()
    {
        // Arrange: no setup required — null map is passed directly
        // Act/Assert:
        Assert.Null(Command.GetMapSequence(null, "any-key"));
    }

    /// <summary>
    ///     Test that Command.GetSequenceString with null sequence returns null
    /// </summary>
    [Fact]
    public void Command_GetSequenceString_NullSequence_ReturnsNull()
    {
        // Arrange:
        var variables = new Dictionary<string, string>();

        // Act/Assert:
        Assert.Null(Command.GetSequenceString(null, 0, variables));
    }
}
