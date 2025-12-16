---
description: Scaffold a new feature with all necessary files
allowed-tools: ["Read", "Write", "Glob", "Grep", "Bash"]
argument-hint: "<feature-name> [--type=class|service|controller]"
---

Create a new feature using Test-Driven Development (TDD) approach.

## Feature Name
$ARGUMENTS

## TDD-First Approach

**IMPORTANT: Always create tests BEFORE implementation code!**

### Step 1: Create the Test File First
Create the test class in `tests/MyProject.Tests/` with tests that define the expected behavior.

### Step 2: Run Tests - Verify They Fail (RED)
```bash
dotnet test
```
Tests MUST fail initially - this confirms we're testing new behavior.

### Step 3: Create Minimal Implementation (GREEN)
Create the class in `src/MyProject/` with just enough code to pass tests.

### Step 4: Refactor
Clean up the code while keeping tests green.

## File Creation Order (TDD)

1. **Test file first**: `tests/MyProject.Tests/{FeatureName}Tests.cs`
2. **Implementation second**: `src/MyProject/{FeatureName}.cs`
3. **Interface (if needed)**: `src/MyProject/I{FeatureName}.cs`

## Test File Template
```csharp
namespace MyProject.Tests;

public class {FeatureName}Tests
{
    [Fact]
    public void Method_WhenCondition_ShouldExpectedBehavior()
    {
        // Arrange

        // Act

        // Assert
    }
}
```

## Implementation Template
```csharp
namespace MyProject;

/// <summary>
/// Description of the feature.
/// </summary>
public class {FeatureName}
{
    // Implement only what tests require
}
```

## Conventions
- PascalCase for public members
- camelCase with underscore prefix for private fields (_privateField)
- Async suffix for async methods
- I prefix for interfaces
- Tests named: Method_Scenario_ExpectedBehavior

## Example Output Structure
```
tests/MyProject.Tests/
  └── {FeatureName}Tests.cs    <- Created FIRST
src/MyProject/
  └── {FeatureName}.cs         <- Created SECOND
```
