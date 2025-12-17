using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Generation;

/// <summary>
/// Generates dungeons using Isaac-style grid growth algorithm.
/// </summary>
public class DungeonGenerator
{
    private const int DefaultTileSize = 16;
    private const int DefaultCellSize = 10; // Each grid cell is 10x10 tiles

    private static readonly (int dx, int dy)[] _directions = [(0, -1), (1, 0), (0, 1), (-1, 0)];

    /// <summary>
    /// Generates a dungeon based on the provided configuration.
    /// </summary>
    public static TmxMap Generate(DungeonConfig config, bool validateConfig = true)
    {
        if (validateConfig)
        {
            config.ThrowIfInvalid();
        }

        var random = CreateRandom(config.Dungeon.Seed);
        var tiles = config.Tiles ?? new TilesConfig();

        // Calculate grid dimensions from map size
        int cellSize = DefaultCellSize;
        int gridWidth = config.Dungeon.Width / cellSize;
        int gridHeight = config.Dungeon.Height / cellSize;

        // Ensure minimum grid size
        gridWidth = Math.Max(3, gridWidth);
        gridHeight = Math.Max(3, gridHeight);

        var map = new TmxMap(
            gridWidth * cellSize,
            gridHeight * cellSize,
            DefaultTileSize,
            DefaultTileSize);

        var tileLayer = map.AddTileLayer("Tiles");
        var roomsGroup = map.AddObjectGroup("Rooms");
        var spawnsGroup = map.AddObjectGroup("Spawns");

        // For void exterior mode, we don't pre-fill with walls
        var fillWithWalls = !config.Dungeon.Exterior.Equals("void", StringComparison.OrdinalIgnoreCase);
        if (fillWithWalls)
        {
            FillWithWalls(tileLayer, tiles);
        }

        // Generate Isaac-style room layout
        int targetRooms = random.Next(config.Rooms.Count.Min, config.Rooms.Count.Max + 1);
        var grid = new GridCell?[gridWidth, gridHeight];
        var cells = GenerateIsaacLayout(grid, gridWidth, gridHeight, targetRooms, random);

        // Assign room types
        AssignRoomTypes(cells, config.Rooms, random);

        // Render cells to tiles
        foreach (var cell in cells)
        {
            RenderCell(tileLayer, cell, cellSize, tiles);
            AddRoomObject(roomsGroup, cell, cellSize);
        }

        // Place doors between adjacent cells
        PlaceDoors(tileLayer, cells, cellSize, tiles, config.Corridors.EffectiveDoorWidth);

        // For void exterior mode, add walls around floors
        if (!fillWithWalls)
        {
            AddWallsAroundFloors(tileLayer, tiles);
        }

        // Add spawn points
        AddSpawnPoints(cells, spawnsGroup, cellSize);

        return map;
    }

    private static Random CreateRandom(int? seed)
    {
        return seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Generates an Isaac-style room layout using breadth-first growth.
    /// </summary>
    private static List<GridCell> GenerateIsaacLayout(
        GridCell?[,] grid,
        int gridWidth,
        int gridHeight,
        int targetRooms,
        Random random)
    {
        var cells = new List<GridCell>();
        var queue = new Queue<GridCell>();

        // Start in center
        int startX = gridWidth / 2;
        int startY = gridHeight / 2;
        var startCell = new GridCell(startX, startY);
        grid[startX, startY] = startCell;
        cells.Add(startCell);
        queue.Enqueue(startCell);

        while (queue.Count > 0 && cells.Count < targetRooms)
        {
            var current = queue.Dequeue();

            // Try each direction in random order
            var shuffledDirs = _directions.OrderBy(_ => random.Next()).ToArray();

            foreach (var (dx, dy) in shuffledDirs)
            {
                if (cells.Count >= targetRooms)
                {
                    break;
                }

                int nx = current.GridX + dx;
                int ny = current.GridY + dy;

                // Check bounds
                if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight)
                {
                    continue;
                }

                // Check if already occupied
                if (grid[nx, ny] != null)
                {
                    continue;
                }

                // Key Isaac rule: only add if would have ≤2 neighbors (prevents loops)
                int neighborCount = CountNeighbors(grid, nx, ny, gridWidth, gridHeight);
                if (neighborCount > 2)
                {
                    continue;
                }

                // Random chance to skip (creates variety)
                if (random.NextDouble() < 0.3)
                {
                    continue;
                }

                // Create new cell
                var newCell = new GridCell(nx, ny);
                grid[nx, ny] = newCell;
                cells.Add(newCell);
                queue.Enqueue(newCell);

                // Link neighbors
                newCell.Neighbors.Add(current);
                current.Neighbors.Add(newCell);

                // Link to any other adjacent cells
                foreach (var (ddx, ddy) in _directions)
                {
                    int adjX = nx + ddx;
                    int adjY = ny + ddy;
                    if (adjX >= 0 && adjX < gridWidth && adjY >= 0 && adjY < gridHeight)
                    {
                        var adj = grid[adjX, adjY];
                        if (adj != null && adj != current && !newCell.Neighbors.Contains(adj))
                        {
                            newCell.Neighbors.Add(adj);
                            adj.Neighbors.Add(newCell);
                        }
                    }
                }
            }
        }

        return cells;
    }

