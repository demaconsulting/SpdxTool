---
title: print command
summary: Print text to the console, one line for each argument or list item.
tags:
  - reference
see-also:
  - workflow-files
examples:
  - name: print
    kind: command
    language: shell
    description: Print one line of text.
    content: '{{tool}} print "SBOM checked"'
    expect: 'SBOM checked'
---
`print` prints text, one line for each argument. On the command line:

```text
{{tool}} print <text>...
```

The command that [[#print]] prints shows one line. As a workflow step
it prints each item of the `text` list, and expands workflow variables
in them, which makes it useful to report what a workflow did:

```yaml
- command: print
  inputs:
    text:
      - Some text to print
      - The value of variable is ${{ variable }}
```

Like all output, the text is hidden by `--silent` and copied to the
`--log` file; see [[command-line]].
