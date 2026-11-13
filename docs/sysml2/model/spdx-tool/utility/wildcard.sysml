package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Wildcard {
        doc /* Case-insensitive glob-style (* and ?) wildcard matching. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Utility/Wildcard.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Utility/WildcardTests.cs");
            designFiles = ("docs/design/spdx-tool/utility/wildcard.md");
            verificationFiles = ("docs/verification/spdx-tool/utility/wildcard.md");
            requirementFiles = ("docs/reqstream/spdx-tool/utility/wildcard.yaml");
        }
    }
}
