## TestResults

### Purpose

DemaConsulting.TestResults provides TRX and JUnit XML serialization used by the SelfTest
subsystem to write validation results in formats consumable by CI/CD systems. It was chosen
because TRX output is required by the ReqStream requirements traceability tool and JUnit XML is
required by GitHub Actions test reporting.

### Features Used

**TrxSerializer** — serializes self-test validation results to the TRX format consumed by the
ReqStream requirements traceability tool and by test reporting integrations in the CI pipeline.

**JUnitSerializer** — serializes self-test validation results to the JUnit XML format used by
CI/CD test reporting dashboards.

### Integration Pattern

DemaConsulting.TestResults is referenced as a NuGet package dependency in
`DemaConsulting.SpdxTool`. The `SelfTest.Validate` orchestrator unit instantiates
`TrxSerializer` and `JUnitSerializer` after all validation steps complete and writes result files
to the paths supplied by the `--result` command-line option. Serializer instances are short-lived
and are not shared between validation runs.
