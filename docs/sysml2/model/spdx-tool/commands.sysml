package SpdxTool_CommandsSubsystem {
    public import SpdxTool_AddPackage::*;
    public import SpdxTool_AddRelationship::*;
    public import SpdxTool_CommandEntry::*;
    public import SpdxTool_CommandErrorException::*;
    public import SpdxTool_CommandUsageException::*;
    public import SpdxTool_Command::*;
    public import SpdxTool_CommandsRegistry::*;
    public import SpdxTool_CopyPackage::*;
    public import SpdxTool_Diagram::*;
    public import SpdxTool_FindPackage::*;
    public import SpdxTool_GetVersion::*;
    public import SpdxTool_Hash::*;
    public import SpdxTool_Help::*;
    public import SpdxTool_Print::*;
    public import SpdxTool_Query::*;
    public import SpdxTool_RenameId::*;
    public import SpdxTool_RunWorkflow::*;
    public import SpdxTool_SetVariable::*;
    public import SpdxTool_ToMarkdown::*;
    public import SpdxTool_UpdatePackage::*;
    public import SpdxTool_CommandsValidate::*;

    private import SoftwareItemMetadata::*;

    part def CommandsSubsystem {
        doc /* Command-pattern implementations of every CLI and workflow command, plus the command registry and exception types. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/CommandTests.cs", "test/DemaConsulting.SpdxTool.Tests/Commands/UnknownCommandTests.cs");
            designFiles = ("docs/design/spdx-tool/commands.md");
            verificationFiles = ("docs/verification/spdx-tool/commands.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/commands.yaml");
        }

        part command : Command;
        part commandEntry : CommandEntry;
        part commandErrorException : CommandErrorException;
        part commandsRegistry : CommandsRegistry;
        part commandUsageException : CommandUsageException;
        part addPackage : AddPackage;
        part addRelationship : AddRelationship;
        part copyPackage : CopyPackage;
        part diagram : Diagram;
        part findPackage : FindPackage;
        part getVersion : GetVersion;
        part hash : Hash;
        part help : Help;
        part print : Print;
        part query : Query;
        part renameId : RenameId;
        part runWorkflow : RunWorkflow;
        part setVariable : SetVariable;
        part toMarkdown : ToMarkdown;
        part updatePackage : UpdatePackage;
        part commandsValidate : CommandsValidate;
    }
}
