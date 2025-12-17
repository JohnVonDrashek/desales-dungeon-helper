using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Rooms;
using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Generation;

/// <summary>
/// Generates dungeons using Isaac-style grid growth algorithm.
/// Now uses the two-stage pipeline: layout generation → room assembly.
/// </summary>
public class DungeonGenerator
{
    private const int DefaultCellSize = 10;

    private static readonly (int dx, int dy, Edge edge)[] _directions =
    [
        (0, -1, Edge.North),
        (1, 0, Edge.East),
        (0, 1, Edge.South),
        (-1, 0, Edge.West)
    ];

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

        int cellSize = config.Dungeon.CellSize > 0 ? config.Dungeon.CellSize : DefaultCellSize;
        int gridWidth = Math.Max(3, config.Dungeon.Width / cellSize);
        int gridHeight = Math.Max(3, config.Dungeon.Height / cellSize);

        // Stage 1: Generate abstract layout
        var grid = new LayoutCell?[gridWidth, gridHeight];
        int targetRooms = random.Next(config.Rooms.Count.Min, config.Rooms.Count.Max + 1);
        var cells = GenerateIsaacLayout(grid, gridWidth, gridHeight, targetRooms, random);

        // Assign room types
        AssignRoomTypes(cells, config.Rooms, random);

        // Stage 2: Build room sources from config
        var roomSources = BuildRoomSources(config, cellSize, tiles);

        // Stage 3: Convert to placed rooms with blueprints
        var placedRooms = ConvertToPlacedRooms(cells, roomSources, random);

        // Stage 4: Assemble into final map
        var assembler = new DungeonAssembler(tiles, cellSize, config.Corridors.EffectiveDoorWidth);
        bool voidExterior = config.Dungeon.Exterior.Equals("void", StringComparison.OrdinalIgnoreCase);

