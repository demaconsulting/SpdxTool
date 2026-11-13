package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def SelfTestSubsystem {
        doc /* Built-in --validate self-test suite that exercises the tool's commands against embedded SPDX fixtures. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/SelfTestTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/self-test.yaml");
        }

        part selfTestValidate : SelfTestValidate;
        part validateBasic : ValidateBasic;
        part validateAddPackage : ValidateAddPackage;
        part validateAddRelationship : ValidateAddRelationship;
        part validateCopyPackage : ValidateCopyPackage;
        part validateDiagram : ValidateDiagram;
        part validateFindPackage : ValidateFindPackage;
        part validateGetVersion : ValidateGetVersion;
        part validateHash : ValidateHash;
        part validateNtia : ValidateNtia;
        part validateQuery : ValidateQuery;
        part validateRenameId : ValidateRenameId;
        part validateRunNuGetWorkflow : ValidateRunNuGetWorkflow;
        part validateToMarkdown : ValidateToMarkdown;
        part validateUpdatePackage : ValidateUpdatePackage;
    }
}
