## SarifMark

### Verification Approach

DemaConsulting.SarifMark is verified through its own self-validation test suite, which runs as
part of the `build-docs` CI job. The test `SarifMark_SarifReading` exercises SarifMark's core
SARIF parsing capability, confirming that the tool is installed and functional in the pipeline
environment. The self-validation TRX result file is produced during the CI run and provides
traceable test evidence for the installation requirement.

Full end-to-end verification of SARIF reading and markdown report generation occurs as part of
each release pipeline run that includes a CodeQL analysis step. The presence of the generated
SARIF report in the release artifact bundle provides observable evidence of correct operation.

No vendor test results or third-party compliance reports are required; the self-validation test
and pipeline artifact evidence described above provide sufficient verification.
