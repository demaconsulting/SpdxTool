## NuGetCaching

### Verification Approach

DemaConsulting.NuGet.Caching is verified through the SelfTest subsystem, which exercises the
RunWorkflow command's NuGet package resolution path as part of the `--validate` self-test suite.
The `ValidateRunNuGetWorkflow` self-test step invokes a workflow that references a NuGet-embedded
workflow file, confirming that the caching facade correctly resolves and caches package metadata
and provides a valid local cache path for the embedded file.

The self-validation test `SpdxTool_SelfTest_ValidateFlag_Succeeds` covers this scenario by
running the full self-test suite, which includes the NuGet workflow validation step.

No dedicated unit tests for the caching library itself are maintained in this repository; the
self-test coverage is sufficient to confirm that the required functionality works in the deployed
tool environment. No vendor test results or third-party compliance reports are required.
