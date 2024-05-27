namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestSha256
{
    [TestMethod]
    public void Sha256CommandMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "sha256");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'sha256' command missing arguments"));
    }

    [TestMethod]
    public void Sha256CommandGenerateMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "sha256",
            "generate",
            "missing-file.txt");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Error calculating sha256 hash for 'missing-file.txt'"));
    }

    [TestMethod]
    public void Sha256CommandGenerate()
    {
        try
        {
            File.WriteAllText("test.txt", "The quick brown fox jumps over the lazy dog");

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "sha256",
                "generate",
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
    public void Sha256CommandVerifyMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "sha256",
            "verify",
            "missing-file.txt");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Error: Could not find file"));
    }

    [TestMethod]
    public void Sha256CommandVerifyBad()
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
                "sha256",
                "verify",
                "test.txt");

            // Verify error reported
            Assert.AreEqual(1, exitCode);
            Assert.IsTrue(output.Contains("Sha256 hash mismatch for 'test.txt'"));
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }

    [TestMethod]
    public void Sha256CommandVerifyGood()
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
                "sha256",
                "verify",
                "test.txt");

            // Verify success reported
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("Sha256 Digest OK for 'test.txt'"));
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }
}