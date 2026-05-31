# Command-Line Reference

## General Syntax

The general command-line syntax is:

```bash
spdx-tool [options] <command> [arguments]
```

## Global Options

- `-h, -?, --help` — Show help message and exit
- `-v, --version` — Show version information and exit
- `-l, --log <log-file>` — Log output to file
- `-s, --silent` — Silence console output
- `--validate` — Perform self-validation
- `-r, --result <file>` — Self-validation result file (`.trx` TRX format or `.xml` JUnit XML format,
  auto-detected from extension)
- `--depth <level>` — Self-validation report depth level

## Available Commands

- `help <command>` — Display extended help about a command
- `add-package` — Add package to SPDX document (workflow only)
- `add-relationship <spdx.json> <args>` — Add relationship between elements
- `copy-package <spdx.json> <args>` — Copy package between SPDX documents
- `diagram <spdx.json> <mermaid.txt> [tools]` — Generate mermaid diagram
- `find-package <spdx.json> <criteria>` — Find package ID in SPDX document
- `get-version <spdx.json> <criteria>` — Get the version of an SPDX package
- `hash <operation> <algorithm> <file>` — Generate or verify hashes of files
- `print <text>` — Print text to the console
- `query <pattern> <program> [args]` — Query program output for value
- `rename-id <arguments>` — Rename an element ID in an SPDX document
- `run-workflow <workflow.yaml>` — Runs the workflow file/url
- `set-variable` — Set workflow variable (workflow only)
- `to-markdown <spdx.json> <out.md> [args]` — Create Markdown summary for SPDX document
- `update-package` — Update package in SPDX document (workflow only)
- `validate <spdx.json> [ntia]` — Validate SPDX document for issues

## Getting Command Help

To get detailed help for any command:

```bash
dotnet spdx-tool help <command>
```

For example:

```bash
dotnet spdx-tool help validate
```

## Validate Command

The `validate` command checks an SPDX document for correctness and optionally for NTIA compliance.

**Syntax:**

```bash
spdx-tool validate <spdx.json> [ntia]
```

**Example:**

```bash
dotnet spdx-tool validate manifest.spdx.json
dotnet spdx-tool validate manifest.spdx.json ntia
```

## Add Relationship Command

The `add-relationship` command adds relationships between SPDX elements.

**Syntax:**

```bash
spdx-tool add-relationship <spdx.json> <id> <type> <element> [comment]
```

**Example:**

```bash
dotnet spdx-tool add-relationship manifest.spdx.json SPDXRef-Package DEPENDS_ON SPDXRef-Library
```

## Copy Package Command

The `copy-package` command copies a package with its relationships between SPDX documents.

**Syntax:**

```bash
spdx-tool copy-package <spdx.json> <args>
```

**Example:**

```bash
dotnet spdx-tool copy-package source.spdx.json SPDXRef-Package target.spdx.json
```

## Find Package Command

The `find-package` command locates a package in an SPDX document based on criteria.

**Syntax:**

```bash
spdx-tool find-package <spdx.json> <criteria>
```

**Example:**

```bash
dotnet spdx-tool find-package manifest.spdx.json name=MyPackage
dotnet spdx-tool find-package manifest.spdx.json version=1.0.0
```

## Get Version Command

The `get-version` command retrieves the version of a package in an SPDX document.

**Syntax:**

```bash
spdx-tool get-version <spdx.json> <criteria>
```

**Example:**

```bash
dotnet spdx-tool get-version manifest.spdx.json id=SPDXRef-Package
```

## To Markdown Command

The `to-markdown` command generates a human-readable markdown summary of an SPDX document.

**Syntax:**

```bash
spdx-tool to-markdown <spdx.json> <out.md> [title] [depth]
```

**Example:**

```bash
dotnet spdx-tool to-markdown manifest.spdx.json summary.md "SBOM Summary"
```

## Diagram Command

The `diagram` command generates a Mermaid diagram visualizing SPDX relationships.

**Syntax:**

```bash
spdx-tool diagram <spdx.json> <mermaid.txt> [tools]
```

**Example:**

```bash
dotnet spdx-tool diagram manifest.spdx.json diagram.mmd
```

## Hash Command

The `hash` command generates or verifies file hashes.

**Syntax:**

```bash
spdx-tool hash <operation> <algorithm> <file>
```

**Example:**

```bash
dotnet spdx-tool hash generate sha256 myfile.txt
dotnet spdx-tool hash verify sha256 myfile.txt
```

## Rename ID Command

The `rename-id` command renames an element identifier throughout an SPDX document.

**Syntax:**

```bash
spdx-tool rename-id <arguments>
```

**Example:**

```bash
dotnet spdx-tool rename-id manifest.spdx.json SPDXRef-Package-1 SPDXRef-Package-2
```

## Query Command

The `query` command executes an external program, captures its output, and extracts a value using a
regular expression with a named capture group.

**Syntax:**

```bash
spdx-tool query <pattern> <program> [args]
```

**Workflow YAML example:**

```yaml
- command: query
  inputs:
    output: dotnet-version
    pattern: "(?<value>\\d+\\.\\d+\\.\\d+.*)"
    program: dotnet
    arguments:
      - --version
```

The `query` command executes a program and applies a regular expression with a named `value` capture
group to extract a result from its output. In CLI mode the extracted value is written to stdout. In
workflow mode the result is stored in the named output variable for use by subsequent steps.

## Print Command

The `print` command prints the value of one or more workflow variables to the output.

**Syntax:**

```bash
spdx-tool print <variable> [<variable>...]
```

**Workflow YAML example:**

```yaml
- command: print
  inputs:
    text:
      - "Version: ${{ dotnet-version }}"
```

The `print` command writes one or more lines of text to the output. In CLI mode each argument is
printed as a separate line. In workflow mode a `text` sequence node provides the lines, with variable
expansion applied to each entry.
