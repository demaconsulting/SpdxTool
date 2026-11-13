package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def SpdxHelpers {
        doc /* Shared helpers for loading and saving SPDX JSON documents with consistent error reporting. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Spdx/SpdxHelpers.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Spdx/SpdxHelpersTests.cs");
            designFiles = ("docs/design/spdx-tool/spdx/spdx-helpers.md");
            verificationFiles = ("docs/verification/spdx-tool/spdx/spdx-helpers.md");
            requirementFiles = ("docs/reqstream/spdx-tool/spdx/spdx-helpers.yaml");
        }
    }
}
