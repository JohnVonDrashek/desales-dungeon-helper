namespace DeSales.DungeonHelper.Tiled;

/// <summary>
/// Represents an object group (layer) in a TMX map.
/// Contains objects like spawn points, room boundaries, etc.
/// </summary>
public class TmxObjectGroup
{
    private readonly List<TmxObject> _objects = [];
    private int _nextId = 1;

    /// <summary>
    /// Gets the name of the object group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets all objects in this group.
    /// </summary>
    public IReadOnlyList<TmxObject> Objects => _objects;

    /// <summary>
    /// Initializes a new object group.
    /// </summary>
    public TmxObjectGroup(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Adds a point object (e.g., spawn location).
    /// </summary>
    public TmxObject AddObject(string name, string type, float x, float y)
    {
        var obj = new TmxObject(_nextId++, name, type, x, y);
        _objects.Add(obj);
        return obj;
    }

    /// <summary>
    /// Adds a rectangle object (e.g., room bounds).
    /// </summary>
    public TmxObject AddObject(string name, string type, float x, float y, float width, float height)
    {
        var obj = new TmxObject(_nextId++, name, type, x, y, width, height);
        _objects.Add(obj);
        return obj;
    }

    /// <summary>
    /// Gets an object by name, or null if not found.
    /// </summary>
    public TmxObject? GetObject(string name)
    {
        return _objects.FirstOrDefault(o => o.Name == name);
    }

    /// <summary>
    /// Gets all objects of a specific type.
    /// </summary>
    public IEnumerable<TmxObject> GetObjectsByType(string type)
    {
        return _objects.Where(o => o.Type == type);
    }

    /// <summary>
    /// Sets the next ID to use (used when loading from file).
    /// </summary>
    internal void SetNextId(int nextId)
    {
        _nextId = nextId;
    }

    /// <summary>
    /// Adds an existing object (used when loading from file).
    /// </summary>
    internal void AddExistingObject(TmxObject obj)
    {
        _objects.Add(obj);
        if (obj.Id >= _nextId)
        {
            _nextId = obj.Id + 1;
        }
    }
}
