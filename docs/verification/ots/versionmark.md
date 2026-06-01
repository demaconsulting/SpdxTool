## VersionMark

### Verification Approach

DemaConsulting.VersionMark is verified through its own self-validation test suite, which runs as
part of the `build-docs` CI job. The test `VersionMark_CapturesVersions` exercises VersionMark's
core version-capture capability, confirming that the tool is installed and functional in the
pipeline environment. The self-validation TRX result file is produced during the CI run and
provides traceable test evidence for the installation requirement.

Full end-to-end verification of the versions markdown report — including the capture of all
pipeline tool versions — occurs as part of each release pipeline run, where the presence of the
generated versions document in the release artifact bundle provides observable evidence of correct
operation.

No vendor test results or third-party compliance reports are required; the self-validation test
and pipeline artifact evidence described above provide sufficient verification.
