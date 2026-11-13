package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def SpdxSubsystem {
        doc /* Stateless SPDX-domain helpers shared by the commands. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/spdx-tool/spdx.md");
            verificationFiles = ("docs/verification/spdx-tool/spdx.md");
            requirementFiles = ("docs/reqstream/spdx-tool/spdx/spdx.yaml");
        }

        part relationshipDirection : RelationshipDirection;
        part spdxHelpers : SpdxHelpers;
    }
}
