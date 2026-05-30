# OTS Verification Evidence

This document describes the overall strategy for verifying that the OTS software items consumed
by the SpdxTool program meet their declared requirements. OTS items are verified through a
combination of integration tests, self-validation exercises, and vendor-supplied evidence rather
than through unit testing of their internal implementation.

## Verification Strategy

**Runtime NuGet package dependencies** (YamlDotNet, DemaConsulting.SpdxModel,
DemaConsulting.NuGet.Caching, and DemaConsulting.TestResults) are verified through integration
tests in `test/DemaConsulting.SpdxTool.Tests/` and through the SelfTest subsystem. Integration
tests exercise the APIs consumed by SpdxTool and confirm that the required functionality works in
the project's runtime environment. The SelfTest subsystem additionally exercises runtime
dependencies end-to-end as part of the `--validate` self-test suite, providing regression
coverage on every pipeline run.

**The test framework** (xUnit) is self-validating: any failure in the xUnit infrastructure would
cause the CI pipeline to fail, making successful test execution its own verification evidence.
Successful completion of the full test suite on each pipeline run constitutes passing verification
for xUnit.

**CI pipeline tools** (DemaConsulting.BuildMark, DemaConsulting.ReqStream,
DemaConsulting.VersionMark, DemaConsulting.SarifMark, and DemaConsulting.SonarMark) are verified
through integration tests in `test/OtsSoftwareTests/` and through their observable pipeline
outputs. Each tool produces a specific artifact — a markdown report, a traceability matrix, or an
enforcement exit code — that is inspected as part of the pipeline run. A successful pipeline run
with the expected artifacts present constitutes passing verification for each CI tool.

Individual verification approaches for each OTS item are documented in the OTS verification
subfolder. OTS requirements are recorded in `docs/reqstream/ots/`. Design documentation is in
`docs/design/ots/`.
