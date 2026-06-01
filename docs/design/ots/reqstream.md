## ReqStream

### Purpose

DemaConsulting.ReqStream is a CI pipeline tool that enforces requirements traceability by
verifying that every requirement declared in the YAML requirements files is linked to at least one
passing test in the TRX result files. It was chosen because it provides automated compliance
enforcement for IEC 62304 traceability obligations. ReqStream is not a runtime dependency of the
local systems.

### Features Used

**Requirements YAML processing** — ReqStream reads the hierarchical requirements YAML file and
resolves all requirement IDs and their declared test links.

**TRX result parsing** — ReqStream parses the TRX files produced by xUnit and the SelfTest
subsystem to determine which test names passed during the pipeline run.

**Report generation** — ReqStream produces a requirements report, a justifications document, and
a traceability matrix as markdown files included in the release artifacts.

**Enforcement mode** — when invoked with the `--enforce` flag, ReqStream exits with a non-zero
exit code if any requirement is not covered by a passing test, causing the pipeline to fail.

### Integration Pattern

ReqStream is installed as a global dotnet tool in the GitHub Actions workflow environment. It is
invoked after the test stage, receiving the requirements YAML path and one or more TRX file paths
as arguments. The `--enforce` flag is always passed in pipeline runs to ensure that uncovered
requirements are caught as build failures. No configuration files beyond the requirements YAML are
required.
