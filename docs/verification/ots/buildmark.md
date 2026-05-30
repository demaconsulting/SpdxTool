## BuildMark

### Verification Approach

DemaConsulting.BuildMark is verified through an integration test (installation verification) in
`test/OtsSoftwareTests/OtsSoftwareTests.cs`. The test `BuildMark_Tool_IsInstalled`
invokes the `buildmark` dotnet tool with the `--help` flag and confirms that the tool is
installed, responds with exit code zero, and produces non-empty output. This confirms that
BuildMark is present and functional in the pipeline environment.

Full end-to-end verification of markdown report generation — including GitHub API queries and
artifact output — occurs as part of each release pipeline run, where the presence of the
generated build-notes document in the release artifact bundle provides observable evidence of
correct operation.

No vendor test results or third-party compliance reports are required; the integration test and
pipeline artifact evidence described above provide sufficient verification.
