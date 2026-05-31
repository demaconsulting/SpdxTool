## xUnit

### Verification Approach

xUnit is self-validating: any defect in test discovery, execution, or result reporting would
cause the CI pipeline to fail or produce incorrect TRX output, which would in turn cause
ReqStream enforcement to fail. The successful completion of all tests across
`DemaConsulting.SpdxTool.Tests`, `DemaConsulting.SpdxTool.Targets.Tests`, and
`OtsSoftwareTests` on every pipeline run constitutes passing verification for xUnit.

Three representative tests from `test/DemaConsulting.SpdxTool.Tests/VersionTests.cs` and
`test/DemaConsulting.SpdxTool.Tests/UsageTests.cs` are nominated as explicit verification
evidence: `SpdxTool_Version_ShortFlag_DisplaysVersion`,
`SpdxTool_Version_LongFlag_DisplaysVersion`, and `SpdxTool_Usage_ShortHelpFlag_DisplaysUsage`.
Passage of these tests confirms that xUnit can discover, execute, and report results for the
project's test methods.

No vendor test results or third-party compliance reports are required; the self-validating nature
of the test infrastructure provides sufficient evidence.

### Test Scenarios

**Execution**: xUnit discovers and executes `[Fact]` test methods in the project's test
assemblies. This scenario is demonstrated by the passage of
`SpdxTool_Version_ShortFlag_DisplaysVersion` and `SpdxTool_Version_LongFlag_DisplaysVersion`.

**Reporting**: xUnit produces TRX result files that are subsequently consumed by ReqStream for
requirements traceability. This scenario is verified indirectly: a successful pipeline run with
all requirements covered confirms that TRX output was generated and parsed correctly. Representative
tests `SpdxTool_Version_ShortFlag_DisplaysVersion` and `SpdxTool_Usage_ShortHelpFlag_DisplaysUsage`
appear in the TRX output on every run.
