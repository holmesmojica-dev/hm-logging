# Versioning Strategy

Hm.Logging follows Semantic Versioning (SemVer).

Version format:

`MAJOR.MINOR.PATCH`

Example:

1.2.3

## Versioning Rules

### MAJOR

Incremented when introducing breaking changes to the public API.

Examples:
- Removing public members
- Changing method signatures
- Behavioral breaking changes
- Removing providers or contracts

### MINOR

Incremented when introducing new backward-compatible features.

Examples:
- New providers
- Additional logging capabilities
- New configuration options
- New extension methods

### PATCH

Incremented for backward-compatible fixes and internal improvements.

Examples:
- Bug fixes
- Performance improvements
- Internal refactors
- Documentation corrections

---

## Pre-release Versions

Before reaching stable production maturity, pre-release versions may be published using suffixes such as:

`0.1.0-preview.1`

Pre-release versions may introduce API adjustments based on architectural improvements and community feedback.

---

## Compatibility Policy

Minor and patch releases aim to preserve backward compatibility whenever possible.

Breaking changes are reserved for major releases.

---

## Version Source

The package version is currently managed directly through the project `.csproj` file.

---

## Initial Release Strategy

The project currently targets early pre-release distribution until the public API and provider ecosystem stabilize.