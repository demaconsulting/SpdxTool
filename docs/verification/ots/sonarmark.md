## SonarMark

### Verification Approach

DemaConsulting.SonarMark is verified through an integration test (installation verification) in
`test/OtsSoftwareTests/OtsSoftwareTests.cs`. The test `SonarMark_Tool_IsInstalled`
invokes the `sonarmark` dotnet tool with the `--help` flag and confirms that the tool is
installed, responds with exit code zero, and produces non-empty output. This confirms that
SonarMark is present and functional in the pipeline environment.

Full end-to-end verification — including SonarCloud API queries, quality-gate retrieval, and
markdown report generation — requires SonarCloud credentials and occurs as part of each release
pipeline run. The presence of the generated SonarCloud quality report in the release artifact
bundle provides observable evidence of correct operation. Tests requiring live SonarCloud
credentials (`SonarMark_QualityGateRetrieval`, `SonarMark_IssuesRetrieval`,
`SonarMark_HotSpotsRetrieval`) are skipped in the local test environment.

No vendor test results or third-party compliance reports are required; the integration test and
pipeline artifact evidence described above provide sufficient verification.
