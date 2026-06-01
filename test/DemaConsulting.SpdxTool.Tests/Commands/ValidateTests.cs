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

using DemaConsulting.SpdxTool.Commands;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'validate' command
/// </summary>
[Collection("CommandSequential")]
public class ValidateTests
{
    /// <summary>
    ///     Test that validate command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void Validate_Run_MissingArguments_ReportsError()
    {
        // Arrange: no file setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "validate");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'validate' command missing arguments", output);
    }

    /// <summary>
    ///     Test that validate command with missing SPDX file reports an error
    /// </summary>
    [Fact]
    public void Validate_Run_MissingSpdxFile_ReportsError()
    {
        // Arrange: no file setup required — the referenced file intentionally does not exist

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "validate",
            "missing.spdx.json");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that validate command with valid SPDX document succeeds
    /// </summary>
    [Fact]
    public void Validate_Run_ValidSpdxDocument_Succeeds()
    {
        const string spdxContents =
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
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "validate",
                "test.spdx.json");

            // Assert: Verify success reported
            Assert.Equal(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate command with valid document with no files analyzed succeeds
    /// </summary>
    [Fact]
    public void Validate_Run_ValidDocumentNoFilesAnalyzed_Succeeds()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "NOASSERTION",
                  "filesAnalyzed": false,
                  "licenseConcluded": "NOASSERTION"
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
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "validate",
                "test.spdx.json");

            // Assert: Verify success (validation checks are lenient)
            Assert.Equal(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate command with NTIA-valid document succeeds
    /// </summary>
    [Fact]
    public void Validate_Run_NtiaValidDocument_Succeeds()
    {
        const string spdxContents =
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
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command with NTIA flag
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "validate",
                "test.spdx.json",
                "ntia");

            // Assert: Verify success reported
            Assert.Equal(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate command with NTIA-invalid document reports NTIA errors
    /// </summary>
    [Fact]
    public void Validate_Run_NtiaInvalidDocument_ReportsNtiaErrors()
    {
        const string spdxContents =
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
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command with NTIA flag
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "validate",
                "test.spdx.json",
                "ntia");

            // Assert: Verify error reported (missing supplier for NTIA)
            Assert.Equal(1, exitCode);
            Assert.Contains("Issues in test.spdx.json", output);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate YAML workflow step with missing spdx input throws a YAML exception
    /// </summary>
    [Fact]
    public void Validate_Run_MissingSpdxInput_ThrowsYamlException()
    {
        const string workflowContents =
            """
            steps:
            - command: validate
              inputs: {}
            """;

        try
        {
            // Arrange: Write workflow with validate step missing spdx input
            File.WriteAllText("validate-workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "validate-workflow.yaml");

            // Assert: Verify error reported for missing spdx input
            Assert.Equal(1, exitCode);
            Assert.Contains("'validate' command missing 'spdx' input", output);
        }
        finally
        {
            File.Delete("validate-workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that validate YAML workflow step with a valid SPDX document succeeds
    /// </summary>
    [Fact]
    public void Validate_Run_ValidYamlWorkflow_Succeeds()
    {
        const string spdxContents =
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
            """;

        const string workflowContents =
            """
            steps:
            - command: validate
              inputs:
                spdx: test.spdx.json
            """;

        try
        {
            // Arrange: Write SPDX file and workflow file
            File.WriteAllText("test.spdx.json", spdxContents);
            File.WriteAllText("validate-workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "validate-workflow.yaml");

            // Assert: Verify success reported
            Assert.Equal(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("validate-workflow.yaml");
        }
    }

    /// <summary>
    ///     Unit test that <see cref="Validate.DoValidate"/> completes without error on a valid SPDX file.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="Validate.DoValidate"/> directly in-process (no external dotnet process) to
    ///     give ReqStream a true unit-level test link for the core validation logic.
    /// </remarks>
    [Fact]
    public void Validate_DoValidate_ValidSpdxFile_ReturnsNoErrors()
    {
        const string spdxContents =
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
            """;

        var spdxFile = Path.Join(Path.GetTempPath(), $"validate-unit-{Guid.NewGuid():N}.spdx.json");
        try
        {
            // Arrange: write a valid SPDX file to a unique temp path
            File.WriteAllText(spdxFile, spdxContents);

            // Act: invoke DoValidate directly in-process with ntia=false
            using var context = Context.Create(["--silent"]);
            Validate.DoValidate(context, spdxFile, ntia: false);

            // Assert: no errors recorded (DoValidate throws on failure; reaching here means success)
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(spdxFile);
        }
    }

    /// <summary>
    ///     Unit test that <see cref="Validate.DoValidate"/> completes without error on an NTIA-compliant SPDX file.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="Validate.DoValidate"/> directly in-process with <c>ntia=true</c> to give
    ///     ReqStream a true unit-level test link for the NTIA minimum-elements validation path.
    /// </remarks>
    [Fact]
    public void Validate_DoValidate_NtiaMinimumValid_ReturnsNoErrors()
    {
        const string spdxContents =
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
            """;

        var spdxFile = Path.Join(Path.GetTempPath(), $"validate-ntia-unit-{Guid.NewGuid():N}.spdx.json");
        try
        {
            // Arrange: write an NTIA-compliant SPDX file (includes required supplier field) to a unique temp path
            File.WriteAllText(spdxFile, spdxContents);

            // Act: invoke DoValidate directly in-process with ntia=true
            using var context = Context.Create(["--silent"]);
            Validate.DoValidate(context, spdxFile, ntia: true);

            // Assert: no errors recorded (DoValidate throws on failure; reaching here means success)
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(spdxFile);
        }
    }

    /// <summary>
    ///     Test that validate YAML workflow step treats ntia input as case-insensitive
    /// </summary>
    [Fact]
    public void Validate_Run_NtiaYamlInputCaseInsensitive_Succeeds()
    {
        const string spdxContents =
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
            """;

        const string workflowContents =
            """
            steps:
            - command: validate
              inputs:
                spdx: test.spdx.json
                ntia: True
            """;

        try
        {
            // Arrange: Write NTIA-compliant SPDX file and workflow with ntia: True (capital T)
            File.WriteAllText("test.spdx.json", spdxContents);
            File.WriteAllText("validate-workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "validate-workflow.yaml");

            // Assert: Verify success reported (ntia: True is treated as true)
            Assert.Equal(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("validate-workflow.yaml");
        }
    }
}
