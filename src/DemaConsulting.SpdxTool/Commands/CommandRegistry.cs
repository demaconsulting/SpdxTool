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

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Commands Registry
/// </summary>
public static class CommandsRegistry
{
    /// <summary>
    /// Dictionary of known commands
    /// </summary>
    private static readonly Dictionary<string, CommandEntry> InternalCommands = new()
    {
        { Help.Entry.Name, Help.Entry },
        { AddPackage.Entry.Name, AddPackage.Entry },
        { AddRelationship.Entry.Name, AddRelationship.Entry },
        { CopyPackage.Entry.Name, CopyPackage.Entry },
        { Diagram.Entry.Name, Diagram.Entry },
        { FindPackage.Entry.Name, FindPackage.Entry },
        { GetVersion.Entry.Name, GetVersion.Entry },
        { Hash.Entry.Name, Hash.Entry },
        { Print.Entry.Name, Print.Entry },
        { Query.Entry.Name, Query.Entry },
        { RenameId.Entry.Name, RenameId.Entry },
        { RunWorkflow.Entry.Name, RunWorkflow.Entry },
        { SetVariable.Entry.Name, SetVariable.Entry },
        { ToMarkdown.Entry.Name, ToMarkdown.Entry },
        { UpdatePackage.Entry.Name, UpdatePackage.Entry },
        { Validate.Entry.Name, Validate.Entry }
    };

    /// <summary>
    /// Gets the commands
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Commands => InternalCommands;
}