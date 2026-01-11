# Contributing to DeSales Dungeon Helper

First off, **thank you** for considering contributing! I truly believe in open source and the power of community collaboration. Unlike many repositories, I actively welcome contributions of all kinds - from bug fixes to new features.

## My Promise to Contributors

- **I will respond to every PR and issue** - I guarantee feedback on all contributions
- **Bug fixes are obvious accepts** - If it fixes a bug, it's getting merged
- **New features are welcome** - I'm genuinely open to new ideas and enhancements
- **Direct line of communication** - If I'm not responding to a PR or issue, email me directly at johnvondrashek@gmail.com

## How to Contribute

### Reporting Bugs

1. Check existing issues to avoid duplicates
2. Open a new issue with:
   - Clear, descriptive title
   - Steps to reproduce
   - Expected vs actual behavior
   - Your environment (OS, .NET version, MonoGame version)
   - Sample YAML config if applicable

### Suggesting Features

I'm open to new features! When proposing:

- Explain the problem you're trying to solve
- Describe your proposed solution
- Consider how it fits with Isaac-style dungeon generation
- Bonus points for ASCII mockups of expected output

### Submitting Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run the tests: `dotnet test`
5. Commit with clear messages
6. Push and open a Pull Request

### Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/desales-dungeon-helper.git
cd desales-dungeon-helper

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the sandbox sample
dotnet run --project samples/DeSales.DungeonHelper.Sandbox
```

### Tech Stack

- **.NET 10.0** - Target framework
- **MonoGame 3.8.2+** - Game framework for runtime helpers
- **YamlDotNet** - YAML configuration parsing
- **xUnit** - Test framework
- **FluentAssertions** - Test assertions

### Project Structure

```
desales-dungeon-helper/
  src/
    DeSales.DungeonHelper/        # Main library
  tests/
    DeSales.DungeonHelper.Tests/  # Unit tests
  samples/
    DeSales.DungeonHelper.Sandbox/ # Sample project
```

### Your First Contribution

Look for issues labeled `good first issue` or `help wanted`. Some good starting points:

- Adding new room types
- Improving TMX export options
- Adding validation for YAML configs
- Writing additional tests

Resources for first-time contributors:
- http://makeapullrequest.com/
- https://www.firsttimersonly.com/

## Code Style

This project values bold, elegant solutions over verbose "safe" code. From our engineering philosophy:

- **Density over sprawl** - 50 brilliant lines beat 500 obvious ones
- **Names are documentation** - if you need a comment, you need a better name
- **State is liability** - every variable is a burden; justify each one

Don't be afraid to propose refactors if you see a better way.

## Code of Conduct

This project follows the [Rule of St. Benedict](CODE_OF_CONDUCT.md) as its code of conduct.

## Questions?

- Open an issue
- Email: johnvondrashek@gmail.com

---

*Thank you for helping make procedural dungeon generation better for everyone!*
