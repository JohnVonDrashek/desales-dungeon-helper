---
description: Perform comprehensive pull request review
allowed-tools: ["Read", "Glob", "Grep", "Bash"]
argument-hint: "<pr-number>"
---

Review pull request #$ARGUMENTS comprehensively.

## Review Process

1. **Fetch PR Information**
   ```bash
   gh pr view $ARGUMENTS
   gh pr diff $ARGUMENTS
   ```

2. **Code Review Checklist**

### Correctness
- [ ] Logic is correct and handles edge cases
- [ ] No obvious bugs or regressions
- [ ] Proper error handling
- [ ] Thread safety considered where applicable

### Code Quality
- [ ] Follows C# conventions and project patterns
- [ ] No code duplication
- [ ] Appropriate use of LINQ (readable, not over-chained)
- [ ] Proper async/await usage (no sync-over-async)
- [ ] Meaningful variable and method names

### Testing
- [ ] Adequate test coverage for new code
- [ ] Tests are meaningful (not just for coverage)
- [ ] Edge cases are tested
- [ ] Tests follow Arrange-Act-Assert pattern

### Security
- [ ] No hardcoded secrets or credentials
- [ ] Input validation present at boundaries
- [ ] No injection vulnerabilities (SQL, command, etc.)
- [ ] Sensitive data not logged

### Performance
- [ ] No obvious performance issues
- [ ] Appropriate data structures used
- [ ] No unnecessary allocations in hot paths
- [ ] Database queries are efficient

### Documentation
- [ ] Public APIs have XML documentation
- [ ] Complex logic is explained in comments
- [ ] CHANGELOG updated if user-facing change
- [ ] README updated if needed

3. **Provide Feedback**
   - Be specific and constructive
   - Suggest improvements with code examples
   - Mark as: Required | Suggestion | Question
