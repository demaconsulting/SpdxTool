## SarifMark

### Purpose

DemaConsulting.SarifMark is a CI pipeline tool that reads the SARIF output produced by CodeQL
code scanning and renders it as a human-readable markdown document included in the release
artifacts. It was chosen to surface static analysis findings in the release bundle without
requiring reviewers to parse raw SARIF JSON. SarifMark is not a runtime dependency of the local
systems.

### Features Used

**SARIF file reading** — SarifMark parses the SARIF 2.1 JSON file produced by the CodeQL GitHub
Actions step to extract rule violations and their source locations.

**Markdown report generation** — SarifMark renders the extracted findings as a structured markdown
document, grouping results by rule and severity. When no violations are found, it writes a
document stating that no issues were detected.

### Integration Pattern

SarifMark is installed as a global dotnet tool in the GitHub Actions workflow environment. It is
invoked after the CodeQL analysis step, receiving the SARIF file path and the desired output
markdown path as command-line arguments. No configuration files are required.
