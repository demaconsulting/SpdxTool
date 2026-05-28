### Wildcard

#### Verification Approach

`Wildcard` is verified with focused unit tests in
`test/DemaConsulting.SpdxTool.Tests/Utility/WildcardTests.cs`. The tests verify
wildcard-to-regular-expression conversion and case-insensitive matching behavior.

#### Test Environment

Tests execute in the xUnit v3 environment as part of `DemaConsulting.SpdxTool.Tests`. No
external service or file system access is required because Wildcard is a pure in-process helper.

#### Acceptance Criteria

Verification is acceptable when exact, `*`, and `?` pattern matching behaves correctly
case-insensitively across all test inputs.

#### Test Scenarios

**ExactMatch**: exact patterns match equal strings case-insensitively and do not match
strings that differ in content or length. This scenario is tested by
`Wildcard_IsMatch_ExactMatch_ReturnsTrue`.

**AsteriskWildcard**: asterisk wildcards match variable-length substrings without losing
case-insensitive behavior. This scenario is tested by
`Wildcard_IsMatch_AsteriskPattern_MatchesMultipleChars`.

**QuestionMarkWildcard**: question mark wildcards match exactly one character. This scenario
is tested by `Wildcard_IsMatch_QuestionMarkPattern_MatchesSingleChar`.
