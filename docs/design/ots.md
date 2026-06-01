# OTS Integration Design

This document describes the integration strategy for the Off-The-Shelf (OTS) software items
consumed by the SpdxTool program. OTS items are third-party components providing functionality not
developed within this program.

## Categories

OTS items fall into three categories:

**Runtime NuGet package dependencies** — libraries consumed at runtime by the local systems and
distributed as transitive dependencies in the published NuGet packages:

- **YamlDotNet** — YAML deserialization for workflow input files
- **DemaConsulting.SpdxModel** — SPDX document object model and JSON serialization
- **DemaConsulting.NuGet.Caching** — NuGet package metadata caching for the run-workflow command
- **DemaConsulting.TestResults** — TRX and JUnit XML serialization for the SelfTest subsystem

**Test framework** — library consumed only by test projects and never included in the published
product:

- **xUnit** — unit test execution and result reporting

**CI pipeline tools** — dotnet tools invoked by the GitHub Actions build pipeline and not part of
the deployed product:

- **DemaConsulting.BuildMark** — generates build-notes documentation from GitHub Actions metadata
- **DemaConsulting.ReqStream** — enforces requirements traceability against test evidence
- **DemaConsulting.VersionMark** — captures and publishes tool-version information
- **DemaConsulting.SarifMark** — converts CodeQL SARIF results to a markdown report
- **DemaConsulting.SonarMark** — generates a SonarCloud quality report

## Integration Strategy

Runtime NuGet packages are declared as package references in the project files of the consuming
systems and are loaded by the .NET runtime at process startup. No explicit initialization or
disposal is required unless noted in the individual design documents. They are included in the
distributed NuGet packages as transitive dependencies.

The test framework is referenced only in test project files and is never included in the published
NuGet packages. xUnit discovers and executes test methods via the `xunit.runner.visualstudio`
adapter and writes TRX result files that feed into coverage reporting and requirements
traceability.

CI pipeline tools are installed as global dotnet tools in the GitHub Actions workflow environment.
They read files produced by earlier pipeline steps — SPDX documents, TRX result files, SARIF
output — and write markdown artifacts to the release bundle. They have no runtime dependency on
the local systems and do not affect the distributed product.

Individual integration and usage designs for each OTS item are documented in the OTS design
subfolder. OTS requirements are recorded in the YAML files under `docs/reqstream/ots/`.
Verification evidence is recorded in `docs/verification/ots/`.
