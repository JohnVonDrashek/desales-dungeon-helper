namespace DeSales.DungeonHelper.Tiled;

/// <summary>
/// Represents an object in a TMX object layer.
/// Can be a point (spawn location) or rectangle (room bounds).
/// </summary>
public class TmxObject
{
    private readonly Dictionary<string, string> _properties = [];

    /// <summary>
    /// Gets the unique ID of the object.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of the object.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the object (e.g., "spawn", "boss", "room").
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the X position in pixels.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the Y position in pixels.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Gets the width in pixels (null for point objects).
    /// </summary>
    public float? Width { get; }

    /// <summary>
    /// Gets the height in pixels (null for point objects).
    /// </summary>
    public float? Height { get; }

    /// <summary>
    /// Gets whether this is a point object (no width/height).
    /// </summary>
    public bool IsPoint => Width is null || Height is null;

    /// <summary>
    /// Gets the position as a Vector2.
    /// </summary>
    public Vector2 Position => new(X, Y);

    /// <summary>
    /// Gets the bounds as a Rectangle.
    /// For point objects, returns a zero-size rectangle at the position.
    /// </summary>
    public Rectangle Bounds => new(
        (int)X,
        (int)Y,
        (int)(Width ?? 0),
        (int)(Height ?? 0));

    /// <summary>
    /// Initializes a new point object.
    /// </summary>
    public TmxObject(int id, string name, string type, float x, float y)
    {
        Id = id;
        Name = name;
        Type = type;
        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new rectangle object.
    /// </summary>
    public TmxObject(int id, string name, string type, float x, float y, float width, float height)
        : this(id, name, type, x, y)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Sets a custom property on this object.
    /// </summary>
    public void SetProperty(string name, string value)
    {
        _properties[name] = value;
    }

    /// <summary>
    /// Gets a custom property value, or null if not found.
    /// </summary>
    public string? GetProperty(string name)
    {
        return _properties.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Gets all custom properties.
    /// </summary>
    public IReadOnlyDictionary<string, string> Properties => _properties;
}
