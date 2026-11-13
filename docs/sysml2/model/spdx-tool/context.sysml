package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Context {
        doc /* Per-invocation execution state: parsed global flags, command arguments, log-file output, and error count. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Context.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/ContextTests.cs");
            designFiles = ("docs/design/spdx-tool/context.md");
            verificationFiles = ("docs/verification/spdx-tool/context.md");
            requirementFiles = ("docs/reqstream/spdx-tool/context.yaml");
        }
    }
}
