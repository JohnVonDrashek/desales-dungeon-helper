namespace MyProject;

/// <summary>
/// Sample class demonstrating project structure.
/// </summary>
public partial class Class1
{
    private readonly ILogger<Class1> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Class1"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public Class1(ILogger<Class1> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sample method that returns a greeting message.
    /// </summary>
    /// <param name="name">The name to greet.</param>
    /// <returns>A greeting message.</returns>
    public string Greet(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        LogGreeting(_logger, name);
        return $"Hello, {name}!";
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Greeting {Name}")]
    private static partial void LogGreeting(ILogger logger, string name);
}
