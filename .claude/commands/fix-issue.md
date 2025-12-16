---
description: Analyze and fix a GitHub issue
allowed-tools: ["Read", "Write", "Glob", "Grep", "Bash"]
argument-hint: "<issue-number>"
---

Analyze and implement a fix for GitHub issue #$ARGUMENTS.

## Process

1. **Fetch Issue Details**
   ```bash
   gh issue view $ARGUMENTS
   ```

2. **Understand Requirements**
   - Read issue description and comments
   - Identify acceptance criteria
   - Check for related issues or PRs

3. **Locate Relevant Code**
   - Find affected files
   - Understand current implementation
   - Identify test coverage

4. **Implement Solution**
   - Follow existing code patterns
   - Add appropriate tests
   - Update documentation if needed

5. **Prepare for Review**
   - Run full test suite: `dotnet test`
   - Check code formatting: `dotnet format --verify-no-changes`
   - Reference issue in commit message

## Commit Message Format
```
fix: Brief description (#ISSUE_NUMBER)

- Detailed change 1
- Detailed change 2

Fixes #ISSUE_NUMBER
```

## Branch Naming Convention
```
fix/issue-{number}-brief-description
feature/issue-{number}-brief-description
```
