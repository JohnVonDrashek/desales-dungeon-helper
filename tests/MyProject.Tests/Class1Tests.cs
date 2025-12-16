using Microsoft.Extensions.Logging;

namespace MyProject.Tests;

public class Class1Tests
{
    private readonly ILogger<Class1> _logger;
    private readonly Class1 _sut;

    public Class1Tests()
    {
        _logger = Substitute.For<ILogger<Class1>>();
        _sut = new Class1(_logger);
    }

    [Fact]
    public void Greet_WithValidName_ReturnsGreetingMessage()
    {
        // Arrange
        var name = "World";

        // Act
        var result = _sut.Greet(name);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob")]
    [InlineData("Charlie")]
    public void Greet_WithVariousNames_ReturnsCorrectGreeting(string name)
    {
        // Act
        var result = _sut.Greet(name);

        // Assert
        result.Should().Be($"Hello, {name}!");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Greet_WithNullOrWhitespaceName_ThrowsArgumentException(string? name)
    {
        // Act
        var act = () => _sut.Greet(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new Class1(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
