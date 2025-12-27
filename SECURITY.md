# Security Policy

## Supported Versions

We release patches for security vulnerabilities. Only the latest version is currently being supported with security updates.

| Version  | Supported          |
| -------- | ------------------ |
| Latest   | :white_check_mark: |
| < Latest | :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability within this project, please use GitHub's vulnerability reporting feature:

1. Go to the [Security tab](https://github.com/demaconsulting/SpdxTool/security) of this repository
2. Click on "Report a vulnerability"
3. Fill out the vulnerability report form with details about the issue

We take all security reports seriously and will respond to your report as quickly as possible. Please do not
publicly disclose the vulnerability until we have had a chance to address it.

## What to Expect

After submitting a vulnerability report, you can expect:

- **Initial Response**: We will acknowledge receipt of your report within 48 hours
- **Status Updates**: We will keep you informed about the progress of addressing the vulnerability
- **Resolution Timeline**: We aim to release a fix within 90 days of the initial report, depending on complexity
- **Credit**: We will credit you in the release notes (unless you prefer to remain anonymous)

## Security Update Process

Our security update process follows these steps:

1. **Triage**: We assess the severity and impact of the reported vulnerability
2. **Development**: We develop and test a fix in a private repository
3. **Release**: We release a patched version as soon as possible
4. **Disclosure**: We publish security advisories after the patch is released
5. **Communication**: We notify users through release notes and GitHub security advisories

## Security Best Practices

When using SpdxTool, we recommend:

- Always use the latest version to benefit from security updates
- Validate SPDX documents from untrusted sources before processing
- Run SpdxTool in sandboxed environments when processing untrusted input
- Review generated SPDX documents before publishing or sharing them
- Keep your .NET runtime updated to the latest version

## Input Validation

SpdxTool implements several input validation measures:

- JSON and YAML parsing with schema validation
- File path sanitization to prevent directory traversal attacks
- Input size limits to prevent resource exhaustion
- Validation of SPDX specification compliance

## Security Tools Used

This project uses multiple security tools to maintain code quality:

- **SonarCloud**: Continuous security analysis and code quality checks
- **CodeQL**: Automated security vulnerability scanning in CI/CD
- **Dependency Scanning**: Automated checks for vulnerable dependencies
- **Static Analysis**: Microsoft.CodeAnalysis.NetAnalyzers and SonarAnalyzer.CSharp

## Responsible Disclosure

We follow responsible disclosure practices:

- We will work with you to understand and address the vulnerability
- We request that you do not publicly disclose the vulnerability until we have released a fix
- We will coordinate with you on the disclosure timeline
- We will provide credit for your responsible disclosure

## Security Hall of Fame

We recognize and thank security researchers who help improve our project's security:

*No security vulnerabilities have been reported yet.*

## Contact

For security-related questions or concerns that are not vulnerabilities, you can:

- Open a discussion in [GitHub Discussions](https://github.com/demaconsulting/SpdxTool/discussions)
- Contact the maintainers through the repository

## Additional Resources

- [SPDX Security Specification](https://spdx.github.io/spdx-spec/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE - Common Weakness Enumeration](https://cwe.mitre.org/)
- [GitHub Security Best Practices](https://docs.github.com/en/code-security)

Thank you for helping keep this project and its users safe!
