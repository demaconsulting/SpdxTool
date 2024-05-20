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