using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Rooms;
using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Generation;

/// <summary>
/// Represents a placed room in the dungeon layout.
/// </summary>
public sealed class PlacedRoom
{
    /// <summary>
    /// Grid position (not tile position).
    /// </summary>
    public int GridX { get; }
    public int GridY { get; }

    /// <summary>
    /// The room blueprint.
    /// </summary>
    public RoomBlueprint Blueprint { get; }

    /// <summary>
    /// Adjacent rooms by edge.
    /// </summary>
    public Dictionary<Edge, PlacedRoom> Neighbors { get; } = [];

    public PlacedRoom(int gridX, int gridY, RoomBlueprint blueprint)
    {
        GridX = gridX;
        GridY = gridY;
        Blueprint = blueprint;
    }

    /// <summary>
    /// Links this room to a neighbor on the specified edge.
    /// </summary>
    public void LinkNeighbor(Edge edge, PlacedRoom neighbor)
    {
        Neighbors[edge] = neighbor;
        neighbor.Neighbors[OppositeEdge(edge)] = this;
    }

    private static Edge OppositeEdge(Edge e) => e switch
    {
        Edge.North => Edge.South,
        Edge.South => Edge.North,
        Edge.East => Edge.West,
        Edge.West => Edge.East,
        _ => throw new ArgumentOutOfRangeException(nameof(e))
    };
}

/// <summary>
/// Assembles room blueprints into a final TmxMap.
/// Handles tile composition, door placement, and metadata generation.
/// </summary>
public sealed class DungeonAssembler
{
    private const int DefaultTileSize = 16;

    private readonly TilesConfig _tiles;
    private readonly int _cellSize;
    private readonly int _doorWidth;

    public DungeonAssembler(TilesConfig tiles, int cellSize, int doorWidth)
    {
        _tiles = tiles;
        _cellSize = cellSize;
        _doorWidth = doorWidth;
    }

    /// <summary>
    /// Assembles placed rooms into a TmxMap.
    /// </summary>
    public TmxMap Assemble(IReadOnlyList<PlacedRoom> rooms, int gridWidth, int gridHeight, bool voidExterior)
    {
        int mapWidth = gridWidth * _cellSize;
        int mapHeight = gridHeight * _cellSize;

        var map = new TmxMap(mapWidth, mapHeight, DefaultTileSize, DefaultTileSize);
        var tileLayer = map.AddTileLayer("Tiles");
        var roomsGroup = map.AddObjectGroup("Rooms");
        var spawnsGroup = map.AddObjectGroup("Spawns");

        // Pre-fill with walls if not void exterior
        if (!voidExterior)
        {
            FillWithWalls(tileLayer);
        }

        // Place each room's tiles
        foreach (var room in rooms)
        {
            PlaceRoomTiles(tileLayer, room);
            AddRoomObject(roomsGroup, room);
        }

        // Place doors between connected rooms
        PlaceAllDoors(tileLayer, rooms);

        // For void exterior, add walls around floors
        if (voidExterior)
        {
            AddWallsAroundFloors(tileLayer);
        }

        // Add spawn points
        AddSpawnPoints(rooms, spawnsGroup);

        return map;
    }

