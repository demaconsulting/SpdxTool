package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateQuery {
        doc /* Self-test step exercising the query command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateQuery.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateQueryTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-query.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-query.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-query.yaml");
        }
    }
}
