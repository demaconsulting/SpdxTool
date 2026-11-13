package SpdxTool {
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
