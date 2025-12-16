---
description: Update project documentation
allowed-tools: ["Read", "Write", "Glob", "Grep", "Bash"]
argument-hint: "[area: api|readme|wiki|all]"
---

Update documentation for the project.

## Area
$ARGUMENTS (default: all)

## Documentation Tasks

### 1. README.md
Ensure it includes:
- Project description and purpose
- Installation instructions (NuGet, source)
- Quick start code example
- Configuration options
- Link to full documentation (wiki)
- Contributing guidelines link
- License information

### 2. API Documentation
For public types and members, ensure XML docs include:
```csharp
/// <summary>
/// Brief description of the type/member.
/// </summary>
/// <param name="paramName">Description of parameter.</param>
/// <returns>Description of return value.</returns>
/// <exception cref="ArgumentException">When this is thrown.</exception>
/// <example>
/// <code>
/// var result = instance.Method("input");
/// </code>
/// </example>
```

### 3. Wiki Pages (docs/ folder)
Update these pages as needed:
- **Home.md** - Project overview and quick links
- **Getting-Started.md** - Installation and first steps
- **Configuration.md** - All configuration options
- **API-Reference.md** - Full API documentation
- **Contributing.md** - How to contribute

### 4. CHANGELOG.md
Ensure recent changes are documented following Keep a Changelog format.

### 5. Sync Check
After updating docs/, changes will auto-sync to GitHub Wiki on push to main.
