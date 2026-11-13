package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ToMarkdown {
        doc /* Command that writes a Markdown summary of an SPDX document. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/ToMarkdown.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/ToMarkdownTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/to-markdown.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/to-markdown.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/to-markdown.yaml");
        }
    }
}
