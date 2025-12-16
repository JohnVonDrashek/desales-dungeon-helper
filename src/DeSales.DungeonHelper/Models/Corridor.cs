namespace DeSales.DungeonHelper.Models;

/// <summary>
/// Represents a corridor connecting rooms in the dungeon.
/// </summary>
public class Corridor
{
    private readonly List<int> _connectedRoomIds = [];

    /// <summary>
    /// Gets the starting point of the corridor.
    /// </summary>
    public Point Start { get; }

    /// <summary>
    /// Gets the ending point of the corridor.
    /// </summary>
    public Point End { get; }

    /// <summary>
    /// Gets the width of the corridor in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the IDs of rooms connected by this corridor.
    /// </summary>
    public IReadOnlyList<int> ConnectedRoomIds => _connectedRoomIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="Corridor"/> class.
    /// </summary>
    /// <param name="start">The starting point.</param>
    /// <param name="end">The ending point.</param>
    /// <param name="width">The width of the corridor in tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when width is less than 1.</exception>
    public Corridor(Point start, Point end, int width)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 1.");
        }

        Start = start;
        End = end;
        Width = width;
    }

    /// <summary>
    /// Records that this corridor connects the specified rooms.
    /// </summary>
    /// <param name="roomId1">The first room ID.</param>
    /// <param name="roomId2">The second room ID.</param>
    public void ConnectRooms(int roomId1, int roomId2)
    {
        if (!_connectedRoomIds.Contains(roomId1))
        {
            _connectedRoomIds.Add(roomId1);
        }

        if (!_connectedRoomIds.Contains(roomId2))
        {
            _connectedRoomIds.Add(roomId2);
        }
    }

    /// <summary>
    /// Gets all points along the corridor path.
    /// Uses an L-shaped path (horizontal then vertical).
    /// </summary>
    /// <returns>An enumerable of points along the corridor.</returns>
    public IEnumerable<Point> GetPath()
    {
        // Generate L-shaped corridor: horizontal first, then vertical
        var current = Start;

        // Horizontal segment
        var xDirection = End.X > Start.X ? 1 : -1;
        while (current.X != End.X)
        {
            yield return current;
            current = new Point(current.X + xDirection, current.Y);
        }

        // Vertical segment
        var yDirection = End.Y > Start.Y ? 1 : -1;
        while (current.Y != End.Y)
        {
            yield return current;
            current = new Point(current.X, current.Y + yDirection);
        }

        // Include the end point
        yield return End;
    }
}
