package OtsDependencies {
    private import SoftwareItemMetadata::*;

    doc /* Off-the-shelf (OTS) runtime dependencies used by SpdxTool. */

    part def SpdxModel {
        doc /* OTS: DemaConsulting.SpdxModel, the in-memory SPDX document model used to read, manipulate, and write SPDX JSON. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/spdx-model.md");
            verificationFiles = ("docs/verification/ots/spdx-model.md");
            requirementFiles = ("docs/reqstream/ots/spdx-model.yaml");
        }
    }

    part def NuGetCaching {
        doc /* OTS: DemaConsulting.NuGet.Caching, used by run-workflow to fetch workflow files from NuGet packages. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/nuget-caching.md");
            verificationFiles = ("docs/verification/ots/nuget-caching.md");
            requirementFiles = ("docs/reqstream/ots/nuget-caching.yaml");
        }
    }

    part def TestResults {
        doc /* OTS: DemaConsulting.TestResults, used by the SelfTest subsystem to record and serialize self-validation results. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/test-results.md");
            verificationFiles = ("docs/verification/ots/test-results.md");
            requirementFiles = ("docs/reqstream/ots/test-results.yaml");
        }
    }

    part def YamlDotNet {
        doc /* OTS: YamlDotNet, used to parse workflow YAML files. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/yamldotnet.md");
            verificationFiles = ("docs/verification/ots/yamldotnet.md");
            requirementFiles = ("docs/reqstream/ots/yamldotnet.yaml");
        }
    }
}
