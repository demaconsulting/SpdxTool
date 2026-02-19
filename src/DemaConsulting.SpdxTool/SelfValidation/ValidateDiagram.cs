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

using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfValidation;

/// <summary>
///     Self-validation of Diagram command
/// </summary>
internal static class ValidateDiagram
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
        context.WriteLine($"- SpdxTool_Diagram: {(passed ? "Passed" : "Failed")}");
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_Diagram",
                ClassName = "DemaConsulting.SpdxTool.SelfValidation.ValidateDiagram",
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

            // Write test SPDX file with packages and relationships
            File.WriteAllText("validate.tmp/test-diagram.spdx.json",
                """
                {
                  "files": [],
                  "packages": [
                    {
                      "SPDXID": "SPDXRef-Application",
                      "name": "Test Application",
                      "versionInfo": "1.0.0",
                      "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                      "licenseConcluded": "MIT"
                    },
                    {
                      "SPDXID": "SPDXRef-Library",
                      "name": "Test Library",
                      "versionInfo": "2.0.0",
                      "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                      "licenseConcluded": "Apache-2.0"
                    }
                  ],
                  "relationships": [
                    {
                      "spdxElementId": "SPDXRef-DOCUMENT",
                      "relatedSpdxElement": "SPDXRef-Application",
                      "relationshipType": "DESCRIBES"
                    },
                    {
                      "spdxElementId": "SPDXRef-Application",
                      "relatedSpdxElement": "SPDXRef-Library",
                      "relationshipType": "DEPENDS_ON"
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
                  }
                }
                """);

            // Run the diagram command
            var exitCode = Validate.RunSpdxTool(
                "validate.tmp",
                [
                    "--silent",
                    "diagram",
                    "test-diagram.spdx.json",
                    "test-diagram.mermaid.txt"
                ]);

            // Fail if SpdxTool reported an error
            if (exitCode != 0)
                return false;

            // Verify the mermaid file was created
            if (!File.Exists("validate.tmp/test-diagram.mermaid.txt"))
                return false;

            // Read and verify mermaid content
            var mermaid = File.ReadAllText("validate.tmp/test-diagram.mermaid.txt");

            // Verify mermaid syntax and content
            return mermaid.Contains("erDiagram") &&
                   mermaid.Contains("Test Application / 1.0.0") &&
                   mermaid.Contains("Test Library / 2.0.0") &&
                   mermaid.Contains("DEPENDS_ON");
        }
        finally
        {
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }
}