    private void FillWithWalls(TmxTileLayer layer)
    {
        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                layer[x, y] = _tiles.Wall;
            }
        }
    }

    private void PlaceRoomTiles(TmxTileLayer layer, PlacedRoom room)
    {
        int offsetX = room.GridX * _cellSize;
        int offsetY = room.GridY * _cellSize;
        var bp = room.Blueprint;

        for (int y = 0; y < bp.Height; y++)
        {
            for (int x = 0; x < bp.Width; x++)
            {
                layer[offsetX + x, offsetY + y] = bp.Tiles[x, y];
            }
        }
    }

    private void PlaceAllDoors(TmxTileLayer layer, IReadOnlyList<PlacedRoom> rooms)
    {
        var processed = new HashSet<(PlacedRoom, PlacedRoom)>();

        foreach (var room in rooms)
        {
            foreach (var (edge, neighbor) in room.Neighbors)
            {
                // Normalize pair to avoid processing twice
                var pair = room.GridX < neighbor.GridX || (room.GridX == neighbor.GridX && room.GridY < neighbor.GridY)
                    ? (room, neighbor)
                    : (neighbor, room);

                if (processed.Contains(pair))
                    continue;
                processed.Add(pair);

                PlaceDoorBetween(layer, room, neighbor, edge);
            }
        }
    }

    private void PlaceDoorBetween(TmxTileLayer layer, PlacedRoom roomA, PlacedRoom roomB, Edge edgeFromA)
    {
        int ax = roomA.GridX * _cellSize;
        int ay = roomA.GridY * _cellSize;
        int bx = roomB.GridX * _cellSize;
        int by = roomB.GridY * _cellSize;

        int center = _cellSize / 2;
        int halfDoor = _doorWidth / 2;

        switch (edgeFromA)
        {
            case Edge.East: // B is to the east of A
                for (int d = -halfDoor; d < _doorWidth - halfDoor; d++)
                {
                    int doorY = ay + center + d;
                    if (layer.IsInBounds(ax + _cellSize - 1, doorY))
                        layer[ax + _cellSize - 1, doorY] = _tiles.Door;
                    if (layer.IsInBounds(bx, doorY))
                        layer[bx, doorY] = _tiles.Door;
                }
                break;

            case Edge.West: // B is to the west of A
                for (int d = -halfDoor; d < _doorWidth - halfDoor; d++)
                {
                    int doorY = ay + center + d;
                    if (layer.IsInBounds(ax, doorY))
                        layer[ax, doorY] = _tiles.Door;
                    if (layer.IsInBounds(bx + _cellSize - 1, doorY))
                        layer[bx + _cellSize - 1, doorY] = _tiles.Door;
                }
                break;

            case Edge.South: // B is to the south of A
                for (int d = -halfDoor; d < _doorWidth - halfDoor; d++)
                {
                    int doorX = ax + center + d;
                    if (layer.IsInBounds(doorX, ay + _cellSize - 1))
                        layer[doorX, ay + _cellSize - 1] = _tiles.Door;
                    if (layer.IsInBounds(doorX, by))
                        layer[doorX, by] = _tiles.Door;
                }
                break;

            case Edge.North: // B is to the north of A
                for (int d = -halfDoor; d < _doorWidth - halfDoor; d++)
                {
                    int doorX = ax + center + d;
                    if (layer.IsInBounds(doorX, ay))
                        layer[doorX, ay] = _tiles.Door;
                    if (layer.IsInBounds(doorX, by + _cellSize - 1))
                        layer[doorX, by + _cellSize - 1] = _tiles.Door;
                }
                break;
        }
    }

    private void AddWallsAroundFloors(TmxTileLayer layer)
    {
        var wallPositions = new HashSet<(int x, int y)>();

        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                var tile = layer[x, y];
                if (tile == _tiles.Floor || tile == _tiles.Door)
                {
                    // Check all 8 neighbors
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (layer.IsInBounds(nx, ny) && layer[nx, ny] == 0)
                            {
                                wallPositions.Add((nx, ny));
                            }
                        }
                    }
                }
            }
        }

        foreach (var (wx, wy) in wallPositions)
        {
            layer[wx, wy] = _tiles.Wall;
        }
    }

    private void AddRoomObject(TmxObjectGroup group, PlacedRoom room)
    {
        group.AddObject(
            $"Room_{room.Blueprint.Type}_{group.Objects.Count}",
            room.Blueprint.Type,
            room.GridX * _cellSize * DefaultTileSize,
            room.GridY * _cellSize * DefaultTileSize,
            _cellSize * DefaultTileSize,
            _cellSize * DefaultTileSize);
    }

    private void AddSpawnPoints(IReadOnlyList<PlacedRoom> rooms, TmxObjectGroup spawnsGroup)
    {
        // Player spawn in spawn room
        var spawnRoom = rooms.FirstOrDefault(r => r.Blueprint.Type == "spawn") ?? rooms[0];
        AddSpawnPoint(spawnsGroup, spawnRoom, "PlayerSpawn", "spawn");

        // Boss spawn in boss room
        var bossRoom = rooms.FirstOrDefault(r => r.Blueprint.Type == "boss");
        if (bossRoom != null)
        {
            AddSpawnPoint(spawnsGroup, bossRoom, "BossSpawn", "boss");
        }

        // Treasure spawns
        var treasureRooms = rooms.Where(r => r.Blueprint.Type == "treasure").ToList();
        for (int i = 0; i < treasureRooms.Count; i++)
        {
            AddSpawnPoint(spawnsGroup, treasureRooms[i], $"TreasureSpawn_{i}", "treasure");
        }
    }

    private void AddSpawnPoint(TmxObjectGroup group, PlacedRoom room, string name, string type)
    {
        int centerX = room.GridX * _cellSize + _cellSize / 2;
        int centerY = room.GridY * _cellSize + _cellSize / 2;

        group.AddObject(name, type, centerX * DefaultTileSize, centerY * DefaultTileSize);
    }
}