    private static int CountNeighbors(GridCell?[,] grid, int x, int y, int width, int height)
    {
        int count = 0;
        foreach (var (dx, dy) in _directions)
        {
            int nx = x + dx;
            int ny = y + dy;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[nx, ny] != null)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Assigns room types based on position and distance from start.
    /// </summary>
    private static void AssignRoomTypes(List<GridCell> cells, RoomsConfig config, Random random)
    {
        if (cells.Count == 0)
        {
            return;
        }

        // First cell (center) is always spawn
        cells[0].Type = "spawn";

        // Find farthest cell from spawn - this becomes boss room
        var farthest = FindFarthestCell(cells, cells[0]);
        if (farthest != cells[0])
        {
            farthest.Type = "boss";
        }

        // Dead ends (only 1 neighbor) become treasure rooms
        var deadEnds = cells.Where(c => c.Neighbors.Count == 1 && c.Type == null).ToList();
        Shuffle(deadEnds, random);

        // Count how many treasure rooms we need
        int treasureCount = 0;
        if (config.Types.TryGetValue("treasure", out var treasureConfig))
        {
            var range = IntRange.Parse(treasureConfig.Count);
            treasureCount = random.Next(range.Min, range.Max + 1);
        }

        foreach (var deadEnd in deadEnds.Take(treasureCount))
        {
            deadEnd.Type = "treasure";
        }

        // Everything else is standard
        foreach (var cell in cells.Where(c => c.Type == null))
        {
            cell.Type = "standard";
        }
    }

    private static GridCell FindFarthestCell(List<GridCell> cells, GridCell start)
    {
        // BFS to find farthest cell
        var visited = new HashSet<GridCell>();
        var queue = new Queue<GridCell>();
        queue.Enqueue(start);
        visited.Add(start);

        GridCell farthest = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            farthest = current;

            foreach (var neighbor in current.Neighbors)
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return farthest;
    }

    private static void Shuffle<T>(List<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Renders a single grid cell to the tile layer.
    /// </summary>
    private static void RenderCell(TmxTileLayer layer, GridCell cell, int cellSize, TilesConfig tiles)
    {
        int x = cell.GridX * cellSize;
        int y = cell.GridY * cellSize;

        // Fill interior with floor
        for (int dy = 1; dy < cellSize - 1; dy++)
        {
            for (int dx = 1; dx < cellSize - 1; dx++)
            {
                layer[x + dx, y + dy] = tiles.Floor;
            }
        }

        // Draw walls on all edges
        for (int d = 0; d < cellSize; d++)
        {
            layer[x + d, y] = tiles.Wall;                    // Top
            layer[x + d, y + cellSize - 1] = tiles.Wall;     // Bottom
            layer[x, y + d] = tiles.Wall;                    // Left
            layer[x + cellSize - 1, y + d] = tiles.Wall;     // Right
        }
    }

    /// <summary>
    /// Places doors between adjacent cells.
    /// </summary>
    private static void PlaceDoors(
        TmxTileLayer layer,
        List<GridCell> cells,
        int cellSize,
        TilesConfig tiles,
        int doorWidth)
    {
        var processedPairs = new HashSet<(GridCell, GridCell)>();

        foreach (var cell in cells)
        {
            foreach (var neighbor in cell.Neighbors)
            {
                // Skip if we already processed this pair
                var pair = cell.GridX < neighbor.GridX || (cell.GridX == neighbor.GridX && cell.GridY < neighbor.GridY)
                    ? (cell, neighbor)
                    : (neighbor, cell);

                if (processedPairs.Contains(pair))
                {
                    continue;
                }

                processedPairs.Add(pair);

                // Place door on shared wall
                PlaceDoor(layer, cell, neighbor, cellSize, tiles, doorWidth);
            }
        }
    }

    private static void PlaceDoor(
        TmxTileLayer layer,
        GridCell a,
        GridCell b,
        int cellSize,
        TilesConfig tiles,
        int doorWidth)
    {
        int ax = a.GridX * cellSize;
        int ay = a.GridY * cellSize;
        int bx = b.GridX * cellSize;
        int by = b.GridY * cellSize;

        int halfDoor = doorWidth / 2;
        int center = cellSize / 2;

        if (b.GridX > a.GridX)
        {
            // Door between horizontally adjacent cells
            // A's right wall and B's left wall
            int doorXA = ax + cellSize - 1;
            int doorXB = bx;
            for (int d = -halfDoor; d < doorWidth - halfDoor; d++)
            {
                int doorY = ay + center + d;
                if (layer.IsInBounds(doorXA, doorY))
                {
                    layer[doorXA, doorY] = tiles.Door;
                }

                if (layer.IsInBounds(doorXB, doorY))
                {
                    layer[doorXB, doorY] = tiles.Door;
                }
            }
        }
        else if (b.GridX < a.GridX)
        {
            // Door between horizontally adjacent cells (reverse)
            int doorXA = ax;
            int doorXB = bx + cellSize - 1;
            for (int d = -halfDoor; d < doorWidth - halfDoor; d++)
            {
                int doorY = ay + center + d;
                if (layer.IsInBounds(doorXA, doorY))
                {
                    layer[doorXA, doorY] = tiles.Door;
                }

                if (layer.IsInBounds(doorXB, doorY))
                {
                    layer[doorXB, doorY] = tiles.Door;
                }
            }
        }
        else if (b.GridY > a.GridY)
        {
            // Door between vertically adjacent cells
            // A's bottom wall and B's top wall
            int doorYA = ay + cellSize - 1;
            int doorYB = by;
            for (int d = -halfDoor; d < doorWidth - halfDoor; d++)
            {
                int doorX = ax + center + d;
                if (layer.IsInBounds(doorX, doorYA))
                {
                    layer[doorX, doorYA] = tiles.Door;
                }

                if (layer.IsInBounds(doorX, doorYB))
                {
                    layer[doorX, doorYB] = tiles.Door;
                }
            }
        }
        else if (b.GridY < a.GridY)
        {
            // Door between vertically adjacent cells (reverse)
            int doorYA = ay;
            int doorYB = by + cellSize - 1;
            for (int d = -halfDoor; d < doorWidth - halfDoor; d++)
            {
                int doorX = ax + center + d;
                if (layer.IsInBounds(doorX, doorYA))
                {
                    layer[doorX, doorYA] = tiles.Door;
                }

                if (layer.IsInBounds(doorX, doorYB))
                {
                    layer[doorX, doorYB] = tiles.Door;
                }
            }
        }
    }

    private static void FillWithWalls(TmxTileLayer layer, TilesConfig tiles)
    {
        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                layer[x, y] = tiles.Wall;
            }
        }
    }

