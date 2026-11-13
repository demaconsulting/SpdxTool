package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def TemporaryDirectory {
        doc /* Disposable, isolated working folder for self-validation and tests. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Utility/TemporaryDirectory.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Utility/TemporaryDirectoryTests.cs");
            designFiles = ("docs/design/spdx-tool/utility/temporary-directory.md");
            verificationFiles = ("docs/verification/spdx-tool/utility/temporary-directory.md");
            requirementFiles = ("docs/reqstream/spdx-tool/utility/temporary-directory.yaml");
        }
    }
}
