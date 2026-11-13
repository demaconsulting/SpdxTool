package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def PathHelpers {
        doc /* Safe path combination that prevents user-supplied paths escaping the base directory. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Utility/PathHelpers.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Utility/PathHelpersTests.cs");
            designFiles = ("docs/design/spdx-tool/utility/path-helpers.md");
            verificationFiles = ("docs/verification/spdx-tool/utility/path-helpers.md");
            requirementFiles = ("docs/reqstream/spdx-tool/utility/path-helpers.yaml");
        }
    }
}
