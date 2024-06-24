# SPDX Tool Workflow Files

While many SPDX Tool commands can be executed from command-line; the normal use of the tool is through YAML workflow files. These files have the benefit of:

- Comments to explain the purpose behind each step
- Variables to transfer information between steps

## File Structure

SPDX Tool workflow files have the following basic structure:

```yaml
# Workflow parameters
parameters:
  parameter-name: value

# Workflow steps
steps:
- command: <command-name>
  inputs:
    <arguments mapping>

- command: <command-name>
  inputs:
    input1: value
    input2: ${{ parameter-name }}
```

## Variables

Variables may be declaredat the top of the workflow file in a parameters section, or may be created when used as an output in a workflow step.

```yaml
# Workflow parameters
parameters:
  parameter1: value1
  parameter2: value2
```

Variables can be expanded in step inputs using the dollar expansion syntax

```yaml
# Workflow steps
steps:
- command: <command-name>
  inputs:
    input1: ${{ parameter1 }}
    input2: Insert ${{ parameter2 }} in the middle
```

Variables can be overridden on the command line:

```
spdx-tool run-workflow workflow.yaml parameter1=command parameter2=line
```

Variables can be changed at runtime by some steps:

```yaml
# Workflow parameters
parameters:
  reported-version: unknown
  dotnet-version: unknown
  pretty-version: unknown

steps:
- command: get-version
  inputs:
    spdx: manifest.spdx.json
    id: SPDXRef-DotNetSDK
    output: reported-version

- command: query
  inputs:
    output: dotnet-version
    pattern: '(?<value>\d+\.\d+\.\d+)'
    program: dotnet
    arguments:
    - '--version'

- command: set-variable
  inputs:
    value: DotNet Version is ${{ dotnet-version }}
    output: pretty-version
```


## YAML Commands

The following are the supported commands and their formats:

```yaml
steps:

  # Add a package to an SPDX document
- command: add-package
  inputs:
    spdx: <spdx.json>             # SPDX file name
    package:                      # New package information
      id: <id>                    # New package ID
      name: <name>                # New package name
      download: <download-url>    # New package download URL
      version: <version>          # Optional package version
      filename: <filename>        # Optional package filename
      supplier: <supplier>        # Optional package supplier
      originator: <originator>    # Optional package originator
      homepage: <homepage>        # Optional package homepage
      copyright: <copyright>      # Optional package copyright
      summary: <summary>          # Optional package summary
      description: <description>  # Optional package description
      license: <license>          # Optional package license
      purl: <package-url>         # Optional package purl
      cpe23: <cpe-identifier>     # Optional package cpe23
    relationships:                # Optional relationships
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # Add a relationship to an SPDX document
- command: add-relationship
  inputs:
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Element ID
    relationships:
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # Copy a package from one SPDX document to another SPDX document  
- command: copy-package
  inputs:
    from: <from.spdx.json>        # Source SPDX file name
    to: <to.spdx.json>            # Destination SPDX file name
    package: <package>            # Package ID
    recursive: true               # Optional recursive flag
    relationships:                # Optional relationships
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # finds the package ID for a package in an SPDX document
- command: find-package
  inputs:
    output: <variable>            # Output variable for package ID
    spdx: <spdx.json>             # SPDX file name
    name: <name>                  # Optional package name
    version: <version>            # Optional package version
    filename: <filename>          # Optional package filename
    download: <url>               # Optional package download URL

  # Get the version of a package in an SPDX document
- command: get-version
  inputs:
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Package ID
    output: <variable>            # Output variable

  # Perform hash operations on the specified file
- command: hash
  inputs:
    operation: generate | verify
    algorithm: sha256
    file: <file>

  # Print text to the console
- command: print
  inputs:
    text:
    - Some text to print
    - The value of variable is ${{ variable }}

  # Query information from the output of a program
- command: query
  inputs:
    output: <variable>
    pattern: <regex with 'value' capture>
    program: <program>
    arguments:
    - <argument>
    - <argument>

  # Rename the SPDX-ID of an element in an SPDX document
- command: rename-id
  inputs:
    spdx: <spdx.json>             # SPDX file name
    old: <old-id>                 # Old element ID
    new: <new-id>                 # New element ID

  # Run a separate workflow file/url
- command: run-workflow
  inputs:
    file: <workflow.yaml>         # Optional workflow file
    url: <url>                    # Optional workflow url
    integrity: <sha256>           # Optional workflow integrity check
    parameters:
      name: <value>               # Optional workflow parameter
      name: <value>               # Optional workflow parameter
    outputs:
      name: <variable>            # Optional output to save to variable
      name: <variable>            # Optional output to save to variable

  # Set a workflow variable
- command: set-variable
  inputs:
    value: <value>                # New value
    output: <variable>            # Variable to set

  # Create a summary markdown from the specified SPDX document
- command: to-markdown
  inputs:
    spdx: <spdx.json>             # SPDX file name
    markdown: <out.md>            # Output markdown file
    title: <title>                # Optional title
    depth: <depth>                # Optional heading depth

  # Update a package in an SPDX document
- command: update-package
  inputs:
    spdx: <spdx.json>             # SPDX filename
    package:                      # Package information
      id: <id>                    # Package ID
      name: <name>                # Optional new package name
      download: <download-url>    # Optional new package download URL
      version: <version>          # Optional new package version
      filename: <filename>        # Optional new package filename
      supplier: <supplier>        # Optional new package supplier
      originator: <originator>    # Optional new package originator
      homepage: <homepage>        # Optional new package homepage
      copyright: <copyright>      # Optional new package copyright
      summary: <summary>          # Optional new package summary
      description: <description>  # Optional new package description
      license: <license>          # Optional new package license

  # Validate an SPDX document
- command: validate
  inputs:
    spdx: <spdx.json>             # SPDX file name
    ntia: true                    # Optional NTIA checking
```
can be driven using workflow yaml files of the following format:

