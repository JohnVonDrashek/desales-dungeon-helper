---
description: Perform comprehensive code analysis on the C# codebase
allowed-tools: ["Read", "Glob", "Grep", "Bash"]
argument-hint: "[file-or-directory]"
---

Analyze the C# codebase for code quality, patterns, and potential improvements.

## Target
$ARGUMENTS

## Analysis Checklist

1. **Architecture Review**
   - Check adherence to SOLID principles
   - Identify dependency injection patterns
   - Review layer separation (if applicable)

2. **Code Quality**
   - Nullable reference type usage
   - Async/await patterns
   - Exception handling practices
   - Disposal patterns (IDisposable, IAsyncDisposable)

3. **Performance Considerations**
   - LINQ usage and potential N+1 queries
   - String concatenation in loops
   - Appropriate use of Span<T> and Memory<T>
   - Collection initialization

4. **Security Review**
   - Input validation
   - SQL injection prevention
   - Secrets handling

5. **Testing Coverage**
   - Test file existence for each class
   - Test naming conventions
   - Assertion quality

Provide actionable recommendations with code examples.
