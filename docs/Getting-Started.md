# Getting Started

This guide will help you get up and running with MyProject.

## Prerequisites

- .NET 10.0 or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

## Installation

### NuGet Package Manager
```bash
Install-Package MyProject
```

### .NET CLI
```bash
dotnet add package MyProject
```

### PackageReference
Add to your `.csproj` file:
```xml
<PackageReference Include="MyProject" Version="1.0.0" />
```

## Basic Usage

### Step 1: Add the using directive

```csharp
using MyProject;
```

### Step 2: Create an instance

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole());
var logger = loggerFactory.CreateLogger<Class1>();

var instance = new Class1(logger);
```

### Step 3: Use the API

```csharp
var result = instance.Greet("World");
Console.WriteLine(result); // Output: Hello, World!
```

## Dependency Injection

If you're using Microsoft.Extensions.DependencyInjection:

```csharp
services.AddLogging(builder => builder.AddConsole());
services.AddTransient<Class1>();
```

Then inject it:

```csharp
public class MyService
{
    private readonly Class1 _class1;

    public MyService(Class1 class1)
    {
        _class1 = class1;
    }
}
```

## Next Steps

- [Configuration](Configuration) - Learn about configuration options
- [API Reference](API-Reference) - Explore the full API
