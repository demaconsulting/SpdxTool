## SysML2Tools

### Purpose

DemaConsulting.SysML2Tools is a CI pipeline tool that lints the SysML2 architecture model and
renders its declared views to SVG diagrams consumed by the design documentation. It keeps the
SpdxTool architecture model (`docs/sysml2/model/**/*.sysml`), its shared metadata definitions
(`docs/sysml2/common/**/*.sysml`), and its declared views (`docs/sysml2/views/design-views.sysml`)
consistent with the design documentation: it validates the model for syntax and reference errors,
and renders each declared view to an SVG diagram embedded directly in the corresponding design
document (for example `docs/design/introduction.md` embeds `SoftwareStructureView.svg`). AI agents
also query the model directly via the `sysml2tools-query` skill rather than parsing the rendered
diagrams or hand-maintained prose. SysML2Tools is not a runtime dependency of the local systems.

### Features Used

**Model validation** — `dotnet sysml2tools lint 'docs/sysml2/**/*.sysml'` reports syntax errors
and unresolved references without producing output files.

**View rendering** — `dotnet sysml2tools render --output docs/design/generated --format svg
'docs/sysml2/common/**/*.sysml' 'docs/sysml2/model/**/*.sysml'
'docs/sysml2/views/design-views.sysml'` renders every `view` declared in `design-views.sysml` to
an SVG file named after the view (for example `SoftwareStructureView.svg`).

**Model querying** — the `sysml2tools-query` skill lets AI agents answer structural and
traceability questions about the model without parsing the raw SysML2 source or the rendered
diagrams.

### Integration Pattern

SysML2Tools is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.sysml2tools.tool` and restored with `dotnet tool restore`. It operates
directly on the SysML2 source files under `docs/sysml2/`; no separate configuration file is
required.

It is used in two places in the pipeline:

- **Lint** (`lint.ps1`, quality-checks job and local pre-PR checks): `dotnet sysml2tools lint
  'docs/sysml2/**/*.sysml'` fails the build if the model contains syntax errors or unresolved
  references.
- **Render** (`build.yaml`, build-docs job): `dotnet sysml2tools render` renders each declared view
  to an SVG file in `docs/design/generated/`, immediately before Pandoc compiles the Design
  document. The design Markdown sources embed bare filenames (for example
  `![Software Structure](SoftwareStructureView.svg)`) that Pandoc resolves through the
  `docs/design/generated` resource path and embeds into the compiled document.

SysML2Tools reads only the local SysML2 model files and writes only local SVG output files; it
requires no external service or network access, and it has no transitive NuGet dependencies that
propagate to the distributed packages.
