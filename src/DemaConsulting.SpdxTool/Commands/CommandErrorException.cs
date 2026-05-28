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
///     Exception thrown when a command encounters a runtime failure.
/// </summary>
/// <remarks>
///     Distinct from <see cref="CommandUsageException"/>, which signals an incorrect
///     invocation. <see cref="CommandErrorException"/> signals that the invocation was
///     structurally valid but the operation could not be completed — for example, a
///     referenced file was not found, or the SPDX document content is invalid.
///     <c>Program</c> catches this exception, writes the message to standard error, and
///     exits with a non-zero exit code.
/// </remarks>
public class CommandErrorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="CommandErrorException"/> with the specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the runtime failure.</param>
    public CommandErrorException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandErrorException"/> with the specified message
    ///     and a reference to the underlying exception that caused this failure.
    /// </summary>
    /// <param name="message">Human-readable description of the runtime failure.</param>
    /// <param name="innerException">The exception that is the direct cause of this failure.</param>
    public CommandErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
