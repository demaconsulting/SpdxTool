### TemporaryDirectory

#### Purpose

TemporaryDirectory provides an isolated, disposable working folder for self-validation and tests.
It creates a unique directory under the current working directory, exposes safe file-path
resolution within that directory, and removes the directory tree when disposed.

#### Data Model

N/A — TemporaryDirectory is a sealed disposable class with a single path property and no shared
state.

#### Key Members

**TemporaryDirectory()**: Creates a uniquely named subdirectory under `Environment.CurrentDirectory`.

- *Post-conditions*: `DirectoryPath` points to an existing directory.
- *Failure behavior*: wraps file-system creation failures in `InvalidOperationException`.

**DirectoryPath**: Gets the full path to the temporary directory.

- *Type*: string read-only property.

**GetFilePath(string)**: Returns a path within the temporary directory and creates any required
intermediate directories.

- *Parameters*: `relativePath` — a relative path under the temporary directory.
- *Returns*: `string` — the resolved path under `DirectoryPath`.
- *Preconditions*: `relativePath` must not be null.
- *Failure behavior*: throws `ArgumentException` when the relative path escapes the temporary
  directory.

**Dispose()**: Deletes the directory tree.

- *Failure behavior*: suppresses cleanup exceptions so disposal remains non-fatal.

#### Error Handling

**InvalidOperationException** — thrown when the temporary directory cannot be created.

**ArgumentNullException** — thrown when `GetFilePath` receives a null relative path.

**ArgumentException** — thrown when `GetFilePath` receives a path that escapes the directory.

#### Dependencies

- System.IO.Directory
- System.IO.Path
- System.Guid
- PathHelpers.SafePathCombine

#### Callers

- SelfTest.Validate.RunInTempDir — creates the outer temporary working directory for each
  validation step.
- TemporaryDirectoryTests — verifies construction, file-path resolution, and disposal behavior.
