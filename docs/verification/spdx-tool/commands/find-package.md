### FindPackage

#### Verification Approach

`FindPackage` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/FindPackageTests.cs`. The tests cover command-line
criteria parsing, workflow output-variable behavior, and package matching by id, name, version, file
name, and download URL.

#### Test Environment

The tests use local SPDX JSON and workflow YAML fixtures in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when invalid invocations are rejected and successful invocations return
the SPDX identifier of the uniquely matching package for each supported criterion.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when no search criteria are supplied. This
scenario is tested by `FindPackage_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the input SPDX file does not exist. This
scenario is tested by `FindPackage_Run_MissingFile_ReportsError`.

**CommandLineNameLookup**: the unit successfully finds and prints a package ID when invoked from the
command line with a name criterion. This scenario is tested by
`FindPackage_ByName_OnCommandLine_FindsPackage`.

**WorkflowNameLookup**: the unit stores the matching package identifier in a workflow variable when
searching by name. This scenario is tested by `FindPackage_Run_ByName_FindsPackage`.

**ByVersionLookup**: the unit finds packages by version criterion. This scenario is tested by
`FindPackage_Run_ByVersion_FindsPackage`.

**ByFileNameLookup**: the unit finds a package by filename when the filename criterion is supplied.
This scenario is tested by `FindPackage_Run_ByFileName_FindsPackage`.

**ByDownloadUrlLookup**: the unit finds a package by download URL when the download criterion is
supplied. This scenario is tested by `FindPackage_Run_ByDownloadUrl_FindsPackage`.

**ByIdLookup**: the unit finds a package by its SPDX element identifier when the id criterion is
supplied. This scenario is tested by `FindPackage_Run_ById_FindsPackage`.

**InvalidCriteria**: the unit reports a usage error when a criterion string does not contain an '='
separator. This scenario is tested by `FindPackage_Run_InvalidCriteria_ReportsError`.

**WorkflowMissingOutput**: the unit reports an error when the `output` input is absent from the
workflow step. This scenario is tested by `FindPackage_Run_MissingOutputInput_ReportsError`.

**WorkflowMissingSpdx**: the unit reports an error when the `spdx` input is absent from the
workflow step. This scenario is tested by `FindPackage_Run_MissingSpdxInput_ReportsError`.

**NoPackageFound**: the unit reports an error when no package in the SPDX document matches the
supplied search criteria. This scenario is tested by `FindPackage_Run_NoPackageFound_ReportsError`.

**MultiplePackagesFound**: the unit reports an error when more than one package in the SPDX document
matches the supplied search criteria. This scenario is tested by
`FindPackage_Run_MultiplePackagesFound_ReportsError`.

**ParseCriteriaEmptyKey**: the unit throws a usage exception when a criterion argument has an empty
key (e.g. `=value`). This scenario is tested by
`FindPackage_ParseCriteria_EmptyKey_ThrowsCommandUsageException`.
