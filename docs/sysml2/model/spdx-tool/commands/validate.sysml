package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def CommandsValidate {
        doc /* Command that validates an SPDX document for specification and optional NTIA minimum-elements compliance (the Commands/Validate class). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Validate.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/ValidateTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/validate.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/validate.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/validate.yaml");
        }
    }
}
