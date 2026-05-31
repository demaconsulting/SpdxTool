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

namespace DemaConsulting.SpdxTool.Utility;

/// <summary>
///     Helper utilities for safe path operations.
/// </summary>
/// <remarks>
///     This class exists to provide a single, auditable point of path-safety enforcement for any command
///     that accepts user-supplied file paths. Centralizing the check prevents directory-traversal
///     vulnerabilities from being re-implemented (or forgotten) in each caller.
///     All members are stateless and thread-safe.
/// </remarks>
internal static class PathHelpers
{
    /// <summary>
    ///     Safely combines a base path with one or more caller-supplied relative path segments,
    ///     rejecting any input that could escape the base directory.
    /// </summary>
    /// <remarks>
    ///     Each segment in <paramref name="relativePaths"/> is validated and appended to the
    ///     running combined path in order. Two validation layers are applied per segment:
    ///     1. An upfront string check rejects segments that contain ".." components or are already
    ///        rooted — these are the most common forms of directory-traversal attack.
    ///     2. A defense-in-depth check resolves both paths with <see cref="Path.GetFullPath(string)"/>
    ///        and uses <see cref="Path.GetRelativePath"/> to confirm the combined result stays under
    ///        the base. This guards against edge cases (e.g. platform-specific path normalization)
    ///        that could bypass the string check.
    ///     This method is stateless and thread-safe.
    /// </remarks>
    /// <param name="basePath">
    ///     The base directory path. Must not be null. Any valid directory path is accepted; it need
    ///     not exist on disk because only string and normalized-path operations are performed.
    /// </param>
    /// <param name="relativePaths">
    ///     One or more caller-supplied relative path segments to append in order. Must not be null.
    ///     Each individual segment must not be null, must not contain ".." components, and must not
    ///     be an absolute (rooted) path.
    /// </param>
    /// <returns>
    ///     The result of combining <paramref name="basePath"/> with each segment in
    ///     <paramref name="relativePaths"/> in order. The returned path is always within
    ///     <paramref name="basePath"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="basePath"/>, <paramref name="relativePaths"/>, or any
    ///     individual segment within <paramref name="relativePaths"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when any segment contains ".." components, is an absolute path, or resolves
    ///     outside the current combined path after normalization.
    /// </exception>
    internal static string SafePathCombine(string basePath, params string[] relativePaths)
    {
        // Validate that basePath and the segments array are not null
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(relativePaths);

        // Apply validation and combination for each segment in order
        var current = basePath;
        foreach (var relativePath in relativePaths)
        {
            // Validate the individual segment is not null
            ArgumentNullException.ThrowIfNull(relativePath);

            // Ensure the segment does not contain path traversal sequences
            if (relativePath.Contains("..") || Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException($"Invalid path component: {relativePath}", nameof(relativePaths));
            }

            // This call to Path.Combine is safe because we have validated that:
            // 1. relativePath does not contain ".." (path traversal)
            // 2. relativePath is not an absolute path (IsPathRooted check)
            var combinedPath = Path.Combine(current, relativePath);

            // Defense-in-depth: ensure the combined path is still under the base path
            var fullBasePath = Path.GetFullPath(basePath);
            var fullCombinedPath = Path.GetFullPath(combinedPath);
            var relativeCheck = Path.GetRelativePath(fullBasePath, fullCombinedPath);
            if (relativeCheck.StartsWith("..") || Path.IsPathRooted(relativeCheck))
            {
                throw new ArgumentException($"Invalid path component: {relativePath}", nameof(relativePaths));
            }

            current = combinedPath;
        }

        return current;
    }
}
