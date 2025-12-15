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
///     Self-validation of NTIA validation
/// </summary>
internal static class ValidateNtia
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
        context.WriteLine($"- SpdxTool_Ntia: {(passed ? "Passed" : "Failed")}");
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_Ntia",
                ClassName = "DemaConsulting.SpdxTool.SelfValidation.ValidateNtia",
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

            // Run individual validation tests
            return DoValidateMissingSupplier() && DoValidateCompliant();
        }
        finally
        {
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }

    /// <summary>
    ///     Validate that NTIA validation detects missing supplier
    /// </summary>
    /// <returns>True on success</returns>
    private static bool DoValidateMissingSupplier()
    {
        // Write test SPDX file that is valid but not NTIA compliant
        // Missing: supplier field for the package
        File.WriteAllText("validate.tmp/test-ntia.spdx.json",
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
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

        // Run validation without NTIA flag - should succeed
        var exitCode1 = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "validate",
                "test-ntia.spdx.json"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode1 != 0)
            return false;

        // Run validation with NTIA flag - should fail due to missing supplier
        var exitCode2 = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--log", "output.log",
                "validate",
                "test-ntia.spdx.json",
                "ntia"
            ]);

        // Should fail validation
        if (exitCode2 == 0)
            return false;

        // Read the log file and verify it contains the expected error
        var log = File.ReadAllText("validate.tmp/output.log");
        if (!log.Contains("NTIA: Package 'Test Package' Missing Supplier"))
            return false;

        return true;
    }

    /// <summary>
    ///     Validate that NTIA validation passes for compliant document
    /// </summary>
    /// <returns>True on success</returns>
    private static bool DoValidateCompliant()
    {
        // Write test SPDX file that is NTIA compliant
        File.WriteAllText("validate.tmp/test-ntia-valid.spdx.json",
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "supplier": "Organization: Test",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
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

        // Run validation with NTIA flag on valid document - should succeed
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "validate",
                "test-ntia-valid.spdx.json",
                "ntia"
            ]);

        // Should pass validation
        return exitCode == 0;
    }
}
