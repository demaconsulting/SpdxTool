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

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Test for wildcard pattern matching
/// </summary>
[TestClass]
public class WildcardTests
{
    /// <summary>
    ///     Test that exact pattern matching returns true for matching strings
    /// </summary>
    [TestMethod]
    public void Wildcard_ExactMatch_ReturnsTrue()
    {
        Assert.IsTrue(Wildcard.IsMatch("Hello", "Hello"));
        Assert.IsTrue(Wildcard.IsMatch("HELLO", "Hello"));
        Assert.IsTrue(Wildcard.IsMatch("hello.WORLD", "Hello.World"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "42"));
        Assert.IsFalse(Wildcard.IsMatch("Hello_World", "Hello.World"));
        Assert.IsFalse(Wildcard.IsMatch("Hello", "....."));
        Assert.IsFalse(Wildcard.IsMatch("_Test", "Test"));
        Assert.IsFalse(Wildcard.IsMatch("Test_", "Test"));
    }

    /// <summary>
    ///     Test that asterisk pattern matching matches multiple characters
    /// </summary>
    [TestMethod]
    public void Wildcard_AsteriskPattern_MatchesMultipleChars()
    {
        Assert.IsTrue(Wildcard.IsMatch("Test.This.String", "Test.*.String"));
        Assert.IsTrue(Wildcard.IsMatch("Test String", "*Test*"));
        Assert.IsTrue(Wildcard.IsMatch("This is a test", "*Test*"));
        Assert.IsTrue(Wildcard.IsMatch("This tests for a string", "*Test*"));
        Assert.IsTrue(Wildcard.IsMatch("Test", "Test*"));
        Assert.IsTrue(Wildcard.IsMatch("Testing", "Test*"));
        Assert.IsTrue(Wildcard.IsMatch("Test", "*Test"));
        Assert.IsTrue(Wildcard.IsMatch("Some Test", "*Test"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "*i*"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "*s"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "e*"));
    }

    /// <summary>
    ///     Test that question mark pattern matching matches a single character
    /// </summary>
    [TestMethod]
    public void Wildcard_QuestionMarkPattern_MatchesSingleChar()
    {
        Assert.IsTrue(Wildcard.IsMatch("Test", "Te?t"));
        Assert.IsTrue(Wildcard.IsMatch("Test", "????"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "?Test"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "Test?"));
        Assert.IsFalse(Wildcard.IsMatch("Test", "?"));
    }
}
