namespace DeSales.DungeonHelper.Rooms;

/// <summary>
/// Cardinal edge of a room where doors can be placed.
/// </summary>
public enum Edge
{
    North,
    East,
    South,
    West
}

/// <summary>
/// A potential door location on a room edge.
/// </summary>
public readonly struct DoorSlot
{
    /// <summary>
    /// Which edge this door slot is on.
    /// </summary>
    public Edge Edge { get; }

    /// <summary>
    /// Offset along the edge in tiles (0 = corner).
    /// For North/South edges: offset from left.
    /// For East/West edges: offset from top.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Width of the door opening in tiles.
    /// </summary>
    public int Width { get; }

    public DoorSlot(Edge edge, int position, int width)
    {
        Edge = edge;
        Position = position;
        Width = width;
    }

    /// <summary>
    /// Creates a centered door slot on the given edge.
    /// </summary>
    public static DoorSlot Centered(Edge edge, int edgeLength, int width)
    {
        int position = (edgeLength - width) / 2;
        return new DoorSlot(edge, position, width);
    }
}

/// <summary>
/// A self-contained room definition: tiles + door slots + metadata.
/// This is the universal currency between room sources and dungeon assembly.
/// </summary>
public sealed class RoomBlueprint
{
    /// <summary>
    /// Room width in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Room height in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// The tile data. Index as Tiles[x, y].
    /// </summary>
    public int[,] Tiles { get; }

    /// <summary>
    /// Room type: "spawn", "boss", "treasure", "standard", etc.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Where doors CAN be placed. Not all slots need to be used.
    /// </summary>
    public DoorSlot[] DoorSlots { get; }

    /// <summary>
    /// Optional name/identifier for this blueprint (useful for templates).
    /// </summary>
    public string? Name { get; }

    public RoomBlueprint(int width, int height, int[,] tiles, string type, DoorSlot[] doorSlots, string? name = null)
    {
        if (tiles.GetLength(0) != width || tiles.GetLength(1) != height)
        {
            throw new ArgumentException($"Tile array dimensions [{tiles.GetLength(0)},{tiles.GetLength(1)}] don't match specified size [{width},{height}]");
        }

        Width = width;
        Height = height;
        Tiles = tiles;
        Type = type;
        DoorSlots = doorSlots;
        Name = name;
    }

    /// <summary>
    /// Gets door slots on the specified edge.
    /// </summary>
    public IEnumerable<DoorSlot> GetSlotsOnEdge(Edge edge) => DoorSlots.Where(s => s.Edge == edge);

    /// <summary>
    /// Returns true if this blueprint has at least one door slot on the given edge.
    /// </summary>
    public bool HasSlotOnEdge(Edge edge) => DoorSlots.Any(s => s.Edge == edge);

    /// <summary>
    /// Returns true if this blueprint can satisfy all required connections.
    /// </summary>
    public bool CanSatisfyEdges(IEnumerable<Edge> requiredEdges) => requiredEdges.All(HasSlotOnEdge);
}
