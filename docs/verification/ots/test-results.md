## TestResults

### Verification Approach

DemaConsulting.TestResults is verified through integration tests in
`test/DemaConsulting.SpdxTool.Tests/SelfTest/SelfTestTests.cs`. These tests invoke the SelfTest
subsystem with TRX and JUnit output paths specified and confirm that the result files are created
and contain the expected content. The tests
`SelfTest_Validate_WithTrxResult_GeneratesTrxFile` and
`SelfTest_Validate_WithJUnitResult_GeneratesJUnitFile` cover the two serialization formats
required by this OTS item.

These integration tests confirm that `TrxSerializer` and `JUnitSerializer` correctly serialize
validation results in the formats expected by CI/CD test reporting systems and by the ReqStream
requirements traceability tool.

No vendor test results or third-party compliance reports are required; the integration tests
described above provide sufficient evidence.

### Test Scenarios

**TRX Serialization**: The SelfTest subsystem serializes validation results in TRX format using
`TrxSerializer`. The result file is created at the specified path and contains the expected test
result entries. This scenario is tested by `SelfTest_Validate_WithTrxResult_GeneratesTrxFile`.

**JUnit Serialization**: The SelfTest subsystem serializes validation results in JUnit XML format
using `JUnitSerializer`. The result file is created at the specified path and contains the expected
test result entries in JUnit schema. This scenario is tested by
`SelfTest_Validate_WithJUnitResult_GeneratesJUnitFile`.
