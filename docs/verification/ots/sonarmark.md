## SonarMark

### Verification Approach

DemaConsulting.SonarMark is verified through its own self-validation test suite, which runs as
part of the `build-docs` CI job. The test `SonarMark_QualityGateRetrieval` exercises SonarMark's
quality-gate retrieval capability, confirming that the tool is installed and functional in the
pipeline environment. The self-validation TRX result file is produced during the CI run and
provides traceable test evidence for the installation requirement. Tests requiring live SonarCloud
credentials (`SonarMark_IssuesRetrieval`, `SonarMark_HotSpotsRetrieval`) are skipped in the local
test environment but run in CI.

Full end-to-end verification — including SonarCloud API queries, quality-gate retrieval, and
markdown report generation — requires SonarCloud credentials and occurs as part of each release
pipeline run. The presence of the generated SonarCloud quality report in the release artifact
bundle provides observable evidence of correct operation.

No vendor test results or third-party compliance reports are required; the self-validation test
and pipeline artifact evidence described above provide sufficient verification.
