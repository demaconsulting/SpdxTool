## VersionMark

### Purpose

DemaConsulting.VersionMark is a CI pipeline tool that captures version metadata for each dotnet
tool used in the pipeline and writes a versions markdown document included in the release
artifacts. It was chosen to provide a transparent audit trail of tool versions used to produce
each release. VersionMark is not a runtime dependency of the local systems.

### Features Used

**Dotnet tool version reading** — VersionMark queries the installed dotnet tool versions by
invoking each tool with its `--version` flag and capturing the output.

**Markdown report generation** — VersionMark renders the captured version strings as a structured
markdown table included in the release artifact archive.

### Integration Pattern

VersionMark is installed as a global dotnet tool in the GitHub Actions workflow environment. It is
invoked as a late pipeline step after all other tools have been used. Tool names are supplied as
command-line arguments; no configuration files are required. The output markdown file is written
to the artifacts staging directory.
