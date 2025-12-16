---
description: Guide through a TDD Red-Green-Refactor cycle
allowed-tools: ["Read", "Write", "Glob", "Grep", "Bash"]
argument-hint: "<feature-or-requirement>"
---

Guide through a complete Test-Driven Development cycle for the specified feature.

## Feature/Requirement
$ARGUMENTS

## TDD Process: Red-Green-Refactor

### Phase 1: RED - Write a Failing Test First

Before writing ANY implementation code:

1. **Understand the requirement** - What behavior needs to be implemented?
2. **Write the test first** - Create a test that describes the expected behavior
3. **Run the test** - Verify it FAILS (this is expected and correct!)

```csharp
[Fact]
public void MethodName_WhenCondition_ShouldExpectedBehavior()
{
    // Arrange - Set up the test scenario

    // Act - Execute the behavior being tested

    // Assert - Verify the expected outcome
}
```

**Run:** `dotnet test --filter "FullyQualifiedName~TestMethodName"`

The test MUST fail before proceeding. If it passes, either:
- The feature already exists
- The test is not testing the right thing

### Phase 2: GREEN - Write Minimal Code to Pass

1. **Write the simplest implementation** that makes the test pass
2. **Don't over-engineer** - Just make it work
3. **Run the test** - Verify it PASSES

```bash
dotnet test
```

Rules for GREEN phase:
- Write only enough code to pass the test
- It's okay if the code is "ugly" - we'll fix it in refactor
- Don't add features not covered by tests

### Phase 3: REFACTOR - Improve the Code

Now that tests pass, improve the code quality:

1. **Remove duplication**
2. **Improve naming**
3. **Extract methods/classes if needed**
4. **Run tests after each change** - They must stay green!

```bash
dotnet test
```

### Repeat the Cycle

For each new behavior:
1. Write a failing test (RED)
2. Make it pass (GREEN)
3. Clean up (REFACTOR)

## TDD Best Practices

### Test Naming
Use descriptive names that document behavior:
- `MethodName_StateUnderTest_ExpectedBehavior`
- `Should_ExpectedBehavior_When_StateUnderTest`

### One Assert Per Test
Each test should verify ONE behavior. Multiple assertions are okay if they verify the same logical concept.

### Test Independence
Tests should not depend on each other or share state.

### FIRST Principles
- **F**ast - Tests should run quickly
- **I**ndependent - No dependencies between tests
- **R**epeatable - Same result every time
- **S**elf-validating - Pass or fail, no manual checking
- **T**imely - Written before or with the code

## Commands

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode (re-run on changes)
dotnet watch test --project tests/MyProject.Tests
```
