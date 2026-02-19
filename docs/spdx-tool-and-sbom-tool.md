# SPDX Tool and SBOM Tool

A common approach to creating SPDX SBOMs is:

1. Use the [Microsoft SBOM DotNet Tool][sbom-tool] to create an SBOM
2. Use SPDX Tool to enhance the SBOM with extended information such as build tools

## Tool Manifest File

A tool manifest file should be constructed at `.config/dotnet-tools.json` specifying the version of SPDX Tool to
use. For example:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "microsoft.sbom.dotnettool": {
      "version": "2.2.6",
      "commands": [
        "sbom-tool"
      ]
    },
    "demaconsulting.spdxtool": {
      "version": "0.1.0-beta.1",
      "commands": [
        "spdx-tool"
      ]
    }
  }
}
```

## GitHub Actions Workflow

This example workflow:

- Installs dotnet
- Restores the dotnet tools from the tool manifest
- Runs the Microsoft SBOM DotNet Tool to generate the SBOM
- Runs SPDX Tool to enhance the SBOM

```yaml
    - name: Setup dotnet
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.x

    - name: Restore Tools
      run: |
        dotnet tool restore

    # TODO - run the build here

    - name: Generate SBOM
      run: >
        dotnet sbom-tool generate
        -b <build-drop-path>
        -bc <build-build-path>
        -pn <package-name>
        -pv <package-version>
        -ps <package-supplier>
        -nsb <sbom-namespace>
        -li true
        -pm true

    - name: Enhance SBOM
      run: |
        dotnet spdx-tool run-workflow spdx-workflow.yaml
```

[sbom-tool]: https://github.com/microsoft/sbom-tool
