package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def RelationshipDirection {
        doc /* Parent/child/sibling traversal direction for SPDX relationship types, with the GetDirection extension mapping. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Spdx/RelationshipDirection.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Spdx/RelationshipDirectionTests.cs");
            designFiles = ("docs/design/spdx-tool/spdx/relationship-direction.md");
            verificationFiles = ("docs/verification/spdx-tool/spdx/relationship-direction.md");
            requirementFiles = ("docs/reqstream/spdx-tool/spdx/relationship-direction.yaml");
        }
    }
}
