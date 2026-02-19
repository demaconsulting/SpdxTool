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
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfValidation;

/// <summary>
///     Self-validation of UpdatePackage
/// </summary>
internal static class ValidateUpdatePackage
{
    /// <summary>
    ///     Run validation test
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="results">Test results</param>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = DoValidate();

        // Report validation result
        context.WriteLine($"- SpdxTool_UpdatePackage: {(passed ? "Passed" : "Failed")}");
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_UpdatePackage",
                ClassName = "DemaConsulting.SpdxTool.SelfValidation.ValidateUpdatePackage",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Do the validation
    /// </summary>
    /// <returns>True on success</returns>
    private static bool DoValidate()
    {
        try
        {
            // Create the temporary validation folder
            Directory.CreateDirectory("validate.tmp");

            // Write test SPDX file
            File.WriteAllText("validate.tmp/test.spdx.json",
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
                """);

            // Write test workflow file
            File.WriteAllText("validate.tmp/workflow.yaml",
                """
                steps:
                - command: update-package
                  inputs:
                    spdx: test.spdx.json
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
                """);

            // Run the workflow file
            var exitCode = Validate.RunSpdxTool(
                "validate.tmp",
                [
                    "--silent",
                    "run-workflow",
                    "workflow.yaml"
                ]);

            // Fail if SpdxTool reported an error
            if (exitCode != 0)
            {
                return false;
            }

            // Read the SPDX document
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("validate.tmp/test.spdx.json"));

            // Verify expected SPDX content
            return doc is
            {
                Packages:
                [
                    {
                        Id: "SPDXRef-Package-1",
                        Name: "New package name",
                        DownloadLocation: "https://new.package.download",
                        Version: "2.0.0",
                        FileName: "new.zip",
                        Supplier: "New Supplier",
                        Originator: "New Originator",
                        HomePage: "https://new.package.org",
                        CopyrightText: "Copyright New Package Maker",
                        Summary: "New Package",
                        Description: "A new package description",
                        ConcludedLicense: "MIT v2",
                        DeclaredLicense: "MIT v2"
                    }
                ]
            };
        }
        finally
        {
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }
}
