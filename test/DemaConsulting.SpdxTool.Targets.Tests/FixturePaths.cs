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

namespace DemaConsulting.SpdxTool.Targets.Tests;

/// <summary>
///     Helper for locating test fixture project directories.
/// </summary>
internal static class FixturePaths
{
    /// <summary>
    ///     Find the repository root by walking up from the test assembly location.
    /// </summary>
    /// <returns>Absolute path to the repository root.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the root cannot be found.</exception>
    public static string GetRepoRoot()
    {
        // Walk up from the test output directory to find the repo root
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(PathHelpers.SafePathCombine(dir, "DemaConsulting.SpdxTool.slnx")))
            {
                return dir;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException(
            "Could not find repository root (looking for DemaConsulting.SpdxTool.slnx)");
    }

    /// <summary>
    ///     Get the path to the SingleTfmProject test fixture directory.
    /// </summary>
    /// <returns>Absolute path to the SingleTfmProject fixture.</returns>
    public static string GetSingleTfmProjectPath()
    {
        var testDir = PathHelpers.SafePathCombine(GetRepoRoot(), "test");
        var fixturesDir = PathHelpers.SafePathCombine(testDir, "TestFixtures");
        return PathHelpers.SafePathCombine(fixturesDir, "SingleTfmProject");
    }

    /// <summary>
    ///     Get the path to the MultiTfmProject test fixture directory.
    /// </summary>
    /// <returns>Absolute path to the MultiTfmProject fixture.</returns>
    public static string GetMultiTfmProjectPath()
    {
        var testDir = PathHelpers.SafePathCombine(GetRepoRoot(), "test");
        var fixturesDir = PathHelpers.SafePathCombine(testDir, "TestFixtures");
        return PathHelpers.SafePathCombine(fixturesDir, "MultiTfmProject");
    }
}
