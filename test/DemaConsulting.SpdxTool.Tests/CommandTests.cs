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
    ///     Test the <see cref="Command.Expand" /> method with a missing variable.
    /// </summary>
    [TestMethod]
    public void Command_Expand_MissingVariable()
    {
        // Test expanding a missing variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string>();
        Assert.ThrowsExactly<InvalidOperationException>(() => Command.Expand(text, variables));
    }

    /// <summary>
    ///     Test the <see cref="Command.Expand" /> method with nothing to expand.
    /// </summary>
    [TestMethod]
    public void Command_Expand_NoVariables()
    {
        // Test expanding nothing
        const string text = "Hello, world!";
        var variables = new Dictionary<string, string>();
        var result = Command.Expand(text, variables);
        Assert.AreEqual(text, result);
    }

    /// <summary>
    ///     Test the <see cref="Command.Expand" /> method for basic expansion.
    /// </summary>
    [TestMethod]
    public void Command_Expand_BasicVariable()
    {
        // Test expanding a basic variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string> { { "name", "world" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    /// <summary>
    ///     Test the <see cref="Command.Expand" /> method for nested expansion.
    /// </summary>
    [TestMethod]
    public void Command_Expand_NestedVariableExpansion()
    {
        // Test expanding a nested variable
        const string text = "Hello, ${{ variable_${{ test }} }}!";
        var variables = new Dictionary<string, string> { { "variable_foo", "world" }, { "test", "foo" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    /// <summary>
    ///     Test the <see cref="Command.GetMapString" /> method for a missing entry.
    /// </summary>
    [TestMethod]
    public void Command_GetMapString_MissingEntry()
    {
        // Test getting a missing parameter
        var map = new YamlMappingNode();
        var variables = new Dictionary<string, string>();
        Assert.IsNull(Command.GetMapString(map, "parameter", variables));
    }

    /// <summary>
    ///     Test the <see cref="Command.GetMapString" /> method with a value requiring expansion.
    /// </summary>
    [TestMethod]
    public void Command_GetMapString_WithExpansion()
    {
        // Test getting a parameter
        var map = new YamlMappingNode { { "parameter", "Hello, ${{ name }}!" } };
        var variables = new Dictionary<string, string> { { "name", "world" } };
        Assert.AreEqual("Hello, world!", Command.GetMapString(map, "parameter", variables));
    }
}