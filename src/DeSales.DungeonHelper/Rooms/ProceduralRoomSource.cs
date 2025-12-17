using DeSales.DungeonHelper.Configuration;

namespace DeSales.DungeonHelper.Rooms;

/// <summary>
/// Generates simple rectangle rooms procedurally.
/// Produces rooms with floor interiors, wall borders, and centered door slots on all edges.
/// </summary>
public sealed class ProceduralRoomSource : IRoomSource
{
    private readonly int _cellSize;
    private readonly TilesConfig _tiles;
    private readonly int _doorWidth;

    public ProceduralRoomSource(int cellSize, TilesConfig tiles, int doorWidth = 1)
    {
        _cellSize = cellSize;
        _tiles = tiles;
        _doorWidth = doorWidth;
    }

    public bool CanProvide(string roomType) => true; // Can generate any type

    public IEnumerable<RoomBlueprint> GetBlueprints(string roomType, Random random)
    {
        // Infinite stream of procedurally generated rooms
        while (true)
        {
            yield return Generate(roomType);
        }
    }

    /// <summary>
    /// Generates a single room blueprint of the given type.
    /// </summary>
    public RoomBlueprint Generate(string roomType)
    {
        var tiles = new int[_cellSize, _cellSize];

        // Fill interior with floor
        for (int y = 1; y < _cellSize - 1; y++)
        {
            for (int x = 1; x < _cellSize - 1; x++)
            {
                tiles[x, y] = _tiles.Floor;
            }
        }

        // Draw walls on all edges
        for (int d = 0; d < _cellSize; d++)
        {
            tiles[d, 0] = _tiles.Wall;                  // Top
            tiles[d, _cellSize - 1] = _tiles.Wall;     // Bottom
            tiles[0, d] = _tiles.Wall;                  // Left
            tiles[_cellSize - 1, d] = _tiles.Wall;     // Right
        }

        // Create centered door slots on all four edges
        var doorSlots = new[]
        {
            DoorSlot.Centered(Edge.North, _cellSize, _doorWidth),
            DoorSlot.Centered(Edge.East, _cellSize, _doorWidth),
            DoorSlot.Centered(Edge.South, _cellSize, _doorWidth),
            DoorSlot.Centered(Edge.West, _cellSize, _doorWidth)
        };

        return new RoomBlueprint(_cellSize, _cellSize, tiles, roomType, doorSlots);
    }
}
