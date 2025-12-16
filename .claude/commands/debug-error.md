---
description: Systematically debug and fix errors
allowed-tools: ["Read", "Write", "Glob", "Grep", "Bash"]
argument-hint: "<error-message-or-file>"
---

Debug and resolve the specified error systematically.

## Error Context
$ARGUMENTS

## Debugging Process

1. **Identify Error Type**
   - Compilation error (CS####)
   - Runtime exception
   - Test failure
   - Build/publish error

2. **Gather Context**
   - Read the affected file(s)
   - Check related types and dependencies
   - Review recent changes with `git diff`

3. **Root Cause Analysis**
   - Trace the error to its source
   - Check for common C# pitfalls:
     - Null reference issues
     - Type mismatches
     - Missing using directives
     - Async deadlocks
     - Disposal issues

4. **Implement Fix**
   - Apply minimal, targeted changes
   - Maintain existing patterns
   - Add defensive coding if appropriate

5. **Verify**
   - Run `dotnet build` to verify compilation
   - Run affected tests with `dotnet test`
   - Check for any new warnings

## Common Error Patterns

### CS8600-CS8605 (Nullability)
Check nullable reference type annotations and add null checks.

### CS0103 (Name does not exist)
Add missing using directive or check for typos.

### CS1061 (Member not found)
Verify the type has the expected member, check for extension method namespaces.

### Test Failures
Review test assertions, check for race conditions in async tests.
