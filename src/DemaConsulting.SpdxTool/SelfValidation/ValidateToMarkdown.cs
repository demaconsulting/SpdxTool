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
///     Self-validation of ToMarkdown command
/// </summary>
internal static class ValidateToMarkdown
{
    /// <summary>
    ///     Run validation test
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="results">Test results</param>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Perform the validation
        var passed = DoValidate();

        // Report validation result to console
        context.WriteLine($"- SpdxTool_ToMarkdown: {(passed ? "Passed" : "Failed")}");
        
        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_ToMarkdown",
                ClassName = "DemaConsulting.SpdxTool.SelfValidation.ValidateToMarkdown",
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
            File.WriteAllText("validate.tmp/test-markdown.spdx.json",
                """
                {
                  "files": [],
                  "packages": [    {
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
                  "relationships": [    {
                      "spdxElementId": "SPDXRef-DOCUMENT",
                      "relatedSpdxElement": "SPDXRef-Application",
                      "relationshipType": "DESCRIBES"
                    },
                    {
                      "spdxElementId": "SPDXRef-Application",
                      "relatedSpdxElement": "SPDXRef-Library",
                      "relationshipType": "CONTAINS"
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

            // Run the to-markdown command to generate markdown summary
            var exitCode = Validate.RunSpdxTool(
                "validate.tmp",
                [
                    "--silent",
                    "to-markdown",
                    "test-markdown.spdx.json",
                    "test-markdown.md",
                    "Test SBOM Summary"
                ]);

            // Fail if SpdxTool reported an error
            if (exitCode != 0)
            {
                return false;
            }

            // Verify the markdown file was created
            if (!File.Exists("validate.tmp/test-markdown.md"))
            {
                return false;
            }

            // Read the generated markdown content
            var markdown = File.ReadAllText("validate.tmp/test-markdown.md");

            // Verify markdown contains expected structure and package information
            return markdown.Contains("Test SBOM Summary") &&
                   markdown.Contains("Root Packages") &&
                   markdown.Contains("Packages") &&
                   markdown.Contains("Test Application") &&
                   markdown.Contains("1.0.0") &&
                   markdown.Contains("Test Library") &&
                   markdown.Contains("2.0.0");
        }
        finally
        {
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }
}
