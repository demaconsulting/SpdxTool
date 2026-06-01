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
///     Provides glob-style wildcard pattern matching for filtering SPDX package fields
///     by name, version, file name, or download URL.
/// </summary>
/// <remarks>
///     Converts glob-style wildcard patterns (<c>*</c> matches any sequence of characters,
///     <c>?</c> matches any single character) to anchored regular expressions, then evaluates
///     them case-insensitively. Used by commands that filter SPDX packages by name, version,
///     file name, or download URL.
/// </remarks>
internal static class Wildcard
{
    /// <summary>
    ///     Converts a wildcard pattern to an anchored regular expression pattern.
    /// </summary>
    /// <remarks>
    ///     Extracted as a private helper to keep <see cref="IsMatch"/> readable and to isolate the
    ///     conversion logic for independent review. All regex metacharacters in the wildcard string
    ///     are escaped before wildcard tokens are substituted, so literal dots, brackets, and
    ///     parentheses in the pattern match exactly rather than acting as regex operators.
    /// </remarks>
    /// <param name="wildPattern">Wildcard pattern to convert. Must not be null.</param>
    /// <returns>Anchored regular expression string (prefixed with <c>^</c> and suffixed with <c>$</c>).</returns>
    private static string WildcardToRegex(string wildPattern)
    {
        return "^" +
               Regex.Escape(wildPattern).Replace("\\*", ".*").Replace("\\?", ".") +
               "$";
    }

    /// <summary>
    ///     Returns true if <paramref name="input"/> matches the entire wildcard pattern case-insensitively.
    /// </summary>
    /// <remarks>
    ///     The match is performed case-insensitively with a 100 ms timeout to prevent
    ///     catastrophic backtracking on pathological patterns. If the timeout expires,
    ///     <see langword="false"/> is returned rather than propagating a
    ///     <see cref="System.Text.RegularExpressions.RegexMatchTimeoutException"/>.
    ///     Stateless and thread-safe.
    /// </remarks>
    /// <param name="input">Input text to test. Must not be null.</param>
    /// <param name="pattern">Wildcard pattern to match against. Must not be null.</param>
    /// <returns>
    ///     <see langword="true"/> when <paramref name="input"/> matches the entire
    ///     <paramref name="pattern"/> case-insensitively; <see langword="false"/> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="input"/> or <paramref name="pattern"/> is null.
    /// </exception>
    public static bool IsMatch(string input, string pattern)
    {
        // Reject null arguments before regex evaluation — callers must provide non-null values.
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);

        // Evaluate the wildcard pattern with a timeout to prevent catastrophic backtracking.
        try
        {
            return Regex.IsMatch(
                input,
                WildcardToRegex(pattern),
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
