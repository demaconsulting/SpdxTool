---
title: Self-validation
summary: Prove that the installed tool works, and save the evidence as test results.
aliases:
  - self-test
tags:
  - reference
order: 7
see-also:
  - command-line
examples:
  - name: run
    kind: command
    language: shell
    description: Run the self-tests. Prints one line per test and exits 0 when all pass.
    content: '{{tool}} --validate'
    expect: 'Validation Passed'
  - name: trx
    kind: command
    language: shell
    description: Also write the results as a TRX file.
    content: '{{tool}} --validate --result validation.trx'
  - name: bad-extension
    kind: command
    language: shell
    description: Any other extension is an error. Exits 1.
    content: '{{tool}} --validate --result validation.txt'
    exit: 1
    expect: 'Unsupported results file format'
---
`--validate` runs a set of tests of the tool itself in a temporary
folder, with SPDX documents and workflows it writes there, so a
regulated project can show that the installed version works on its
build machine.

**Report.** The output is Markdown. It starts with a heading and a table
of the tool version, machine name, operating system, .NET runtime, and
time stamp. Each test prints one line with `Passed` or `Failed`, and the
report ends with `Total Tests`, `Passed`, and `Failed` lines, followed
by `Validation Passed` when no test failed. The exit code is 0 only when
every test passes. `--depth` sets the level of the report heading.

**Results files.** `--result <file>` (or `-r`) also writes the outcome;
the extension chooses the format. `.trx` writes a Visual Studio TRX file
and `.xml` writes JUnit XML, both readable by CI test-report tasks. Any
other extension is an error after the tests ran ([[#bad-extension]]).

**Evidence.** The tests have stable names that a requirements file can
list as the tests that prove the tool works in its environment:
`SpdxTool_AddPackage`, `SpdxTool_AddRelationship`, `SpdxTool_Basic`,
`SpdxTool_CopyPackage`, `SpdxTool_Diagram`, `SpdxTool_FindPackage`,
`SpdxTool_GetVersion`, `SpdxTool_Hash`, `SpdxTool_Ntia`,
`SpdxTool_Query`, `SpdxTool_RenameId`, `SpdxTool_RunNuGetWorkflow`,
`SpdxTool_ToMarkdown`, and `SpdxTool_UpdatePackage`.

**Requirements.** `SpdxTool_Query` runs `dotnet`, and
`SpdxTool_RunNuGetWorkflow` needs network access or the workflow
package in the NuGet cache.
