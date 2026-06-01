# CI/CD Integration

## GitHub Actions

The following example GitHub Actions workflow installs SpdxTool globally, validates an SPDX document
for NTIA compliance, generates a markdown summary, and publishes both as artifacts:

```yaml
name: SBOM Validation

on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'

      - name: Install SpdxTool
        run: dotnet tool install --global DemaConsulting.SpdxTool

      - name: Generate SBOM
        run: |
          # Your SBOM generation commands here

      - name: Validate SBOM
        run: spdx-tool validate manifest.spdx.json ntia

      - name: Generate Summary
        run: spdx-tool to-markdown manifest.spdx.json sbom-summary.md

      - name: Upload SBOM
        uses: actions/upload-artifact@v4
        with:
          name: sbom
          path: |
            manifest.spdx.json
            sbom-summary.md
```

For local-tool installations, add a `dotnet tool restore` step before the SpdxTool commands to
restore tools from the `.config/dotnet-tools.json` manifest.

## Azure DevOps

The following example Azure DevOps pipeline installs SpdxTool globally, validates an SPDX document,
generates a markdown summary, and publishes it as a build artifact:

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.x'

  - script: |
      dotnet tool install --global DemaConsulting.SpdxTool
    displayName: 'Install SpdxTool'

  - script: |
      spdx-tool validate manifest.spdx.json ntia
    displayName: 'Validate SBOM'

  - script: |
      spdx-tool to-markdown manifest.spdx.json sbom-summary.md
    displayName: 'Generate SBOM Summary'

  - task: PublishBuildArtifacts@1
    inputs:
      pathToPublish: 'manifest.spdx.json'
      artifactName: 'sbom'
```
