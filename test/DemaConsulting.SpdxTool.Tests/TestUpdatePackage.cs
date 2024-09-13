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

using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestUpdatePackage
{
    [TestMethod]
    public void UpdatePackageCommandLine()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "update-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "'update-package' command is only valid in a workflow");
    }

    [TestMethod]
    public void UpdatePackageWorkflow()
    {
        // SPDX contents
        const string spdxContents = 
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-1",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-1",
                  "relationshipType": "DESCRIBES"
                }
              ],
              "spdxVersion": "SPDX-2.2",
              "dataLicense": "CC0-1.0",
              "SPDXID": "SPDXRef-DOCUMENT",
              "name": "Test Document",
              "documentNamespace": "https://sbom.spdx.org",
              "creationInfo": {
                "created": "2021-10-01T00:00:00Z",
                "creators": [ "Person: Malcolm Nixon" ]
              },
              "documentDescribes": [ "SPDXRef-Package-1" ]
            }
            """;

        // Workflow contents
        const string workflowContents = 
            """
            steps:
            - command: update-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  name: New package name
                  download: https://new.package.download
                  version: 2.0.0
                  filename: new.zip
                  supplier: New Supplier
                  originator: New Originator
                  homepage: https://new.package.org
                  copyright: Copyright New Package Maker
                  summary: New Package
                  description: A new package description
                  license: MIT v2
            """;

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", spdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Verify both packages present
            Assert.AreEqual(1, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("New package name", doc.Packages[0].Name);
            Assert.AreEqual("https://new.package.download", doc.Packages[0].DownloadLocation);
            Assert.AreEqual("2.0.0", doc.Packages[0].Version);
            Assert.AreEqual("new.zip", doc.Packages[0].FileName);
            Assert.AreEqual("New Supplier", doc.Packages[0].Supplier);
            Assert.AreEqual("New Originator", doc.Packages[0].Originator);
            Assert.AreEqual("https://new.package.org", doc.Packages[0].HomePage);
            Assert.AreEqual("Copyright New Package Maker", doc.Packages[0].CopyrightText);
            Assert.AreEqual("New Package", doc.Packages[0].Summary);
            Assert.AreEqual("A new package description", doc.Packages[0].Description);
            Assert.AreEqual("MIT v2", doc.Packages[0].ConcludedLicense);
            Assert.AreEqual("MIT v2", doc.Packages[0].DeclaredLicense);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}