    private static void AddWallsAroundFloors(TmxTileLayer layer, TilesConfig tiles)
    {
        var wallPositions = new HashSet<(int x, int y)>();

        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                var tile = layer[x, y];
                if (tile == tiles.Floor || tile == tiles.Door)
                {
                    // Check all 8 neighbors
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0)
                            {
                                continue;
                            }

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
            layer[wx, wy] = tiles.Wall;
        }
    }

    private static void AddRoomObject(TmxObjectGroup group, GridCell cell, int cellSize)
    {
        group.AddObject(
            $"Room_{cell.Type}_{group.Objects.Count}",
            cell.Type ?? "standard",
            cell.GridX * cellSize * DefaultTileSize,
            cell.GridY * cellSize * DefaultTileSize,
            cellSize * DefaultTileSize,
            cellSize * DefaultTileSize);
    }

    private static void AddSpawnPoints(List<GridCell> cells, TmxObjectGroup spawnsGroup, int cellSize)
    {
        // Player spawn in spawn room
        var spawnRoom = cells.FirstOrDefault(c => c.Type == "spawn") ?? cells.First();
        AddSpawnPoint(spawnsGroup, spawnRoom, cellSize, "PlayerSpawn", "spawn");

        // Boss spawn in boss room
        var bossRoom = cells.FirstOrDefault(c => c.Type == "boss");
        if (bossRoom != null)
        {
            AddSpawnPoint(spawnsGroup, bossRoom, cellSize, "BossSpawn", "boss");
        }

        // Treasure spawns in treasure rooms
        var treasureRooms = cells.Where(c => c.Type == "treasure").ToList();
        for (int i = 0; i < treasureRooms.Count; i++)
        {
            AddSpawnPoint(spawnsGroup, treasureRooms[i], cellSize, $"TreasureSpawn_{i}", "treasure");
        }
    }

    private static void AddSpawnPoint(TmxObjectGroup group, GridCell cell, int cellSize, string name, string type)
    {
        int centerX = cell.GridX * cellSize + cellSize / 2;
        int centerY = cell.GridY * cellSize + cellSize / 2;

        group.AddObject(
            name,
            type,
            centerX * DefaultTileSize,
            centerY * DefaultTileSize);
    }

    /// <summary>
    /// Represents a cell in the grid layout.
    /// </summary>
    private sealed class GridCell
    {
        public int GridX { get; }
        public int GridY { get; }
        public string? Type { get; set; }
        public List<GridCell> Neighbors { get; } = [];

        public GridCell(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
        }
    }
}
