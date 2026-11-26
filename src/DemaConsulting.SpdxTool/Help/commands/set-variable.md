---
title: set-variable command
summary: Set a workflow variable from text that may use other variables (workflow only).
tags:
  - reference
see-also:
  - workflow-files
---
`set-variable` sets a variable of the running workflow. It runs only as
a workflow step:

```yaml
- command: set-variable
  inputs:
    value: <value>                # New value
    output: <variable>            # Variable to set
```

`value` is expanded first, so it can combine other variables, as in
`${{ app-id }} ${{ app-version }}`; `output` is the variable name, taken
as written. Later steps read the variable as `${{ <variable> }}`, and a
parent workflow can read it through `outputs`; see [[workflow-files]].

On the command line the command exits 1 with
`'set-variable' command is only valid in a workflow`.
