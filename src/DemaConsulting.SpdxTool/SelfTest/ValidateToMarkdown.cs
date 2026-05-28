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

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-validation of the ToMarkdown command.
/// </summary>
/// <remarks>
///     Exercises the to-markdown command end-to-end to confirm it is correctly installed and
///     operational. Called by the SelfTest orchestrator as part of the deployment validation
///     sequence.
/// </remarks>
internal static class ValidateToMarkdown
{
    /// <summary>
    ///     Runs the to-markdown self-test and records the outcome in the test results collection.
    /// </summary>
    /// <remarks>
    ///     Delegates to DoValidate for the actual file-system operations, then writes a pass or
    ///     fail message to the context and appends a TestResult entry to results. The test name
    ///     SpdxTool_ToMarkdown is fixed so that ReqStream can trace it to the
    ///     SpdxTool-SelfTest-ToMarkdown requirement.
    /// </remarks>
    /// <param name="context">Active program context used for console output. Must not be null.</param>
    /// <param name="results">Test results collection to append the outcome to. Must not be null.</param>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Perform the validation
        var passed = DoValidate();

        // Report validation result to console
        if (passed)
        {
            context.WriteLine($"✓ SpdxTool_ToMarkdown - Passed");
        }
        else
        {
            context.WriteError($"✗ SpdxTool_ToMarkdown - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_ToMarkdown",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateToMarkdown",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Performs the to-markdown validation in a temporary directory.
    /// </summary>
    /// <remarks>
    ///     Creates a two-package SPDX JSON document (Test Application 1.0.0/MIT and Test Library
    ///     2.0.0/Apache-2.0) with DESCRIBES and CONTAINS relationships, invokes the to-markdown
    ///     command via Validate.RunSpdxTool, then verifies that the output Markdown file contains
    ///     the expected title, section headings, package names, and version strings. The temporary
    ///     directory is always deleted in a finally block regardless of outcome.
    /// </remarks>
    /// <returns>
    ///     True if the to-markdown command exits with code zero and the output Markdown file
    ///     contains all expected strings; false if the exit code is non-zero or any expected
    ///     string is absent.
    /// </returns>
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
