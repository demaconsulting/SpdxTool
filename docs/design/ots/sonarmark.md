## SonarMark

### Purpose

DemaConsulting.SonarMark is a CI pipeline tool that retrieves quality-gate and metrics data from
SonarCloud and renders it as a markdown document included in the release artifacts. It was chosen
to provide a traceable quality snapshot for each release without requiring reviewers to access the
SonarCloud web interface. SonarMark is not a runtime dependency of the local systems.

### Features Used

**SonarCloud API query** — SonarMark calls the SonarCloud REST API to retrieve the quality-gate
status, coverage percentage, code duplications, and other metric values for the project.

**Markdown report generation** — SonarMark renders the retrieved metrics and quality-gate verdict
as a structured markdown document included in the release artifact archive.

### Integration Pattern

SonarMark is installed as a global dotnet tool in the GitHub Actions workflow environment. It is
invoked after the SonarCloud analysis step, receiving the project key, organization, and a
SonarCloud authentication token via command-line arguments. The output markdown file is written to
the artifacts staging directory.
