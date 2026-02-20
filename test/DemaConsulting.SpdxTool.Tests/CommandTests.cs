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

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the <see cref="Command" /> class.
/// </summary>
[TestClass]
public class CommandTests
{
    /// <summary>
    ///     Test that Command.Expand with missing variable returns missing token
    /// </summary>
    [TestMethod]
    public void Command_Expand_MissingVariable_ReturnsMissingToken()
    {
        // Test expanding a missing variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string>();
        Assert.ThrowsExactly<InvalidOperationException>(() => Command.Expand(text, variables));
    }

    /// <summary>
    ///     Test that Command.Expand with no variables returns the original string
    /// </summary>
    [TestMethod]
    public void Command_Expand_NoVariables_ReturnsOriginal()
    {
        // Test expanding nothing
        const string text = "Hello, world!";
        var variables = new Dictionary<string, string>();
        var result = Command.Expand(text, variables);
        Assert.AreEqual(text, result);
    }

    /// <summary>
    ///     Test that Command.Expand with basic variable returns expanded string
    /// </summary>
    [TestMethod]
    public void Command_Expand_BasicVariable_ReturnsExpanded()
    {
        // Test expanding a basic variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string> { { "name", "world" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    /// <summary>
    ///     Test that Command.Expand with nested variable returns fully expanded string
    /// </summary>
    [TestMethod]
    public void Command_Expand_NestedVariable_ReturnsFullyExpanded()
    {
        // Test expanding a nested variable
        const string text = "Hello, ${{ variable_${{ test }} }}!";
        var variables = new Dictionary<string, string> { { "variable_foo", "world" }, { "test", "foo" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    /// <summary>
    ///     Test that Command.GetMapString with missing entry throws exception
    /// </summary>
    [TestMethod]
    public void Command_GetMapString_MissingEntry_ThrowsException()
    {
        // Test getting a missing parameter
        var map = new YamlMappingNode();
        var variables = new Dictionary<string, string>();
        Assert.IsNull(Command.GetMapString(map, "parameter", variables));
    }

    /// <summary>
    ///     Test that Command.GetMapString with variable expansion returns expanded value
    /// </summary>
    [TestMethod]
    public void Command_GetMapString_WithVariableExpansion_ReturnsExpanded()
    {
        // Test getting a parameter
        var map = new YamlMappingNode { { "parameter", "Hello, ${{ name }}!" } };
        var variables = new Dictionary<string, string> { { "name", "world" } };
        Assert.AreEqual("Hello, world!", Command.GetMapString(map, "parameter", variables));
    }
}
