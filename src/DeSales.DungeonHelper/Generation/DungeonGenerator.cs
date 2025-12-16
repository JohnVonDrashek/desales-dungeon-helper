using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Generation;

/// <summary>
/// Generates dungeons based on configuration and outputs to TMX format.
/// </summary>
public class DungeonGenerator
{
    private const int DefaultTileSize = 16;
    private const int RoomPadding = 1; // Minimum space between rooms

    /// <summary>
    /// Generates a dungeon based on the provided configuration.
    /// </summary>
    /// <param name="config">The dungeon configuration.</param>
    /// <param name="validateConfig">Whether to validate the configuration before generating. Default is true.</param>
    /// <returns>A TMX map containing the generated dungeon.</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public static TmxMap Generate(DungeonConfig config, bool validateConfig = true)
    {
        if (validateConfig)
        {
            config.ThrowIfInvalid();
        }

        var random = CreateRandom(config.Dungeon.Seed);
        var tiles = config.Tiles ?? new TilesConfig();

        var map = new TmxMap(
            config.Dungeon.Width,
            config.Dungeon.Height,
            DefaultTileSize,
            DefaultTileSize);

        var tileLayer = map.AddTileLayer("Tiles");
        var roomsGroup = map.AddObjectGroup("Rooms");
        var spawnsGroup = map.AddObjectGroup("Spawns");

        // Generate rooms
        var rooms = GenerateRooms(config, random, map.Width, map.Height);

        // Render rooms to tile layer and add room objects
        foreach (var room in rooms)
        {
            RenderRoom(tileLayer, room, tiles);
            AddRoomObject(roomsGroup, room);
        }

        // Connect rooms with corridors
        GenerateCorridors(rooms, tileLayer, tiles);

        // Add spawn points
        AddSpawnPoints(rooms, spawnsGroup);

        return map;
    }

    private static Random CreateRandom(int? seed)
    {
        return seed.HasValue ? new Random(seed.Value) : new Random();
    }

    private static List<GeneratedRoom> GenerateRooms(DungeonConfig config, Random random, int mapWidth, int mapHeight)
    {
        var rooms = new List<GeneratedRoom>();
        var roomCount = random.Next(config.Rooms.Count.Min, config.Rooms.Count.Max + 1);

        // First pass: count how many rooms of each type we need
        var typeCounts = CalculateRoomTypeCounts(config.Rooms.Types, roomCount, random);

        // Generate rooms for each type
        foreach (var (typeName, count) in typeCounts)
        {
            var typeConfig = config.Rooms.Types.GetValueOrDefault(typeName) ?? new RoomTypeConfig();

            for (var i = 0; i < count; i++)
            {
                var room = TryPlaceRoom(
                    rooms,
                    typeName,
                    typeConfig.Size,
                    random,
                    mapWidth,
                    mapHeight,
                    maxAttempts: 100);

                if (room != null)
                {
                    rooms.Add(room);
                }
            }
        }

        return rooms;
    }

