package SpdxToolTargets {
    private import SoftwareItemMetadata::*;

    part def TargetsMulti {
        doc /* Multi-TFM MSBuild targets that import the single-TFM targets so decoration runs once per package (documented in the system doc). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool.Targets/buildMultiTargeting/DemaConsulting.SpdxTool.Targets.targets");
            testFiles = ("test/DemaConsulting.SpdxTool.Targets.Tests/DecorateSbomTargetTests.cs");
            designFiles = ("docs/design/spdx-tool-targets.md");
            verificationFiles = ("docs/verification/spdx-tool-targets.md");
            requirementFiles = ("docs/reqstream/spdx-tool-targets/targets-multi.yaml");
        }
    }
}
