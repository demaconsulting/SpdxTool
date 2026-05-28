# Troubleshooting

## Common Issues

### Invalid SPDX Document

**Problem**: Validation fails with schema errors.

**Solution**:

- Ensure the SPDX document conforms to the SPDX 2.3 specification
- Check that all required fields are present
- Verify JSON syntax is correct
- Use the `validate` command to get detailed error messages

### Package Not Found

**Problem**: `find-package` or `get-version` commands cannot locate a package.

**Solution**:

- Verify the package exists in the SPDX document
- Check that search criteria match exactly (case-sensitive)
- Use multiple criteria to narrow the search
- List all packages with `to-markdown` to see available packages

### Workflow Variable Not Expanding

**Problem**: Variables in workflow files are not being replaced.

**Solution**:

- Ensure variable syntax is correct: `${{ variable-name }}`
- Check that the variable is defined in parameters or set by a previous step
- Verify the output parameter name matches the variable name
- Variables are case-sensitive

### Permission Errors

**Problem**: Cannot write SPDX document or output files.

**Solution**:

- Ensure the output directory exists
- Verify write permissions on the output directory
- Check disk space availability
- Use an absolute path or verify the working directory

## Debug Mode

Enable detailed logging for troubleshooting:

```bash
dotnet spdx-tool --log debug.log <command> <arguments>
```

This provides detailed information about:

- Command execution
- File operations
- Variable expansion
- Error stack traces

## SPDX Specification

SpdxTool supports the SPDX 2.3 specification. For full details see the
[SPDX Specification][spdx-spec] and the [SPDX GitHub repository][spdx-github].

## NTIA Minimum Elements

The NTIA minimum elements for SBOM include:

- Author name
- Timestamp
- Component name
- Version string
- Component identifiers (PURL, CPE)
- Dependency relationships
- Author of SBOM data

For more information see the
[NTIA SBOM Minimum Elements report][ntia-sbom].

## Version History

See the [SpdxTool releases page][releases] for detailed version history and release notes.

## License

SpdxTool is licensed under the MIT License. See the [LICENSE file][license] for details.

## Contributing

Contributions are welcome. See the [Contributing Guidelines][contributing] for details on setting up
a development environment, coding standards, and submitting pull requests.

## Support

For issues, questions, or feature requests:

- **GitHub Issues**: <https://github.com/demaconsulting/SpdxTool/issues>
- **NuGet**: <https://www.nuget.org/packages/DemaConsulting.SpdxTool>

## Additional Resources

- **SPDX Official Site**: <https://spdx.dev/>
- **Microsoft SBOM Tool**: <https://github.com/microsoft/sbom-tool>
- **.NET Tool Documentation**: <https://learn.microsoft.com/dotnet/core/tools/global-tools>
- **SPDX Model Library**: <https://github.com/demaconsulting/SpdxModel>
- **SPDX Workflows Library**: <https://github.com/demaconsulting/SpdxWorkflows>

[spdx-spec]: https://spdx.github.io/spdx-spec/
[spdx-github]: https://github.com/spdx/spdx-spec
[ntia-sbom]: https://www.ntia.gov/files/ntia/publications/sbom_minimum_elements_report.pdf
[releases]: https://github.com/demaconsulting/SpdxTool/releases
[license]: https://github.com/demaconsulting/SpdxTool/blob/main/LICENSE
[contributing]: https://github.com/demaconsulting/SpdxTool/blob/main/CONTRIBUTING.md
