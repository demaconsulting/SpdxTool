package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Hash {
        doc /* Command that generates or verifies a SHA-256 hash sidecar file for a file. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Hash.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/HashTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/hash.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/hash.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/hash.yaml");
        }
    }
}
