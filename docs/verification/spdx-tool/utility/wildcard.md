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

**NullArguments**: null input or pattern arguments throw ArgumentNullException immediately.
This scenario is tested by `Wildcard_IsMatch_NullInput_ThrowsArgumentNullException` and
`Wildcard_IsMatch_NullPattern_ThrowsArgumentNullException`, linked to requirement
`SpdxTool-Utility-Wildcard-NullArgs`.

**EmptyStringBoundary**: empty input and empty pattern boundary conditions are handled
correctly — an empty input matches an empty pattern, a non-empty input does not match an
empty pattern, and an asterisk matches the empty string. This scenario is tested by
`Wildcard_IsMatch_EmptyInputs_BehavesCorrectly`.
