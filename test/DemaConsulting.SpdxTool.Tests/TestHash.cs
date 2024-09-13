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

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestHash
{
    [TestMethod]
    public void HashCommandMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "'hash' command missing arguments");
    }

    [TestMethod]
    public void HashCommandGenerateMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "generate",
            "sha256",
            "missing-file.txt");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "Error: Could not find file 'missing-file.txt'");
    }

    [TestMethod]
    public void HashCommandGenerate()
    {
        try
        {
            File.WriteAllText("test.txt", "The quick brown fox jumps over the lazy dog");

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "generate",
                "sha256",
                "test.txt");

            // Verify success reported
            Assert.AreEqual(0, exitCode);

            // Verify the hash file was created
            Assert.IsTrue(File.Exists("test.txt.sha256"));
            var digest = File.ReadAllText("test.txt.sha256");
            Assert.AreEqual("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592", digest);
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }

    [TestMethod]
    public void HashCommandVerifyMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "verify",
            "sha256",
            "missing-file.txt");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "Error: Could not find file");
    }

    [TestMethod]
    public void HashCommandVerifyBad()
    {
        try
        {
            File.WriteAllText("test.txt", "Test string");
            File.WriteAllText("test.txt.sha256", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                "test.txt");

            // Verify error reported
            Assert.AreEqual(1, exitCode);
            StringAssert.Contains(output, "Sha256 hash mismatch for 'test.txt'");
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }

    [TestMethod]
    public void HashCommandVerifyGood()
    {
        try
        {
            File.WriteAllText("test.txt", "The quick brown fox jumps over the lazy dog");
            File.WriteAllText("test.txt.sha256", "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                "test.txt");

            // Verify success reported
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "Sha256 Digest OK for 'test.txt'");
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }
}