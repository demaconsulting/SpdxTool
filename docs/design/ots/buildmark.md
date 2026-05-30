## BuildMark

### Purpose

DemaConsulting.BuildMark is a CI pipeline tool that generates a build-notes markdown document
from GitHub Actions workflow run metadata. It was chosen to provide a consistent, auditable record
of each release build included in the release artifacts bundle. BuildMark is not a runtime
dependency of the local systems.

### Features Used

**GitHub Actions API query** — BuildMark queries the GitHub REST API to retrieve the workflow run
identifier, trigger event, branch, commit SHA, and timestamp for the current pipeline execution.

**Markdown report generation** — BuildMark renders the retrieved metadata as a structured markdown
document suitable for inclusion in the release artifact archive.

### Integration Pattern

BuildMark is installed as a global dotnet tool in the GitHub Actions workflow environment. It is
invoked as a pipeline step after the build and test stages complete. It reads GitHub Actions
environment variables and the `GITHUB_TOKEN` secret to authenticate API calls, and writes the
resulting markdown file to the artifacts staging directory. No configuration files are required;
all inputs are supplied as command-line arguments to the tool.
