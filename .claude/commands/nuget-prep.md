---
description: Prepare project for NuGet package publication
allowed-tools: ["Read", "Write", "Glob", "Grep", "Bash"]
argument-hint: "[version]"
---

Prepare the project for NuGet package release.

## Version
$ARGUMENTS

## Checklist

### 1. Update Version Information
- Update version in Directory.Build.props or .csproj
- Follow semantic versioning (MAJOR.MINOR.PATCH)
  - MAJOR: Breaking changes
  - MINOR: New features, backward compatible
  - PATCH: Bug fixes, backward compatible

### 2. Update CHANGELOG.md
```markdown
## [x.y.z] - YYYY-MM-DD

### Added
- New features

### Changed
- Changes in existing functionality

### Fixed
- Bug fixes

### Removed
- Removed features

### Security
- Security fixes
```

### 3. Verify Package Metadata in Directory.Build.props
- [ ] PackageId
- [ ] Description
- [ ] Authors
- [ ] PackageProjectUrl
- [ ] RepositoryUrl
- [ ] PackageTags
- [ ] PackageLicenseExpression
- [ ] PackageReadmeFile
- [ ] PackageIcon

### 4. Quality Checks
```bash
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release
```

### 5. Validate Package Contents
```bash
# List package contents
unzip -l src/MyProject/bin/Release/*.nupkg
```

### 6. Create Release
After merging to main, create a GitHub Release with tag `vX.Y.Z` to trigger the NuGet publish workflow.
