### ValidateHash

#### Purpose

ValidateHash exercises the hash command end-to-end within the Self-Test subsystem. It verifies that
a SHA-256 hash file can be generated for a known file and that the generated hash file can subsequently
be verified, confirming both the generate and verify sub-commands function correctly.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the hash self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: When no exception is thrown, a TestResult entry named SpdxTool_Hash has been
  appended to results and a pass or fail message has been written to the Context.

**DoValidate**: orchestrates both sub-tests in a shared temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if both DoValidateGenerate and DoValidateVerify succeed.
- *Preconditions*: A writable working directory is available. Callers must execute serially because
  Validate.RunSpdxTool temporarily mutates the process-wide current working directory.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

**DoValidateGenerate**: verifies that the hash generate sub-command produces the correct SHA-256 hash.

- *Parameters*: None.
- *Returns*: `bool` — true if RunSpdxTool returns exit code zero, the .sha256 file exists, and the
  hash value matches the known expected digest.
- *Preconditions*: validate.tmp exists.
- *Post-conditions*: test-file.txt.sha256 has been created in validate.tmp. The comparison is exact
  (case-sensitive, no trimming), relying on the assumption that `GenerateSha256` writes only a
  lowercase hex string with no trailing whitespace.

Writes a test file containing "The quick brown fox jumps over the lazy dog", calls Validate.RunSpdxTool
with --silent, hash, generate, sha256, and the file path, then verifies the generated hash file
contains the expected SHA-256 digest value.

**DoValidateVerify**: verifies that the hash verify sub-command accepts a correct hash and rejects a
corrupted one.

- *Parameters*: None.
- *Returns*: `bool` — true if verification with the correct hash returns exit code zero and
  verification with a corrupted hash returns a non-zero exit code.
- *Preconditions*: validate.tmp and test-file.txt.sha256 exist from DoValidateGenerate.
- *Post-conditions*: If the first verification call succeeds (exit code zero), the hash file is
  subsequently overwritten with all-zero digits to test rejection. If the first call fails, the hash
  file is not overwritten and the method returns false immediately.

Calls Validate.RunSpdxTool twice: first with the correct hash (expects exit code zero), then after
overwriting the hash file with zeros (expects non-zero exit code).

#### Error Handling

Returns false if hash generate returns a non-zero exit code, the hash file is not created, or the
hash value does not match the expected digest. Returns false if hash verify with the correct hash
returns a non-zero exit code, or if hash verify with the corrupted hash returns exit code zero. The
finally block guards the Directory.Delete call with a Directory.Exists check to prevent a secondary
DirectoryNotFoundException masking the original exception when Directory.CreateDirectory fails
(e.g., because validate.tmp already exists as a file). If `DoValidate` throws an exception, the
exception propagates uncaught out of `Run()` and no `TestResult` is appended to results for this
step.

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the hash command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
