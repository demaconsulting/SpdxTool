## NuGetCaching

### Purpose

DemaConsulting.NuGet.Caching provides NuGet package metadata caching used by the RunWorkflow
command when resolving workflow files embedded in NuGet packages. It was chosen to avoid redundant
network requests to NuGet feeds during workflow execution when multiple steps reference packages
from the same feed.

### Features Used

**Package metadata cache** — the caching facade retrieves and caches package information keyed by
package ID and version, allowing repeated lookups within a workflow run to be served from an
in-process cache rather than issuing a new network request for each step.

**Local cache resolution** — the library provides the path to the requested NuGet package in the
local NuGet cache directory, which the RunWorkflow command uses to locate embedded workflow YAML
files.

### Integration Pattern

DemaConsulting.NuGet.Caching is referenced as a NuGet package dependency in
`DemaConsulting.SpdxTool`. It is instantiated by the `RunWorkflow` command unit when processing
workflow steps that specify a package source. The cache instance is scoped to the lifetime of a
single `run-workflow` command invocation and is discarded when the command completes. No
additional configuration is required beyond constructing the cache object.