    private static Dictionary<string, int> CalculateRoomTypeCounts(
        Dictionary<string, RoomTypeConfig> types,
        int totalRooms,
        Random random)
    {
        var result = new Dictionary<string, int>();
        var remaining = totalRooms;

        // First pass: handle explicit counts
        foreach (var (typeName, typeConfig) in types)
        {
            if (typeConfig.Count.Equals("rest", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var range = IntRange.Parse(typeConfig.Count);
            var count = random.Next(range.Min, range.Max + 1);
            result[typeName] = count;
            remaining -= count;
        }

        // Second pass: distribute "rest" rooms
        var restTypes = types.Where(t => t.Value.Count.Equals("rest", StringComparison.OrdinalIgnoreCase)).ToList();
        if (restTypes.Count > 0 && remaining > 0)
        {
            var perType = remaining / restTypes.Count;
            var extra = remaining % restTypes.Count;

            foreach (var (typeName, _) in restTypes)
            {
                var count = perType + (extra > 0 ? 1 : 0);
                extra = Math.Max(0, extra - 1);
                result[typeName] = count;
            }
        }

        return result;
    }

    private static GeneratedRoom? TryPlaceRoom(
        List<GeneratedRoom> existingRooms,
        string type,
        SizeRange sizeRange,
        Random random,
        int mapWidth,
        int mapHeight,
        int maxAttempts)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var width = random.Next(sizeRange.MinWidth, sizeRange.MaxWidth + 1);
            var height = random.Next(sizeRange.MinHeight, sizeRange.MaxHeight + 1);

            // Leave room for walls on all sides
            var maxX = mapWidth - width - 1;
            var maxY = mapHeight - height - 1;

            if (maxX < 1 || maxY < 1)
            {
                continue;
            }

            var x = random.Next(1, maxX);
            var y = random.Next(1, maxY);

            var candidate = new GeneratedRoom(type, x, y, width, height);

            if (!Overlaps(candidate, existingRooms))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool Overlaps(GeneratedRoom candidate, List<GeneratedRoom> existingRooms)
    {
        foreach (var existing in existingRooms)
        {
            // Add padding between rooms
            var padded = new GeneratedRoom(
                existing.Type,
                existing.X - RoomPadding,
                existing.Y - RoomPadding,
                existing.Width + RoomPadding * 2,
                existing.Height + RoomPadding * 2);

            if (candidate.X < padded.X + padded.Width &&
                candidate.X + candidate.Width > padded.X &&
                candidate.Y < padded.Y + padded.Height &&
                candidate.Y + candidate.Height > padded.Y)
            {
                return true;
            }
        }

        return false;
    }

    private static void GenerateCorridors(List<GeneratedRoom> rooms, TmxTileLayer layer, TilesConfig tiles)
    {
        if (rooms.Count < 2)
        {
            return;
        }

        // Use Prim's algorithm to create a minimum spanning tree connecting all rooms
        var connected = new HashSet<int> { 0 };
        var edges = new List<(int from, int to)>();

        while (connected.Count < rooms.Count)
        {
            var bestEdge = (-1, -1);
            var bestDistance = double.MaxValue;

            foreach (var fromIdx in connected)
            {
                for (var toIdx = 0; toIdx < rooms.Count; toIdx++)
                {
                    if (connected.Contains(toIdx))
                    {
                        continue;
                    }

                    var distance = GetRoomDistance(rooms[fromIdx], rooms[toIdx]);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestEdge = (fromIdx, toIdx);
                    }
                }
            }

            if (bestEdge.Item1 != -1)
            {
                connected.Add(bestEdge.Item2);
                edges.Add(bestEdge);
            }
            else
            {
                break; // No more connections possible
            }
        }

        // Draw corridors for each edge
        foreach (var (fromIdx, toIdx) in edges)
        {
            DrawCorridor(rooms[fromIdx], rooms[toIdx], layer, tiles, rooms);
        }
    }

    private static double GetRoomDistance(GeneratedRoom a, GeneratedRoom b)
    {
        var aCenterX = a.X + a.Width / 2;
        var aCenterY = a.Y + a.Height / 2;
        var bCenterX = b.X + b.Width / 2;
        var bCenterY = b.Y + b.Height / 2;

        var dx = aCenterX - bCenterX;
        var dy = aCenterY - bCenterY;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static void DrawCorridor(GeneratedRoom from, GeneratedRoom to, TmxTileLayer layer, TilesConfig tiles, List<GeneratedRoom> allRooms)
    {
        // Find the best exit points from each room (these are wall positions)
        var (fromExit, toExit) = FindBestExitPoints(from, to);

        // Calculate corridor points just outside the room walls
        var fromCorridorStart = GetPointOutsideRoom(fromExit, from);
        var toCorridorStart = GetPointOutsideRoom(toExit, to);

        // Draw L-shaped corridor between the two corridor start points
        // Use the Y of the first point for horizontal, X of second for vertical
        DrawHorizontalSegment(fromCorridorStart.x, toCorridorStart.x, fromCorridorStart.y, layer, tiles, allRooms);
        DrawVerticalSegment(fromCorridorStart.y, toCorridorStart.y, toCorridorStart.x, layer, tiles, allRooms);

        // Place doors at exit points
        layer[fromExit.x, fromExit.y] = tiles.Door;
        layer[toExit.x, toExit.y] = tiles.Door;
    }

    private static (int x, int y) GetPointOutsideRoom((int x, int y) exitPoint, GeneratedRoom room)
    {
        // Determine which wall the exit is on and return the point just outside
        if (exitPoint.x == room.X) // Left wall
        {
            return (exitPoint.x - 1, exitPoint.y);
        }

        if (exitPoint.x == room.X + room.Width - 1) // Right wall
        {
            return (exitPoint.x + 1, exitPoint.y);
        }

        if (exitPoint.y == room.Y) // Top wall
        {
            return (exitPoint.x, exitPoint.y - 1);
        }

        // Bottom wall
        return (exitPoint.x, exitPoint.y + 1);
    }

    private static ((int x, int y) fromExit, (int x, int y) toExit) FindBestExitPoints(GeneratedRoom from, GeneratedRoom to)
    {
        var fromCenterX = from.X + from.Width / 2;
        var fromCenterY = from.Y + from.Height / 2;
        var toCenterX = to.X + to.Width / 2;
        var toCenterY = to.Y + to.Height / 2;

        // Determine primary direction
        var dx = toCenterX - fromCenterX;
        var dy = toCenterY - fromCenterY;

        (int x, int y) fromExit;
        (int x, int y) toExit;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Horizontal connection is primary
            if (dx > 0)
            {
                // 'to' is to the right of 'from'
                fromExit = (from.X + from.Width - 1, Clamp(toCenterY, from.Y + 1, from.Y + from.Height - 2));
                toExit = (to.X, Clamp(fromCenterY, to.Y + 1, to.Y + to.Height - 2));
            }
            else
            {
                // 'to' is to the left of 'from'
                fromExit = (from.X, Clamp(toCenterY, from.Y + 1, from.Y + from.Height - 2));
                toExit = (to.X + to.Width - 1, Clamp(fromCenterY, to.Y + 1, to.Y + to.Height - 2));
            }
        }
        else
        {
            // Vertical connection is primary
            if (dy > 0)
            {
                // 'to' is below 'from'
                fromExit = (Clamp(toCenterX, from.X + 1, from.X + from.Width - 2), from.Y + from.Height - 1);
                toExit = (Clamp(fromCenterX, to.X + 1, to.X + to.Width - 2), to.Y);
            }
            else
            {
                // 'to' is above 'from'
                fromExit = (Clamp(toCenterX, from.X + 1, from.X + from.Width - 2), from.Y);
                toExit = (Clamp(fromCenterX, to.X + 1, to.X + to.Width - 2), to.Y + to.Height - 1);
            }
        }

        return (fromExit, toExit);
    }

    private static int Clamp(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private static void DrawHorizontalSegment(int x1, int x2, int y, TmxTileLayer layer, TilesConfig tiles, List<GeneratedRoom> allRooms)
    {
        var minX = Math.Min(x1, x2);
        var maxX = Math.Max(x1, x2);

        for (var x = minX; x <= maxX; x++)
        {
            PlaceCorridorTile(x, y, layer, tiles, allRooms);
        }
    }

    private static void DrawVerticalSegment(int y1, int y2, int x, TmxTileLayer layer, TilesConfig tiles, List<GeneratedRoom> allRooms)
    {
        var minY = Math.Min(y1, y2);
        var maxY = Math.Max(y1, y2);

        for (var y = minY; y <= maxY; y++)
        {
            PlaceCorridorTile(x, y, layer, tiles, allRooms);
        }
    }

    private static void PlaceCorridorTile(int x, int y, TmxTileLayer layer, TilesConfig tiles, List<GeneratedRoom> allRooms)
    {
        var currentTile = layer[x, y];

        // Skip if inside a room (not on wall) - don't overwrite room floors
        if (IsInsideAnyRoom(x, y, allRooms))
        {
            return;
        }

        // Check if this tile is on ANY room's wall
        var isOnWall = allRooms.Any(room => IsOnRoomWall(x, y, room));

        if (isOnWall)
        {
            // Place door on wall
            layer[x, y] = tiles.Door;
        }
        else if (currentTile == 0) // Empty tile - place floor
        {
            layer[x, y] = tiles.Floor;
        }
        // If it's already a floor or door, leave it alone
    }

    private static bool IsInsideAnyRoom(int x, int y, List<GeneratedRoom> rooms)
    {
        return rooms.Any(room =>
            x > room.X && x < room.X + room.Width - 1 &&
            y > room.Y && y < room.Y + room.Height - 1);
    }

    private static bool IsOnRoomWall(int x, int y, GeneratedRoom room)
    {
        // Check if point is on the room's wall (edge) but not a corner
        var onLeftWall = x == room.X && y > room.Y && y < room.Y + room.Height - 1;
        var onRightWall = x == room.X + room.Width - 1 && y > room.Y && y < room.Y + room.Height - 1;
        var onTopWall = y == room.Y && x > room.X && x < room.X + room.Width - 1;
        var onBottomWall = y == room.Y + room.Height - 1 && x > room.X && x < room.X + room.Width - 1;

        return onLeftWall || onRightWall || onTopWall || onBottomWall;
    }

    private static void RenderRoom(TmxTileLayer layer, GeneratedRoom room, TilesConfig tiles)
    {
        // Fill interior with floor
        for (var x = room.X + 1; x < room.X + room.Width - 1; x++)
        {
            for (var y = room.Y + 1; y < room.Y + room.Height - 1; y++)
            {
                layer[x, y] = tiles.Floor;
            }
        }

        // Add walls on edges
        for (var x = room.X; x < room.X + room.Width; x++)
        {
            layer[x, room.Y] = tiles.Wall;
            layer[x, room.Y + room.Height - 1] = tiles.Wall;
        }

        for (var y = room.Y; y < room.Y + room.Height; y++)
        {
            layer[room.X, y] = tiles.Wall;
            layer[room.X + room.Width - 1, y] = tiles.Wall;
        }
    }

    private static void AddRoomObject(TmxObjectGroup group, GeneratedRoom room)
    {
        group.AddObject(
            $"Room_{room.Type}_{group.Objects.Count}",
            room.Type,
            room.X * DefaultTileSize,
            room.Y * DefaultTileSize,
            room.Width * DefaultTileSize,
            room.Height * DefaultTileSize);
    }

    private static void AddSpawnPoints(List<GeneratedRoom> rooms, TmxObjectGroup spawnsGroup)
    {
        // Player spawn in spawn room
        var spawnRoom = rooms.FirstOrDefault(r => r.Type == "spawn") ?? rooms.First();
        AddSpawnPoint(spawnsGroup, spawnRoom, "PlayerSpawn", "spawn");

        // Boss spawn in boss room
        var bossRoom = rooms.FirstOrDefault(r => r.Type == "boss");
        if (bossRoom != null)
        {
            AddSpawnPoint(spawnsGroup, bossRoom, "BossSpawn", "boss");
        }

        // Treasure spawns in treasure rooms
        var treasureRooms = rooms.Where(r => r.Type == "treasure").ToList();
        for (var i = 0; i < treasureRooms.Count; i++)
        {
            AddSpawnPoint(spawnsGroup, treasureRooms[i], $"TreasureSpawn_{i}", "treasure");
        }
    }

    private static void AddSpawnPoint(TmxObjectGroup group, GeneratedRoom room, string name, string type)
    {
        var centerX = room.X + room.Width / 2;
        var centerY = room.Y + room.Height / 2;

        group.AddObject(
            name,
            type,
            centerX * DefaultTileSize,
            centerY * DefaultTileSize);
    }

    /// <summary>
    /// Internal room representation during generation.
    /// </summary>
    private sealed class GeneratedRoom
    {
        public string Type { get; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public GeneratedRoom(string type, int x, int y, int width, int height)
        {
            Type = type;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