        return assembler.Assemble(placedRooms, gridWidth, gridHeight, voidExterior);
    }

    /// <summary>
    /// Builds room sources for each room type based on configuration.
    /// </summary>
    private static Dictionary<string, IRoomSource> BuildRoomSources(DungeonConfig config, int cellSize, TilesConfig tiles)
    {
        var sources = new Dictionary<string, IRoomSource>();
        var proceduralSource = new ProceduralRoomSource(cellSize, tiles, config.Corridors.EffectiveDoorWidth);

        // Build template source if templates_dir is specified
        TemplateRoomSource? templateSource = null;
        if (!string.IsNullOrEmpty(config.Rooms.TemplatesDir) && Directory.Exists(config.Rooms.TemplatesDir))
        {
            templateSource = new TemplateRoomSource(config.Rooms.TemplatesDir, tiles);
        }

        // For each room type, determine the source
        foreach (var (typeName, typeConfig) in config.Rooms.Types)
        {
            var source = typeConfig.Source.ToLowerInvariant() switch
            {
                "template" => BuildTemplateSource(typeName, typeConfig, config.Rooms.TemplatesDir, tiles, templateSource),
                "mixed" => BuildMixedSource(typeName, typeConfig, config.Rooms.TemplatesDir, tiles, proceduralSource, templateSource),
                _ => proceduralSource as IRoomSource // "procedural" or default
            };

            sources[typeName] = source;
        }

        // Add default procedural source for any types not explicitly configured
        sources.TryAdd("spawn", proceduralSource);
        sources.TryAdd("boss", proceduralSource);
        sources.TryAdd("treasure", proceduralSource);
        sources.TryAdd("standard", proceduralSource);

        return sources;
    }

    private static IRoomSource BuildTemplateSource(
        string typeName,
        RoomTypeConfig typeConfig,
        string? templatesDir,
        TilesConfig tiles,
        TemplateRoomSource? globalTemplateSource)
    {
        // If specific template files are listed, use those
        if (typeConfig.TemplateFiles.Count > 0 && !string.IsNullOrEmpty(templatesDir))
        {
            var files = typeConfig.TemplateFiles.Select(f => Path.Combine(templatesDir, f)).ToList();
            return new TemplateRoomSource(files, tiles, typeName);
        }

        // Otherwise use the global template source if available
        return globalTemplateSource as IRoomSource ?? new ProceduralRoomSource(10, tiles); // Fallback to procedural
    }

    private static CompositeRoomSource BuildMixedSource(
        string typeName,
        RoomTypeConfig typeConfig,
        string? templatesDir,
        TilesConfig tiles,
        ProceduralRoomSource proceduralSource,
        TemplateRoomSource? globalTemplateSource)
    {
        var composite = new CompositeRoomSource();

        // Add procedural with weight (1 - weight_template)
        composite.AddSource(proceduralSource, 1.0 - typeConfig.WeightTemplate);

        // Add template source with weight_template
        var templateSource = BuildTemplateSource(typeName, typeConfig, templatesDir, tiles, globalTemplateSource);
        composite.AddSource(templateSource, typeConfig.WeightTemplate);

        return composite;
    }

    private static Random CreateRandom(int? seed)
    {
        return seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Generates an Isaac-style room layout using breadth-first growth.
    /// </summary>
    private static List<LayoutCell> GenerateIsaacLayout(
        LayoutCell?[,] grid,
        int gridWidth,
        int gridHeight,
        int targetRooms,
        Random random)
    {
        var cells = new List<LayoutCell>();
        var queue = new Queue<LayoutCell>();

        // Start in center
        int startX = gridWidth / 2;
        int startY = gridHeight / 2;
        var startCell = new LayoutCell(startX, startY);
        grid[startX, startY] = startCell;
        cells.Add(startCell);
        queue.Enqueue(startCell);

        while (queue.Count > 0 && cells.Count < targetRooms)
        {
            var current = queue.Dequeue();

            // Try each direction in random order
            var shuffledDirs = _directions.OrderBy(_ => random.Next()).ToArray();

            foreach (var (dx, dy, edge) in shuffledDirs)
            {
                if (cells.Count >= targetRooms)
                    break;

                int nx = current.GridX + dx;
                int ny = current.GridY + dy;

                // Check bounds
                if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight)
                    continue;

                // Check if already occupied
                if (grid[nx, ny] != null)
                    continue;

                // Key Isaac rule: only add if would have ≤2 neighbors (prevents loops)
                int neighborCount = CountNeighbors(grid, nx, ny, gridWidth, gridHeight);
                if (neighborCount > 2)
                    continue;

                // Random chance to skip (creates variety)
                if (random.NextDouble() < 0.3)
                    continue;

                // Create new cell
                var newCell = new LayoutCell(nx, ny);
                grid[nx, ny] = newCell;
                cells.Add(newCell);
                queue.Enqueue(newCell);

                // Link neighbors with edge info
                newCell.SetNeighbor(OppositeEdge(edge), current);
                current.SetNeighbor(edge, newCell);

                // Link to any other adjacent cells
                foreach (var (ddx, ddy, adjEdge) in _directions)
                {
                    int adjX = nx + ddx;
                    int adjY = ny + ddy;
                    if (adjX >= 0 && adjX < gridWidth && adjY >= 0 && adjY < gridHeight)
                    {
                        var adj = grid[adjX, adjY];
                        if (adj != null && adj != current && !newCell.HasNeighbor(OppositeEdge(adjEdge)))
                        {
                            newCell.SetNeighbor(OppositeEdge(adjEdge), adj);
                            adj.SetNeighbor(adjEdge, newCell);
                        }
                    }
                }
            }
        }

        return cells;
    }

    private static Edge OppositeEdge(Edge e) => e switch
    {
        Edge.North => Edge.South,
        Edge.South => Edge.North,
        Edge.East => Edge.West,
        Edge.West => Edge.East,
        _ => throw new ArgumentOutOfRangeException(nameof(e))
    };

    private static int CountNeighbors(LayoutCell?[,] grid, int x, int y, int width, int height)
    {
        int count = 0;
        foreach (var (dx, dy, _) in _directions)
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
    private static void AssignRoomTypes(List<LayoutCell> cells, RoomsConfig config, Random random)
    {
        if (cells.Count == 0)
            return;

        // First cell (center) is always spawn
        cells[0].Type = "spawn";

        // Find farthest cell from spawn - this becomes boss room
        var farthest = FindFarthestCell(cells, cells[0]);
        if (farthest != cells[0])
        {
            farthest.Type = "boss";
        }

        // Dead ends (only 1 neighbor) become treasure rooms
        var deadEnds = cells.Where(c => c.NeighborCount == 1 && c.Type == null).ToList();
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

    private static LayoutCell FindFarthestCell(List<LayoutCell> cells, LayoutCell start)
    {
        var visited = new HashSet<LayoutCell>();
        var queue = new Queue<LayoutCell>();
        queue.Enqueue(start);
        visited.Add(start);

        LayoutCell farthest = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            farthest = current;

            foreach (var neighbor in current.GetAllNeighbors())
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
    /// Converts layout cells to placed rooms with blueprints.
    /// </summary>
    private static List<PlacedRoom> ConvertToPlacedRooms(
        List<LayoutCell> cells,
        Dictionary<string, IRoomSource> roomSources,
        Random random)
    {
        var cellToRoom = new Dictionary<LayoutCell, PlacedRoom>();

        // Create PlacedRoom for each cell
        foreach (var cell in cells)
        {
            var source = roomSources.TryGetValue(cell.Type!, out var s) ? s : roomSources["standard"];
            var blueprint = source.GetBlueprints(cell.Type!, random).First();
            var placedRoom = new PlacedRoom(cell.GridX, cell.GridY, blueprint);
            cellToRoom[cell] = placedRoom;
        }

        // Link neighbors
        foreach (var cell in cells)
        {
            var room = cellToRoom[cell];
            foreach (var (edge, neighbor) in cell.GetNeighborsByEdge())
            {
                if (cellToRoom.TryGetValue(neighbor, out var neighborRoom))
                {
                    room.Neighbors[edge] = neighborRoom;
                }
            }
        }

        return cellToRoom.Values.ToList();
    }

    /// <summary>
    /// Represents a cell in the grid layout with edge-based neighbor tracking.
    /// </summary>
    private sealed class LayoutCell
    {
        public int GridX { get; }
        public int GridY { get; }
        public string? Type { get; set; }

        private readonly Dictionary<Edge, LayoutCell> _neighbors = [];

        public int NeighborCount => _neighbors.Count;

        public LayoutCell(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
        }

        public void SetNeighbor(Edge edge, LayoutCell neighbor) => _neighbors[edge] = neighbor;

        public bool HasNeighbor(Edge edge) => _neighbors.ContainsKey(edge);

        public Dictionary<Edge, LayoutCell>.ValueCollection GetAllNeighbors() => _neighbors.Values;

        public IEnumerable<(Edge edge, LayoutCell neighbor)> GetNeighborsByEdge()
            => _neighbors.Select(kvp => (kvp.Key, kvp.Value));
    }
}
