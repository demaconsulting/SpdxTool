package SpdxToolTargets {
    private import SoftwareItemMetadata::*;

    part def TargetsSingle {
        doc /* Single-TFM MSBuild targets that unzip the package, run the spdx-tool workflow on its SBOM, and rezip it (documented in the system doc). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool.Targets/build/DemaConsulting.SpdxTool.Targets.targets");
            testFiles = ("test/DemaConsulting.SpdxTool.Targets.Tests/DecorateSbomTargetTests.cs");
            designFiles = ("docs/design/spdx-tool-targets.md");
            verificationFiles = ("docs/verification/spdx-tool-targets.md");
            requirementFiles = ("docs/reqstream/spdx-tool-targets/targets-single.yaml");
        }
    }
}
