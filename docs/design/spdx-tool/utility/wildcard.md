### Wildcard

#### Purpose

Wildcard converts glob-style wildcard patterns (`*` and `?`) into regular expressions and
matches them against input strings case-insensitively. It is used by commands that filter
SPDX packages by name, version, file name, or download URL.

#### Data Model

N/A — Wildcard is a static class with no instance state.

#### Key Methods

**IsMatch(string, string)**: Returns true if the input string matches the wildcard pattern.

- *Parameters*: `string input` — the text to test; `string pattern` — the glob-style wildcard
  pattern.
- *Returns*: `bool` — true when input matches pattern.
- *Preconditions*: Neither parameter may be null.
- *Post-conditions*: Pure function; no side effects. The match is evaluated with a 100 ms
  regex timeout to prevent catastrophic backtracking.

**WildCardToRegex(string)** (private): Converts a wildcard pattern string to a regular
expression by escaping literal characters and replacing `\*` with `.*` and `\?` with `.`.

- *Parameters*: `string wildPattern` — the wildcard pattern.
- *Returns*: `string` — anchored regular expression string.

#### Error Handling

**RegexMatchTimeoutException** — caught internally by IsMatch when the generated pattern
exceeds the 100 ms evaluation timeout. IsMatch returns false in this case; the exception
is never propagated to callers.

#### Dependencies

- System.Text.RegularExpressions.Regex (IsMatch, Escape, RegexOptions.IgnoreCase)

#### Callers

- FindPackage — calls IsMatch to test each SpdxPackage against the search criteria
- GetVersion — uses FindPackage.FindPackageByCriteria which calls IsMatch
- CopyPackage — calls IsMatch when filtering packages to copy by name
