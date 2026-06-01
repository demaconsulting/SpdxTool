## ReqStream

### Verification Approach

DemaConsulting.ReqStream is verified through its own self-validation test suite, which runs as
part of the `build-docs` CI job. The test `ReqStream_RequirementsProcessing` exercises
ReqStream's core requirements processing capability, confirming that the tool is installed and
functional in the pipeline environment. The self-validation TRX result file is produced during
the CI run and provides traceable test evidence for the installation requirement.

Full end-to-end verification — including requirements YAML processing, TRX result parsing, report
generation, and enforcement mode — occurs on every pipeline run. A non-zero exit code from
ReqStream causes the pipeline to fail, so a completed pipeline run with all requirements covered
is direct evidence that enforcement mode worked correctly and all required tests passed.

No vendor test results or third-party compliance reports are required; the self-validation test
and pipeline enforcement evidence described above provide sufficient verification.
