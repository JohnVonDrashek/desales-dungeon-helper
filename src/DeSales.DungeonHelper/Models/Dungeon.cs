namespace DeSales.DungeonHelper.Models;

/// <summary>
/// Represents a generated dungeon with rooms, corridors, and a tile map.
/// </summary>
public class Dungeon
{
    private readonly List<Room> _rooms = [];
    private readonly List<Corridor> _corridors = [];

    /// <summary>
    /// Gets the name of the dungeon.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the width of the dungeon in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the dungeon in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the seed used for generation.
    /// </summary>
    public int Seed { get; }

    /// <summary>
    /// Gets the tile map of the dungeon.
    /// </summary>
    public TileMap TileMap { get; }

    /// <summary>
    /// Gets all rooms in the dungeon.
    /// </summary>
    public IReadOnlyList<Room> Rooms => _rooms;

    /// <summary>
    /// Gets all corridors in the dungeon.
    /// </summary>
    public IReadOnlyList<Corridor> Corridors => _corridors;

    /// <summary>
    /// Gets the spawn room, or null if none exists.
    /// </summary>
    public Room? SpawnRoom => _rooms.FirstOrDefault(r => r.Type == RoomType.Spawn);

    /// <summary>
    /// Gets the boss room, or null if none exists.
    /// </summary>
    public Room? BossRoom => _rooms.FirstOrDefault(r => r.Type == RoomType.Boss);

    /// <summary>
    /// Initializes a new instance of the <see cref="Dungeon"/> class.
    /// </summary>
    /// <param name="name">The name of the dungeon.</param>
    /// <param name="width">The width in tiles.</param>
    /// <param name="height">The height in tiles.</param>
    /// <param name="seed">The seed used for generation.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dimensions are invalid.</exception>
    public Dungeon(string name, int width, int height, int seed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be at least 1.");
        }

        Name = name;
        Width = width;
        Height = height;
        Seed = seed;
        TileMap = new TileMap(width, height);
    }

    /// <summary>
    /// Adds a room to the dungeon.
    /// </summary>
    /// <param name="room">The room to add.</param>
    public void AddRoom(Room room)
    {
        _rooms.Add(room);
    }

    /// <summary>
    /// Adds a corridor to the dungeon.
    /// </summary>
    /// <param name="corridor">The corridor to add.</param>
    public void AddCorridor(Corridor corridor)
    {
        _corridors.Add(corridor);
    }

    /// <summary>
    /// Gets the room at the specified point, or null if no room contains that point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>The room containing the point, or null.</returns>
    public Room? GetRoomAt(Point point)
    {
        return _rooms.FirstOrDefault(r => r.Contains(point));
    }
}
