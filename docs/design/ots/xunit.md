## xUnit

### Purpose

xUnit is the unit-testing framework used by the project test suite. It discovers and runs test
methods, collects pass/fail results, and writes TRX result files that feed into coverage reporting
and requirements traceability. xUnit was chosen because it is the standard .NET testing framework
used across the DemaConsulting program.

### Features Used

**Test method discovery and execution** — xUnit discovers all methods marked with `[Fact]` or
`[Theory]` in test assemblies and executes them in the xUnit runner.

**xunit.runner.visualstudio adapter** — the Visual Studio test adapter enables `dotnet test` to
invoke xUnit and produce TRX result files via the standard VSTest infrastructure.

**Parameterized tests** — the `[Theory]` and `[InlineData]` attributes are used for parameterized
test scenarios, reducing duplication in boundary-value and error-path tests.

### Integration Pattern

xUnit is referenced as a NuGet package dependency in each test project
(`DemaConsulting.SpdxTool.Tests`, `DemaConsulting.SpdxTool.Targets.Tests`, and
`OtsSoftwareTests`). It is not referenced by any production project and is not included in the
published NuGet packages. No initialization or configuration beyond the project reference is
required; the xUnit runner is invoked automatically by `dotnet test`.
