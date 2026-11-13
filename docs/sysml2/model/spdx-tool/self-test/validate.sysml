package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def SelfTestValidate {
        doc /* Self-validation orchestrator that runs every step, prints a summary, and optionally writes TRX or JUnit results (the SelfTest/Validate class). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/Validate.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate.yaml");
        }
    }
}
