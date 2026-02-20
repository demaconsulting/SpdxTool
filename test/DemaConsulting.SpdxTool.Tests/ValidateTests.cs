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

/// <summary>
///     Tests for the 'validate' command
/// </summary>
[TestClass]
public class ValidateTests
{
    /// <summary>
    ///     Test that validate command with missing arguments reports an error
    /// </summary>
    [TestMethod]
    public void Validate_MissingArguments_ReportsError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "validate");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'validate' command missing arguments", output);
    }

    /// <summary>
    ///     Test that validate command with missing SPDX file reports an error
    /// </summary>
    [TestMethod]
    public void Validate_MissingSpdxFile_ReportsError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "validate",
            "missing.spdx.json");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that validate command with valid SPDX document succeeds
    /// </summary>
    [TestMethod]
    public void Validate_ValidSpdxDocument_Succeeds()
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
            Assert.AreEqual(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate command with valid document with no files analyzed succeeds
    /// </summary>
    [TestMethod]
    public void Validate_ValidDocumentNoFilesAnalyzed_Succeeds()
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
            Assert.AreEqual(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate command with NTIA-valid document succeeds
    /// </summary>
    [TestMethod]
    public void Validate_NtiaValidDocument_Succeeds()
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
            Assert.AreEqual(0, exitCode);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that validate command with NTIA-invalid document reports NTIA errors
    /// </summary>
    [TestMethod]
    public void Validate_NtiaInvalidDocument_ReportsNtiaErrors()
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
            Assert.AreEqual(1, exitCode);
            Assert.Contains("Issues in test.spdx.json", output);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }
}
