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
///     Immutable descriptor for a single registered command.
/// </summary>
/// <remarks>
///     Each <see cref="CommandEntry"/> bundles the metadata that <c>Program</c> needs to
///     display help and dispatch commands, together with the <see cref="Command"/> instance
///     used to execute the command. All entries are created once at static-initialization
///     time by their respective command classes (e.g., <c>Help.Entry</c>) and stored in
///     <see cref="CommandsRegistry"/>.
/// </remarks>
/// <param name="Name">CLI-visible command name (e.g., <c>"validate"</c>, <c>"add-package"</c>).</param>
/// <param name="CommandLine">One-line example showing typical CLI usage.</param>
/// <param name="Summary">Short description used in the main help listing.</param>
/// <param name="Details">Multi-line extended help text shown by the <c>help</c> command.</param>
/// <param name="Instance">Singleton <see cref="Command"/> instance used to execute this command.</param>
public sealed record CommandEntry(string Name, string CommandLine, string Summary, string[] Details, Command Instance);
