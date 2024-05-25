namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestFindPackage
{
    /// <summary>
    /// SPDX file for finding packages
    /// </summary>
    private const string SpdxContents = "{\r\n" +
                                        "  \"files\": [],\r\n" +
                                        "  \"packages\": [" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-1\",\r\n" +
                                        "      \"name\": \"Test Package\",\r\n" +
                                        "      \"versionInfo\": \"1.0.0\",\r\n" +
                                        "      \"packageFileName\": \"package1.zip\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-2\",\r\n" +
                                        "      \"name\": \"Another Test Package\",\r\n" +
                                        "      \"versionInfo\": \"2.0.0\",\r\n" +
                                        "      \"packageFileName\": \"package2.tar\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    }\r\n" +
                                        "  ],\r\n" +
                                        "  \"relationships\": [" +
                                        "    {\r\n" +
                                        "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                        "      \"relatedSpdxElement\": \"SPDXRef-Package-1\",\r\n" +
                                        "      \"relationshipType\": \"DESCRIBES\"\r\n" +
                                        "    }\r\n" +
                                        "  ],\r\n" +
                                        "  \"spdxVersion\": \"SPDX-2.2\",\r\n" +
                                        "  \"dataLicense\": \"CC0-1.0\",\r\n" +
                                        "  \"SPDXID\": \"SPDXRef-DOCUMENT\",\r\n" +
                                        "  \"name\": \"Test Document\",\r\n" +
                                        "  \"documentNamespace\": \"https://sbom.spdx.org\",\r\n" +
                                        "  \"creationInfo\": {\r\n" +
                                        "    \"created\": \"2021-10-01T00:00:00Z\",\r\n" +
                                        "    \"creators\": [ \"Person: Malcolm Nixon\" ]\r\n" +
                                        "  },\r\n" +
                                        "  \"documentDescribes\": [ \"SPDXRef-Package-1\" ]\r\n" +
                                        "}";

    [TestMethod]
    public void FindPackageMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "find-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'find-package' command missing arguments"));
    }

    [TestMethod]
    public void FindPackageMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "find-package",
            "missing.spdx.json",
            "name=anything");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: missing.spdx.json"));
    }

    [TestMethod]
    public void FindPackageCommandLine()
    {
        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "find-package",
                "spdx.json",
                "name=Another Test Package");

            // Verify package ID
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("SPDXRef-Package-2"));
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    [TestMethod]
    public void FindPackageByName()
    {
        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: find-package\n" +
                                        "  inputs:\n" +
                                        "    output: packageId\n" +
                                        "    spdx: spdx.json\n" +
                                        "    name: Test Package\n" +
                                        "" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n"+
                                        "    - Found package ${{ packageId }}";

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify package ID
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("Found package SPDXRef-Package-1"));
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    [TestMethod]
    public void FindPackageByVersion()
    {
        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: find-package\n" +
                                        "  inputs:\n" +
                                        "    output: packageId\n" +
                                        "    spdx: spdx.json\n" +
                                        "    version: 2.0.0\n" +
                                        "" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - Found package ${{ packageId }}";

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify package ID
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("Found package SPDXRef-Package-2"));
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    [TestMethod]
    public void FindPackageByFileName()
    {
        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: find-package\n" +
                                        "  inputs:\n" +
                                        "    output: packageId\n" +
                                        "    spdx: spdx.json\n" +
                                        "    filename: package1.zip\n" +
                                        "" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - Found package ${{ packageId }}";

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify package ID
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("Found package SPDXRef-Package-1"));
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    [TestMethod]
    public void FindPackageByDownload()
    {
        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: find-package\n" +
                                        "  inputs:\n" +
                                        "    output: packageId\n" +
                                        "    spdx: spdx.json\n" +
                                        "    download: https://github.com/demaconsulting/SpdxModel\n" +
                                        "" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - Found package ${{ packageId }}";

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify package ID
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("Found package SPDXRef-Package-2"));
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}