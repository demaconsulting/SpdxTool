## ReqStream

### Verification Approach

DemaConsulting.ReqStream is verified through an integration test (installation verification) in
`test/OtsSoftwareTests/OtsSoftwareTests.cs`. The test `ReqStream_Tool_IsInstalled` invokes the
`reqstream` dotnet tool with the `--help` flag and confirms that the tool is installed, responds
with exit code zero, and produces non-empty output. This confirms that ReqStream is present and
functional in the pipeline environment.

Full end-to-end verification — including requirements YAML processing, TRX result parsing, report
generation, and enforcement mode — occurs on every pipeline run. A non-zero exit code from
ReqStream causes the pipeline to fail, so a completed pipeline run with all requirements covered
is direct evidence that enforcement mode worked correctly and all required tests passed.

No vendor test results or third-party compliance reports are required; the integration test and
pipeline enforcement evidence described above provide sufficient verification.
