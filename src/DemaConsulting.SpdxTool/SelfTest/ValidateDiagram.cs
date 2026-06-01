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
///     Self-test step that exercises the <c>diagram</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that a Mermaid entity-relationship diagram can be generated from an SPDX document
///     and that the output file is created with the expected diagram syntax and package content.
///     Uses a temporary <c>validate.tmp</c> directory in the current working directory; callers
///     must ensure sequential execution to avoid races on that directory and on the process-wide
///     current directory set by <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateDiagram
{
    /// <summary>
    ///     Executes the diagram self-test and records the result.
    /// </summary>
    /// <param name="context">
    ///     The active Program context providing output and error streams. Must not be null.
    /// </param>
    /// <param name="results">
    ///     The TestResults collection to append the step outcome to. Must not be null.
    /// </param>
    /// <remarks>
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. If <see cref="DoValidate"/> throws an exception,
    ///     the exception propagates uncaught from this method and no <see cref="TestResult"/> is
    ///     recorded for this step.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir(Validate.TempDir, DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_Diagram", "DemaConsulting.SpdxTool.SelfTest.ValidateDiagram", passed);
    }

    /// <summary>
    ///     Performs the actual diagram validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the command succeeded and the output Mermaid file contains the expected
    ///     diagram syntax and package content; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Writes an SPDX JSON document containing two packages (Test Application and Test Library)
    ///     connected by a DEPENDS_ON relationship. Invokes
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> with the <c>diagram</c> command,
    ///     then verifies that the output file exists and contains the <c>erDiagram</c> keyword,
    ///     both package names and versions, and the DEPENDS_ON relationship label.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the Mermaid file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = Validate.TempDir;

        // Write test SPDX file with packages and relationships
        File.WriteAllText($"{tempDir}/test-diagram.spdx.json",
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

        // Run the diagram command to generate Mermaid diagram
        var exitCode = Validate.RunSpdxTool(
            tempDir,
            [
                "--silent",
                "diagram",
                "test-diagram.spdx.json",
                "test-diagram.mermaid.txt"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode != 0)
        {
            return false;
        }

        // Verify the Mermaid file was created
        if (!File.Exists($"{tempDir}/test-diagram.mermaid.txt"))
        {
            return false;
        }

        // Read the generated Mermaid content
        var mermaid = File.ReadAllText($"{tempDir}/test-diagram.mermaid.txt");

        // Verify Mermaid contains expected diagram syntax and content
        return mermaid.Contains("erDiagram") &&
               mermaid.Contains("Test Application / 1.0.0") &&
               mermaid.Contains("Test Library / 2.0.0") &&
               mermaid.Contains("DEPENDS_ON");
    }
}
