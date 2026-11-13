package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def UtilitySubsystem {
        doc /* Stateless cross-cutting helpers shared across the tool. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/spdx-tool/utility.md");
            verificationFiles = ("docs/verification/spdx-tool/utility.md");
            requirementFiles = ("docs/reqstream/spdx-tool/utility/utility.yaml");
        }

        part temporaryDirectory : TemporaryDirectory;
        part pathHelpers : PathHelpers;
        part wildcard : Wildcard;
    }
}
