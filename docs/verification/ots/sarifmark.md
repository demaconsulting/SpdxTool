## SarifMark

### Verification Approach

DemaConsulting.SarifMark is verified through two integration tests (installation verification) in
`test/OtsSoftwareTests/OtsSoftwareTests.cs`. The test `SarifMark_Tool_IsInstalled` invokes the
`sarifmark` dotnet tool with the `--help` flag and confirms that the tool is installed and
responds correctly. The test `SarifMark_Tool_ReportsVersion` invokes the tool with the
`--version` flag and confirms that it reports its own version string, demonstrating that the tool
executable is present and functional.

Full end-to-end verification of SARIF reading and markdown report generation occurs as part of
each release pipeline run that includes a CodeQL analysis step. The presence of the generated
SARIF report in the release artifact bundle provides observable evidence of correct operation.

No vendor test results or third-party compliance reports are required; the integration tests and
pipeline artifact evidence described above provide sufficient verification.
