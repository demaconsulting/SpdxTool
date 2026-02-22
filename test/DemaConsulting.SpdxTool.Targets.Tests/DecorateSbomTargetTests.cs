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

using System.IO.Compression;

namespace DemaConsulting.SpdxTool.Targets.Tests;

/// <summary>
///     Integration tests for DemaConsulting.SpdxTool.Targets MSBuild targets.
/// </summary>
/// <remarks>
///     These tests invoke dotnet pack on fixture projects and verify the
///     DecorateSbomTarget behavior. They require spdx-tool to be available
///     on the PATH (installed globally in CI via dotnet tool install --global).
/// </remarks>
[TestClass]
public class DecorateSbomTargetTests
{
    /// <summary>
    ///     Test that a single-TFM project pack with DecorateSBOM=true decorates the SBOM.
    /// </summary>
    [TestMethod]
    public void SingleTfmProject_DecorateSbomTrue_DecoratesSbom()
    {
        // Arrange
        var fixtureDir = FixturePaths.GetSingleTfmProjectPath();
        CleanBinObj(fixtureDir);

        // Act: pack the project
        var exitCode = DotnetRunner.Run(
            out var output,
            fixtureDir,
            "pack",
            "--configuration", "Release");

        // Assert: pack succeeded and decoration ran
        Assert.AreEqual(0, exitCode, $"dotnet pack failed:\n{output}");
        Assert.Contains("SpdxTool: Decorating SBOM in TestFixtures.SingleTfmProject.1.0.0.nupkg", output);
        Assert.Contains("SpdxTool: SBOM decoration complete", output);

        // Assert: nupkg exists and contains the SBOM manifest
        var nupkgPath = Path.Combine(fixtureDir, "bin", "Release", "TestFixtures.SingleTfmProject.1.0.0.nupkg");
        Assert.IsTrue(File.Exists(nupkgPath), $"NuPkg not found: {nupkgPath}");
        AssertNupkgContainsSbom(nupkgPath);
    }

    /// <summary>
    ///     Test that a multi-TFM project pack with DecorateSBOM=true decorates the SBOM.
    /// </summary>
    [TestMethod]
    public void MultiTfmProject_DecorateSbomTrue_DecoratesSbom()
    {
        // Arrange
        var fixtureDir = FixturePaths.GetMultiTfmProjectPath();
        CleanBinObj(fixtureDir);

        // Act: pack the project
        var exitCode = DotnetRunner.Run(
            out var output,
            fixtureDir,
            "pack",
            "--configuration", "Release");

        // Assert: pack succeeded and decoration ran
        Assert.AreEqual(0, exitCode, $"dotnet pack failed:\n{output}");
        Assert.Contains("SpdxTool: Decorating SBOM in TestFixtures.MultiTfmProject.1.0.0.nupkg", output);
        Assert.Contains("SpdxTool: SBOM decoration complete", output);

        // Assert: nupkg exists and contains the SBOM manifest
        var nupkgPath = Path.Combine(fixtureDir, "bin", "Release", "TestFixtures.MultiTfmProject.1.0.0.nupkg");
        Assert.IsTrue(File.Exists(nupkgPath), $"NuPkg not found: {nupkgPath}");
        AssertNupkgContainsSbom(nupkgPath);
    }

    /// <summary>
    ///     Test that DecorateSBOM=false skips the decoration target.
    /// </summary>
    [TestMethod]
    public void SingleTfmProject_DecorateSbomFalse_SkipsDecoration()
    {
        // Arrange
        var fixtureDir = FixturePaths.GetSingleTfmProjectPath();
        CleanBinObj(fixtureDir);

        // Act: pack with DecorateSBOM=false
        var exitCode = DotnetRunner.Run(
            out var output,
            fixtureDir,
            "pack",
            "--configuration", "Release",
            "/p:DecorateSBOM=false");

        // Assert: pack succeeded but decoration did NOT run
        Assert.AreEqual(0, exitCode, $"dotnet pack failed:\n{output}");
        Assert.IsFalse(output.Contains("SpdxTool: Decorating SBOM"),
            "Decoration should not run when DecorateSBOM=false");
    }

    /// <summary>
    ///     Test that GenerateSBOM=false skips both SBOM generation and decoration.
    /// </summary>
    [TestMethod]
    public void SingleTfmProject_GenerateSbomFalse_SkipsEntirely()
    {
        // Arrange
        var fixtureDir = FixturePaths.GetSingleTfmProjectPath();
        CleanBinObj(fixtureDir);

        // Act: pack with GenerateSBOM=false
        var exitCode = DotnetRunner.Run(
            out var output,
            fixtureDir,
            "pack",
            "--configuration", "Release",
            "/p:GenerateSBOM=false");

        // Assert: pack succeeded but decoration did NOT run
        Assert.AreEqual(0, exitCode, $"dotnet pack failed:\n{output}");
        Assert.IsFalse(output.Contains("SpdxTool: Decorating SBOM"),
            "Decoration should not run when GenerateSBOM=false");
    }

    /// <summary>
    ///     Test that a missing workflow file produces a clear error message.
    /// </summary>
    [TestMethod]
    public void SingleTfmProject_MissingWorkflow_ReportsError()
    {
        // Arrange
        var fixtureDir = FixturePaths.GetSingleTfmProjectPath();
        CleanBinObj(fixtureDir);

        // Act: pack with a non-existent workflow file
        var exitCode = DotnetRunner.Run(
            out var output,
            fixtureDir,
            "pack",
            "--configuration", "Release",
            "/p:SpdxWorkflowFile=nonexistent-workflow.yaml");

        // Assert: pack failed with clear error
        Assert.AreNotEqual(0, exitCode, "dotnet pack should fail with missing workflow file");
        Assert.Contains("SpdxTool workflow file not found", output);
    }

    /// <summary>
    ///     Clean the bin and obj directories for a fixture project to ensure a fresh build.
    /// </summary>
    /// <param name="projectDir">Path to the project directory.</param>
    private static void CleanBinObj(string projectDir)
    {
        var binDir = Path.Combine(projectDir, "bin");
        var objDir = Path.Combine(projectDir, "obj");

        if (Directory.Exists(binDir))
        {
            Directory.Delete(binDir, true);
        }

        if (Directory.Exists(objDir))
        {
            Directory.Delete(objDir, true);
        }
    }

    /// <summary>
    ///     Assert that a nupkg file contains an SBOM manifest.
    /// </summary>
    /// <param name="nupkgPath">Path to the nupkg file.</param>
    private static void AssertNupkgContainsSbom(string nupkgPath)
    {
        using var archive = ZipFile.OpenRead(nupkgPath);
        var sbomEntry = archive.Entries.FirstOrDefault(
            e => e.FullName.Contains("_manifest") &&
                 e.FullName.EndsWith("manifest.spdx.json", StringComparison.OrdinalIgnoreCase));

        Assert.IsNotNull(sbomEntry, "NuPkg should contain _manifest/spdx_2.2/manifest.spdx.json");
    }
}
