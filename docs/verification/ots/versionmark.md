## VersionMark

### Verification Approach

DemaConsulting.VersionMark is verified through two integration tests (installation verification) in
`test/OtsSoftwareTests/OtsSoftwareTests.cs`. The test `VersionMark_Tool_IsInstalled` invokes the
`versionmark` dotnet tool with the `--help` flag and confirms that the tool is installed and
responds correctly. The test `VersionMark_Tool_ReportsVersion` invokes the tool with the
`--version` flag and confirms that it reports its own version string, demonstrating that dotnet
tool version querying is functional.

Full end-to-end verification of the versions markdown report — including the capture of all
pipeline tool versions — occurs as part of each release pipeline run, where the presence of the
generated versions document in the release artifact bundle provides observable evidence of correct
operation.

No vendor test results or third-party compliance reports are required; the integration tests and
pipeline artifact evidence described above provide sufficient verification.
