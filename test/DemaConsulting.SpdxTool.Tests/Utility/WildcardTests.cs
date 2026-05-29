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

using DemaConsulting.SpdxTool.Utility;

namespace DemaConsulting.SpdxTool.Tests.Utility;

/// <summary>
///     Test for wildcard pattern matching
/// </summary>
/// <remarks>
///     Unit tests for <see cref="Wildcard"/>. Each test exercises a distinct matching scenario
///     for <see cref="Wildcard.IsMatch"/>: exact matching, asterisk wildcards, question mark
///     wildcards, null argument handling, and empty-string boundary conditions.
/// </remarks>
public class WildcardTests
{
    /// <summary>
    ///     Test that exact pattern matching returns true for matching strings
    /// </summary>
    /// <remarks>
    ///     Verifies that IsMatch returns true for case-insensitive exact matches and false when
    ///     the input differs in content, length, or separator from the pattern.
    /// </remarks>
    [Fact]
    public void Wildcard_IsMatch_ExactMatch_ReturnsTrue()
    {
        // Act / Assert: verify exact matching behavior across multiple inputs
        Assert.Multiple(
            () => Assert.True(Wildcard.IsMatch("Hello", "Hello")),
            () => Assert.True(Wildcard.IsMatch("HELLO", "Hello")),
            () => Assert.True(Wildcard.IsMatch("hello.WORLD", "Hello.World")),
            () => Assert.False(Wildcard.IsMatch("Test", "42")),
            () => Assert.False(Wildcard.IsMatch("Hello_World", "Hello.World")),
            () => Assert.False(Wildcard.IsMatch("Hello", ".....")),
            () => Assert.False(Wildcard.IsMatch("_Test", "Test")),
            () => Assert.False(Wildcard.IsMatch("Test_", "Test")));
    }

    /// <summary>
    ///     Test that asterisk pattern matching matches multiple characters
    /// </summary>
    /// <remarks>
    ///     Verifies that the <c>*</c> wildcard matches zero or more characters anywhere in the
    ///     pattern, and that non-matching positions are correctly rejected.
    /// </remarks>
    [Fact]
    public void Wildcard_IsMatch_AsteriskPattern_MatchesMultipleChars()
    {
        // Act / Assert: verify asterisk wildcard matching behavior across multiple inputs
        Assert.Multiple(
            () => Assert.True(Wildcard.IsMatch("Test.This.String", "Test.*.String")),
            () => Assert.True(Wildcard.IsMatch("Test String", "*Test*")),
            () => Assert.True(Wildcard.IsMatch("This is a test", "*Test*")),
            () => Assert.True(Wildcard.IsMatch("This tests for a string", "*Test*")),
            () => Assert.True(Wildcard.IsMatch("Test", "Test*")),
            () => Assert.True(Wildcard.IsMatch("Testing", "Test*")),
            () => Assert.True(Wildcard.IsMatch("Test", "*Test")),
            () => Assert.True(Wildcard.IsMatch("Some Test", "*Test")),
            () => Assert.False(Wildcard.IsMatch("Test", "*i*")),
            () => Assert.False(Wildcard.IsMatch("Test", "*s")),
            () => Assert.False(Wildcard.IsMatch("Test", "e*")));
    }

    /// <summary>
    ///     Test that question mark pattern matching matches a single character
    /// </summary>
    /// <remarks>
    ///     Verifies that the <c>?</c> wildcard matches exactly one character, and that inputs
    ///     with too few or too many characters are rejected.
    /// </remarks>
    [Fact]
    public void Wildcard_IsMatch_QuestionMarkPattern_MatchesSingleChar()
    {
        // Act / Assert: verify question mark wildcard matching behavior across multiple inputs
        Assert.Multiple(
            () => Assert.True(Wildcard.IsMatch("Test", "Te?t")),
            () => Assert.True(Wildcard.IsMatch("Test", "????")),
            () => Assert.False(Wildcard.IsMatch("Test", "?Test")),
            () => Assert.False(Wildcard.IsMatch("Test", "Test?")),
            () => Assert.False(Wildcard.IsMatch("Test", "?")));
    }

    /// <summary>
    ///     Test that IsMatch throws ArgumentNullException when input is null.
    /// </summary>
    /// <remarks>
    ///     Verifies the documented null-argument contract: passing null for the input parameter
    ///     must throw ArgumentNullException.
    /// </remarks>
    [Fact]
    public void Wildcard_IsMatch_NullInput_ThrowsArgumentNullException()
    {
        // Act / Assert: null input argument causes ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => Wildcard.IsMatch(null!, "pattern"));
    }

    /// <summary>
    ///     Test that IsMatch throws ArgumentNullException when pattern is null.
    /// </summary>
    /// <remarks>
    ///     Verifies the documented null-argument contract: passing null for the pattern parameter
    ///     must throw ArgumentNullException.
    /// </remarks>
    [Fact]
    public void Wildcard_IsMatch_NullPattern_ThrowsArgumentNullException()
    {
        // Act / Assert: null pattern argument causes ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => Wildcard.IsMatch("input", null!));
    }

    /// <summary>
    ///     Test that IsMatch handles empty strings and patterns correctly.
    /// </summary>
    /// <remarks>
    ///     Verifies boundary conditions: an empty input matches an empty pattern, a non-empty
    ///     input does not match an empty pattern, and an asterisk pattern matches the empty string.
    /// </remarks>
    [Fact]
    public void Wildcard_IsMatch_EmptyInputs_BehavesCorrectly()
    {
        // Act / Assert: verify empty string and empty pattern boundary behavior
        Assert.Multiple(
            () => Assert.True(Wildcard.IsMatch("", "")),
            () => Assert.False(Wildcard.IsMatch("Test", "")),
            () => Assert.True(Wildcard.IsMatch("", "*")));
    }
}