```yaml
# Workflow parameters
parameters:
  parameter-name: value

# Workflow steps
steps:
- command: <command-name>
  inputs:
    <arguments mapping>

- command: <command-name>
  inputs:
    input1: value
    input2: ${{ parameter-name }}
```

## YAML Variables

Variables are specified at the top of the workflow file in a parameters section:

```yaml
# Workflow parameters
parameters:
  parameter1: value1
  parameter2: value2
```

Variables can be expanded in step inputs using the dollar expansion syntax

```yaml
# Workflow steps
steps:
- command: <command-name>
  inputs:
    input1: ${{ parameter1 }}
    input2: Insert ${{ parameter2 }} in the middle
```

Variables can be overridden on the command line:

```
spdx-tool run-workflow workflow.yaml parameter1=command parameter2=line
```

Variables can be changed at runtime by some steps:

```yaml
# Workflow parameters
parameters:
  reported-version: unknown
  dotnet-version: unknown
  pretty-version: unknown

steps:
- command: get-version
  inputs:
    spdx: manifest.spdx.json
    id: SPDXRef-DotNetSDK
    output: reported-version

- command: query
  inputs:
    output: dotnet-version
    pattern: '(?<value>\d+\.\d+\.\d+)'
    program: dotnet
    arguments:
    - '--version'

- command: set-variable
  inputs:
    value: DotNet Version is ${{ dotnet-version }}
    output: pretty-version
```


## YAML Commands

The following are the supported commands and their formats:

```yaml
steps:

  # Add a package to an SPDX document
- command: add-package
  inputs:
    spdx: <spdx.json>             # SPDX file name
    package:                      # New package information
      id: <id>                    # New package ID
      name: <name>                # New package name
      download: <download-url>    # New package download URL
      version: <version>          # Optional package version
      filename: <filename>        # Optional package filename
      supplier: <supplier>        # Optional package supplier
      originator: <originator>    # Optional package originator
      homepage: <homepage>        # Optional package homepage
      copyright: <copyright>      # Optional package copyright
      summary: <summary>          # Optional package summary
      description: <description>  # Optional package description
      license: <license>          # Optional package license
      purl: <package-url>         # Optional package purl
      cpe23: <cpe-identifier>     # Optional package cpe23
    relationships:                # Optional relationships
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # Add a relationship to an SPDX document
- command: add-relationship
  inputs:
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Element ID
    relationships:
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # Copy a package from one SPDX document to another SPDX document  
- command: copy-package
  inputs:
    from: <from.spdx.json>        # Source SPDX file name
    to: <to.spdx.json>            # Destination SPDX file name
    package: <package>            # Package ID
    recursive: true               # Optional recursive flag
    relationships:                # Optional relationships
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # finds the package ID for a package in an SPDX document
- command: find-package
  inputs:
    output: <variable>            # Output variable for package ID
    spdx: <spdx.json>             # SPDX file name
    name: <name>                  # Optional package name
    version: <version>            # Optional package version
    filename: <filename>          # Optional package filename
    download: <url>               # Optional package download URL

  # Get the version of a package in an SPDX document
- command: get-version
  inputs:
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Package ID
    output: <variable>            # Output variable

  # Perform hash operations on the specified file
- command: hash
  inputs:
    operation: generate | verify
    algorithm: sha256
    file: <file>

  # Print text to the console
- command: print
  inputs:
    text:
    - Some text to print
    - The value of variable is ${{ variable }}

  # Query information from the output of a program
- command: query
  inputs:
    output: <variable>
    pattern: <regex with 'value' capture>
    program: <program>
    arguments:
    - <argument>
    - <argument>

  # Rename the SPDX-ID of an element in an SPDX document
- command: rename-id
  inputs:
    spdx: <spdx.json>             # SPDX file name
    old: <old-id>                 # Old element ID
    new: <new-id>                 # New element ID

  # Run a separate workflow file/url
- command: run-workflow
  inputs:
    file: <workflow.yaml>         # Optional workflow file
    url: <url>                    # Optional workflow url
    integrity: <sha256>           # Optional workflow integrity check
    parameters:
      name: <value>               # Optional workflow parameter
      name: <value>               # Optional workflow parameter
    outputs:
      name: <variable>            # Optional output to save to variable
      name: <variable>            # Optional output to save to variable

  # Set a workflow variable
- command: set-variable
  inputs:
    value: <value>                # New value
    output: <variable>            # Variable to set

  # Create a summary markdown from the specified SPDX document
- command: to-markdown
  inputs:
    spdx: <spdx.json>             # SPDX file name
    markdown: <out.md>            # Output markdown file
    title: <title>                # Optional title
    depth: <depth>                # Optional heading depth

  # Update a package in an SPDX document
- command: update-package
  inputs:
    spdx: <spdx.json>             # SPDX filename
    package:                      # Package information
      id: <id>                    # Package ID
      name: <name>                # Optional new package name
      download: <download-url>    # Optional new package download URL
      version: <version>          # Optional new package version
      filename: <filename>        # Optional new package filename
      supplier: <supplier>        # Optional new package supplier
      originator: <originator>    # Optional new package originator
      homepage: <homepage>        # Optional new package homepage
      copyright: <copyright>      # Optional new package copyright
      summary: <summary>          # Optional new package summary
      description: <description>  # Optional new package description
      license: <license>          # Optional new package license

  # Validate an SPDX document
- command: validate
  inputs:
    spdx: <spdx.json>             # SPDX file name
    ntia: true                    # Optional NTIA checking
```
