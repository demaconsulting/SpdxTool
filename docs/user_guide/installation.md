# Installation

## Prerequisites

Before installing SpdxTool, ensure you have:

- **.NET SDK**: Version 8.0 or later
- **Operating System**: Windows, Linux, or macOS

## Installation Methods

### Local Installation

To add SpdxTool to a .NET tool manifest file:

```bash
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local DemaConsulting.SpdxTool
```

The tool can then be executed by:

```bash
dotnet spdx-tool <arguments>
```

### Global Installation

For global installation across all projects:

```bash
dotnet tool install --global DemaConsulting.SpdxTool
```

Then execute directly:

```bash
spdx-tool <arguments>
```

## Verifying Installation

To verify SpdxTool is installed correctly:

```bash
dotnet spdx-tool --version
```

This will display the installed version number.
