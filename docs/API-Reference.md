# API Reference

Complete API documentation for MyProject.

## Namespace: MyProject

### Class1

Main class providing core functionality.

#### Constructor

```csharp
public Class1(ILogger<Class1> logger)
```

**Parameters:**
- `logger` - The logger instance for diagnostic output

**Exceptions:**
- `ArgumentNullException` - Thrown when logger is null

#### Methods

##### Greet

Returns a greeting message for the specified name.

```csharp
public string Greet(string name)
```

**Parameters:**
- `name` - The name to greet (cannot be null or whitespace)

**Returns:**
- A greeting message in the format "Hello, {name}!"

**Exceptions:**
- `ArgumentException` - Thrown when name is null, empty, or whitespace

**Example:**
```csharp
var instance = new Class1(logger);
var result = instance.Greet("Alice");
// result: "Hello, Alice!"
```

---

## Extension Methods

*No extension methods defined yet.*

---

## Interfaces

*No public interfaces defined yet.*

---

## Enums

*No enums defined yet.*
