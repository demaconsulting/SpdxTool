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
///     Self-validation of basic SPDX validation
/// </summary>
internal static class ValidateBasic
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
        context.WriteLine($"- SpdxTool_Validate: {(passed ? "Passed" : "Failed")}");
        
        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_Validate",
                ClassName = "DemaConsulting.SpdxTool.SelfValidation.ValidateBasic",
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

            // Run validation tests for both valid and invalid documents
            return DoValidateValid() && DoValidateInvalid();
        }
        finally
        {
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }

    /// <summary>
    ///     Validate that basic validation passes for valid document
    /// </summary>
    /// <returns>True on success</returns>
    private static bool DoValidateValid()
    {
        // Write test SPDX file that is valid
        File.WriteAllText("validate.tmp/test-valid.spdx.json",
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package",
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
              }
            }
            """);

        // Run validation without NTIA flag on valid document
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "validate",
                "test-valid.spdx.json"
            ]);

        // Validation should pass for valid document
        return exitCode == 0;
    }

    /// <summary>
    ///     Validate that basic validation detects invalid document
    /// </summary>
    /// <returns>True on success</returns>
    private static bool DoValidateInvalid()
    {
        // Write test SPDX file that is invalid (missing required SPDXID)
        File.WriteAllText("validate.tmp/test-invalid.spdx.json",
            """
            {
              "files": [],
              "packages": [    {
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [],
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

        // Run validation on invalid document
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "--log", "output.log",
                "validate",
                "test-invalid.spdx.json"
            ]);

        // Validation should fail for invalid document
        if (exitCode == 0)
            return false;

        // Read the log file to verify error was reported
        var log = File.ReadAllText("validate.tmp/output.log");
        
        // Verify log contains error about missing SPDXID
        return log.Contains("Issues in test-invalid.spdx.json") || log.Contains("Package") || log.Contains("SPDXID");
    }
}
