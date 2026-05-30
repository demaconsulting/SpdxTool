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

using DemaConsulting.SpdxTool;
using DemaConsulting.SpdxTool.Commands;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'set-variable' command.
/// </summary>
[Collection("CommandSequential")]
public class SetVariableTests
{
    /// <summary>
    ///     Test that set-variable command on command line reports workflow-only error
    /// </summary>
    [Fact]
    public void SetVariable_Run_OnCommandLine_ReportsWorkflowOnlyError()
    {
        // Arrange: create a minimal execution context
        using var context = Context.Create([]);

        // Act / Assert: CLI invocation always throws because the command is workflow-only
        Assert.Throws<CommandUsageException>(() => SetVariable.Instance.Run(context, []));
    }

    /// <summary>
    ///     Test that set-variable command in workflow sets the variable
    /// </summary>
    [Fact]
    public void SetVariable_Run_InWorkflow_SetsVariable()
    {
        // Arrange: context, pre-populated variable map, and a YAML step node
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string> { ["p1"] = "Hello", ["p2"] = "World" };
        var inputs = new YamlMappingNode();
        inputs.Add("value", "${{ p1 }} and ${{ p2 }}");
        inputs.Add("output", "p1p2");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act: run the command directly
        SetVariable.Instance.Run(context, step, variables);

        // Assert: the expanded value is stored under the output key
        Assert.Equal("Hello and World", variables["p1p2"]);
    }

    /// <summary>
    ///     Test that set-variable command throws when the value input is missing
    /// </summary>
    [Fact]
    public void SetVariable_Run_MissingValue_ThrowsException()
    {
        // Arrange: step node with output but no value
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string>();
        var inputs = new YamlMappingNode();
        inputs.Add("output", "my-var");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act / Assert: absent value input must produce a YamlException
        Assert.Throws<YamlException>(() => SetVariable.Instance.Run(context, step, variables));
    }

    /// <summary>
    ///     Test that set-variable command throws when the output input is missing
    /// </summary>
    [Fact]
    public void SetVariable_Run_MissingOutput_ThrowsException()
    {
        // Arrange: step node with value but no output
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string>();
        var inputs = new YamlMappingNode();
        inputs.Add("value", "hello");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act / Assert: absent output input must produce a YamlException
        Assert.Throws<YamlException>(() => SetVariable.Instance.Run(context, step, variables));
    }

    /// <summary>
    ///     Test that set-variable command stores the output key as the literal YAML string
    ///     without applying workflow variable expansion to it
    /// </summary>
    [Fact]
    public void SetVariable_Run_OutputWithVariableSyntax_StoredLiterally()
    {
        // Arrange: context, variable map with some_var defined, and a step whose output key
        //          contains ${{ }} syntax that would resolve if expansion were applied
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string> { ["some_var"] = "expanded_key" };
        var inputs = new YamlMappingNode();
        inputs.Add("output", "${{ some_var }}");
        inputs.Add("value", "test_value");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act: run the command directly
        SetVariable.Instance.Run(context, step, variables);

        // Assert: the literal key "${{ some_var }}" was used, not the expanded "expanded_key"
        Assert.Equal("test_value", variables["${{ some_var }}"]);
    }
}
