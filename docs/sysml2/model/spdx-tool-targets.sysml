package SpdxToolTargets {
    private import SoftwareItemMetadata::*;

    doc
    /*
     * DemaConsulting.SpdxTool.Targets is an MSBuild targets extension that decorates the SBOM
     * inside a NuGet package by running spdx-tool during dotnet pack.
     */

    part def SpdxToolTargetsSystem {
        doc /* The DemaConsulting.SpdxTool.Targets system as a whole. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.SpdxTool.Targets.Tests/DecorateSbomTargetTests.cs");
            designFiles = ("docs/design/spdx-tool-targets.md");
            verificationFiles = ("docs/verification/spdx-tool-targets.md");
            requirementFiles = ("docs/reqstream/spdx-tool-targets/spdx-tool-targets.yaml");
        }

        part targetsSingle : TargetsSingle;
        part targetsMulti : TargetsMulti;
    }
}
