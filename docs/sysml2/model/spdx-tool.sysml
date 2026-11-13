package SpdxTool {
    private import SoftwareItemMetadata::*;

    import OtsDependencies::*;

    doc
    /*
     * DemaConsulting.SpdxTool is a .NET command-line tool for creating, validating, and
     * manipulating SPDX (Software Package Data Exchange) documents.
     */

    part def SpdxToolSystem {
        doc /* The DemaConsulting.SpdxTool system as a whole. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/IntegrationTests.cs");
            designFiles = ("docs/design/spdx-tool.md");
            verificationFiles = ("docs/verification/spdx-tool.md");
            requirementFiles = ("docs/reqstream/spdx-tool/spdx-tool.yaml", "docs/reqstream/spdx-tool/platform-requirements.yaml");
        }

        part program : Program;
        part context : Context;
        part commands : CommandsSubsystem;
        part selfTest : SelfTestSubsystem;
        part spdx : SpdxSubsystem;
        part utility : UtilitySubsystem;

        part spdxModel : SpdxModel;
        part nuGetCaching : NuGetCaching;
        part testResults : TestResults;
        part yamlDotNet : YamlDotNet;
    }
}
