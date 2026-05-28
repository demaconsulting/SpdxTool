# Self-Validation

## Overview

Self-validation produces a report demonstrating that SpdxTool is functioning correctly. This is useful
in regulated industries where tool validation evidence is required.

## Running Validation

To perform self-validation:

```bash
dotnet spdx-tool --validate
```

To save the validation report to a log file:

```bash
dotnet spdx-tool --log validation.log --validate
```

To generate a TRX test results file:

```bash
dotnet spdx-tool --validate --result validation.trx
```

To generate a JUnit XML test results file:

```bash
dotnet spdx-tool --validate --result validation.xml
```

To control the depth of the validation report:

```bash
dotnet spdx-tool --validate --depth 2
```

The output format is automatically selected based on the file extension: `.trx` produces a Visual Studio
TRX file and `.xml` produces a JUnit XML file.

## Validation Report

The validation report contains:

- SpdxTool version
- Machine name
- Operating system version
- .NET runtime version
- Timestamp
- Test results for each validation test

Example validation report:

```text
# DemaConsulting.SpdxTool

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| SpdxTool Version    | 2.6.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Microsoft Windows NT 10.0.19045.0                  |
| DotNet Runtime      | .NET 8.0.0                                         |
| Time Stamp          | 2024-01-15 10:30:00Z                               |

✓ SpdxTool_AddPackage - Passed
✓ SpdxTool_AddRelationship - Passed
✓ SpdxTool_Basic - Passed
✓ SpdxTool_CopyPackage - Passed
✓ SpdxTool_Diagram - Passed
✓ SpdxTool_FindPackage - Passed
✓ SpdxTool_GetVersion - Passed
✓ SpdxTool_Hash - Passed
✓ SpdxTool_Ntia - Passed
✓ SpdxTool_Query - Passed
✓ SpdxTool_RenameId - Passed
✓ SpdxTool_RunNuGetWorkflow - Passed
✓ SpdxTool_ToMarkdown - Passed
✓ SpdxTool_UpdatePackage - Passed

Total Tests: 14
Passed: 14
Failed: 0

Validation Passed
```

## Validation Tests

Each test exercises a specific SpdxTool command end-to-end and verifies the expected result:

- **SpdxTool_AddPackage** — Creates an SPDX document with one package, then runs the `add-package`
  command via a workflow to add a second package with a `BUILD_TOOL_OF` relationship. Verifies that
  the resulting document contains both packages and the expected relationship.

- **SpdxTool_AddRelationship** — Creates an SPDX document with two packages, then runs the
  `add-relationship` command to add a `CONTAINS` relationship with a comment between them. Verifies
  that the resulting document contains the relationship with the correct type and comment.

- **SpdxTool_Basic** — Creates a valid SPDX document and verifies that the `validate` command accepts
  it (exit code 0). Then creates an invalid SPDX document (package missing its SPDXID) and verifies
  that the `validate` command rejects it with an appropriate error message.

- **SpdxTool_CopyPackage** — Creates two SPDX documents each with one package, then runs the
  `copy-package` command to copy a package from one document to the other with a `CONTAINED_BY`
  relationship. Verifies that the target document contains both packages and the expected relationship.

- **SpdxTool_Diagram** — Creates an SPDX document with two packages connected by a `DEPENDS_ON`
  relationship, then runs the `diagram` command to generate a Mermaid diagram. Verifies that the
  output file contains the expected `erDiagram` syntax, package names, and relationship type.

- **SpdxTool_FindPackage** — Creates an SPDX document with two packages, then runs the `find-package`
  command in a workflow to locate a package by name and print its ID. Verifies that the log output
  contains the expected SPDX ID.

- **SpdxTool_GetVersion** — Creates an SPDX document with two packages, then runs the `get-version`
  command in a workflow to retrieve a package version by its SPDX ID and print it. Verifies that the
  log output contains the expected version string.

- **SpdxTool_Hash** — Creates a file with known content and runs the `hash generate` command to
  produce a SHA-256 hash file. Verifies the hash value is correct. Then runs the `hash verify`
  command with the correct hash (should pass) and with a corrupted hash (should fail).

- **SpdxTool_Ntia** — Creates an SPDX document with a package missing the required supplier field.
  Verifies that the `validate` command without the `ntia` flag passes, but with the `ntia` flag fails
  with an error citing the missing supplier. Then creates an NTIA-compliant document and verifies that
  `validate ntia` passes.

- **SpdxTool_Query** — Runs the `query` command in a workflow to execute `dotnet --version` and
  extract the version number using a regular expression pattern. Verifies that the log output contains
  a version string in the expected format.

- **SpdxTool_RenameId** — Creates an SPDX document with a package identified as `SPDXRef-Package-1`,
  then runs the `rename-id` command to rename it to `SPDXRef-Package-2`. Verifies that the resulting
  document contains the package under its new identifier.

- **SpdxTool_RunNuGetWorkflow** — Runs the `run-workflow` command to execute the
  `GetDotNetVersion.yaml` workflow from the `DemaConsulting.SpdxWorkflows` NuGet package. Verifies
  that the workflow completes successfully (exit code 0).

- **SpdxTool_ToMarkdown** — Creates an SPDX document with two packages in a `CONTAINS` relationship,
  then runs the `to-markdown` command to generate a Markdown summary. Verifies that the output file
  contains the expected title, section headings, and package information.

- **SpdxTool_UpdatePackage** — Creates an SPDX document with one package, then runs the
  `update-package` command to update all fields of the package including name, version, download
  location, supplier, originator, homepage, copyright, summary, description, and license. Verifies
  that all fields in the resulting document match the updated values.

## Validation Failure

On validation failure:

- The tool exits with a non-zero exit code
- The report indicates which validation tests failed
- Error messages provide diagnostic information

This report may be useful in regulated industries requiring evidence of tool validation.
