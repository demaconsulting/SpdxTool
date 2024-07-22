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
public class TestGetVersion
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
    public void GetVersionMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "get-version");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'get-version' command missing arguments"));
    }

    [TestMethod]
    public void GetVersionGenerateMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "get-version",
            "missing.spdx.json",
            "id=SPDXRef-Package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: missing.spdx.json"));
    }

    [TestMethod]
    public void GetVersionCommandLine()
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
                "get-version",
                "spdx.json",
                "id=SPDXRef-Package-2");

            // Verify package ID
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("2.0.0"));
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    [TestMethod]
    public void GetVersionWorkflow()
    {
        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: get-version\n" +
                                        "  inputs:\n" +
                                        "    spdx: spdx.json\n" +
                                        "    id: SPDXRef-Package-2\n" +
                                        "    output: version\n" +
                                        "" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - Found version ${{ version }}";

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
            Assert.IsTrue(output.Contains("Found version 2.0.0"));
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}