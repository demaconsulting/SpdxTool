# SPDX Tool for GitHub CI

SPDX Tool can be used in GitHub actions by:

- Adding a tool manifest file to the repository
- Ensuring dotnet is installed on the build agent
- Restoring dotnet tools
- Running SPDX Tool

## Tool Manifest File

A tool manifest file should be constructed at `.config/dotnet-tools.json` specifying the version of SPDX Tool to
use. For example:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "demaconsulting.spdxtool": {
      "version": "0.1.0-beta.1",
      "commands": [
        "spdx-tool"
      ]
    }
  }
}
```

## Install DotNet Step

The GitHub Actions workflow file should have a step to ensure dotnet is installed. For example:

```yaml
    - name: Setup dotnet 8
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 8.x
```

## Restore DotNet Tools Step

The GitHub Actions workflow file should have a step to restore dotnet tools specified in the manifest file. For example:

```yaml
    - name: Restore Tools
      run: |
        dotnet tool restore
```

## Run SPDX Tool Step

The GitHub Actions workflow file should have a step to run SPDX Tool. For example:

```yaml
    - name: Run SBOM Workflow
      run: |
        dotnet spdx-tool run-workflow spdx-workflow.yaml
```
