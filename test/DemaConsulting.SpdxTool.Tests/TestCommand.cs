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

[TestClass]
public class TestCommand
{
    [TestMethod]
    public void CommandExpandMissing()
    {
        // Test expanding a missing variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string>();
        Assert.ThrowsException<InvalidOperationException>(() => Command.Expand(text, variables));
    }

    [TestMethod]
    public void CommandExpandNothing()
    {
        // Test expanding nothing
        const string text = "Hello, world!";
        var variables = new Dictionary<string, string>();
        var result = Command.Expand(text, variables);
        Assert.AreEqual(text, result);
    }

    [TestMethod]
    public void CommandExpandBasic()
    {
        // Test expanding a basic variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string> { { "name", "world" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    [TestMethod]
    public void CommandExpandDouble()
    {
        // Test expanding a nested variable
        const string text = "Hello, ${{ name }}!";
        var variables = new Dictionary<string, string> { { "name", "${{ target }}" }, { "target", "world" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    [TestMethod]
    public void CommandExpandNested()
    {
        // Test expanding a nested variable
        const string text = "Hello, ${{ variable_${{ test }} }}!";
        var variables = new Dictionary<string, string> { { "variable_foo", "world" }, { "test", "foo" } };
        var result = Command.Expand(text, variables);
        Assert.AreEqual("Hello, world!", result);
    }

    [TestMethod]
    public void CommandGetMapStringMissing()
    {
        // Test getting a missing parameter
        var map = new YamlMappingNode();
        var variables = new Dictionary<string, string>();
        Assert.IsNull(Command.GetMapString(map, "parameter", variables));
    }

    [TestMethod]
    public void CommandGetMapString()
    {
        // Test getting a parameter
        var map = new YamlMappingNode { { "parameter", "Hello, ${{ name }}!" } };
        var variables = new Dictionary<string, string> { { "name", "world" } };
        Assert.AreEqual("Hello, world!", Command.GetMapString(map, "parameter", variables));
    }
}