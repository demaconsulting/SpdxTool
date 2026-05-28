### ValidateUpdatePackage

#### Verification Approach

`ValidateUpdatePackage` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateUpdatePackageTests.cs`, which runs the step
end-to-end against temporary SPDX content.

#### Test Environment

The test uses a temporary working directory with local SPDX and workflow fixtures in the standard
xUnit v3 environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after updating the
expected package metadata in the SPDX document.

#### Test Scenarios

**EndToEndUpdatePackage**: the self-test step proves that `update-package` updates package metadata
during validation. This scenario is tested by `SpdxTool_UpdatePackage`.
