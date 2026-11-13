package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateAddRelationship {
        doc /* Self-test step exercising the add-relationship command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateAddRelationship.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateAddRelationshipTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-add-relationship.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-add-relationship.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-add-relationship.yaml");
        }
    }
}
