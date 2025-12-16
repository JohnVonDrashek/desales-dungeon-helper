namespace DeSales.DungeonHelper.Models;

/// <summary>
/// Represents a room in the dungeon.
/// </summary>
public class Room
{
    private readonly List<Point> _doors = [];

    /// <summary>
    /// Gets the bounding rectangle of the room.
    /// </summary>
    public Rectangle Bounds { get; }

    /// <summary>
    /// Gets the type of the room.
    /// </summary>
    public RoomType Type { get; }

    /// <summary>
    /// Gets the center point of the room.
    /// </summary>
    public Point Center => new(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);

    /// <summary>
    /// Gets the door positions for this room.
    /// </summary>
    public IReadOnlyList<Point> Doors => _doors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Room"/> class.
    /// </summary>
    /// <param name="bounds">The bounding rectangle of the room.</param>
    /// <param name="type">The type of the room.</param>
    public Room(Rectangle bounds, RoomType type)
    {
        Bounds = bounds;
        Type = type;
    }

    /// <summary>
    /// Adds a door at the specified position.
    /// </summary>
    /// <param name="position">The position of the door.</param>
    public void AddDoor(Point position)
    {
        _doors.Add(position);
    }

    /// <summary>
    /// Checks if the room contains the specified point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>True if the room contains the point, false otherwise.</returns>
    public bool Contains(Point point)
    {
        return Bounds.Contains(point);
    }
}
