package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateToMarkdown {
        doc /* Self-test step exercising the to-markdown command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateToMarkdown.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateToMarkdownTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-to-markdown.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-to-markdown.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-to-markdown.yaml");
        }
    }
}
