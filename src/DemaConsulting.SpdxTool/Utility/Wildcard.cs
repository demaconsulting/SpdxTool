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

using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Utility;

/// <summary>
///     Wildcard Match Class
/// </summary>
/// <remarks>
///     Converts glob-style wildcard patterns (<c>*</c> matches any sequence of characters,
///     <c>?</c> matches any single character) to anchored regular expressions, then evaluates
///     them case-insensitively. Used by commands that filter SPDX packages by name, version,
///     file name, or download URL.
/// </remarks>
public static class Wildcard
{
    /// <summary>
    ///     Convert a wildcard pattern to a regular expression pattern
    /// </summary>
    /// <param name="wildPattern">Wildcard pattern</param>
    /// <returns>Regular expression pattern</returns>
    private static string WildCardToRegex(string wildPattern)
    {
        return "^" +
               Regex.Escape(wildPattern).Replace("\\*", ".*").Replace("\\?", ".") +
               "$";
    }

    /// <summary>
    ///     Check for a wildcard match
    /// </summary>
    /// <param name="input">Input text</param>
    /// <param name="pattern">Wildcard pattern</param>
    /// <returns>True if input text matches wildcard</returns>
    /// <remarks>
    ///     The match is performed case-insensitively with a 100 ms timeout to prevent
    ///     catastrophic backtracking on pathological patterns. If the timeout expires,
    ///     <see langword="false"/> is returned rather than propagating a
    ///     <see cref="System.Text.RegularExpressions.RegexMatchTimeoutException"/>.
    /// </remarks>
    public static bool IsMatch(string input, string pattern)
    {
        try
        {
            return Regex.IsMatch(
                input,
                WildCardToRegex(pattern),
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
