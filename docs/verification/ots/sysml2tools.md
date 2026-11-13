## SysML2Tools

### Verification Approach

DemaConsulting.SysML2Tools is verified by a combination of self-validation evidence and CI
pipeline evidence:

- **Self-validation evidence**: `dotnet sysml2tools --validate --results <trx-file>` runs the
  tool's built-in self-test suite as part of the `build-docs` CI job, exercising `lint` and
  `render --format svg` against known model fixtures and writing a TRX file consumed by ReqStream
  (`SysML2Tools_LintSelfTest`, `SysML2Tools_RenderSvgSelfTest`).
- **Pipeline evidence**: `lint.ps1` runs `dotnet sysml2tools lint 'docs/sysml2/**/*.sysml'`
  against the actual SpdxTool model and fails the build on any syntax or reference error. The
  `build-docs` job runs `dotnet sysml2tools render` to produce one SVG file per declared view under
  `docs/design/generated/`, and FileAssert asserts that every rendered view exists and is a
  non-trivial SVG document before Pandoc embeds the diagrams into the Design document.

### Test Scenarios

#### SysML2Tools_LintSelfTest

**Scenario**: SysML2Tools is invoked with `--validate`, which exercises `lint` against a known-good
and a known-bad model fixture as part of its built-in self-test suite.

**Expected**: Exits 0 with no reported syntax or reference errors for the valid fixture, and
correctly reports an error for the invalid fixture.

#### SysML2Tools_RenderSvgSelfTest

**Scenario**: SysML2Tools is invoked with `--validate`, which exercises `render --format svg`
against a known-good model fixture as part of its built-in self-test suite.

**Expected**: Exits 0 and produces a non-empty SVG file for the fixture's declared view.
