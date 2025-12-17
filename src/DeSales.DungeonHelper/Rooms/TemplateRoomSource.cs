using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Rooms;

/// <summary>
/// Loads room blueprints from TMX template files.
/// Supports explicit door markers via "Doors" object group, or auto-detection by scanning edges.
/// </summary>
public sealed class TemplateRoomSource : IRoomSource
{
    private readonly Dictionary<string, List<RoomBlueprint>> _blueprintsByType = [];
    private readonly TilesConfig _tiles;

    /// <summary>
    /// Creates a template room source from TMX files in the specified directory.
    /// </summary>
    /// <param name="templatesDir">Directory containing TMX room templates.</param>
    /// <param name="tiles">Tile configuration for floor/wall detection.</param>
    /// <param name="typeMapping">Maps room types to specific template files. If null, all templates are available to all types.</param>
    public TemplateRoomSource(string templatesDir, TilesConfig tiles, Dictionary<string, List<string>>? typeMapping = null)
    {
        _tiles = tiles;

        if (!Directory.Exists(templatesDir))
        {
            return; // No templates, will fall back to procedural
        }

        var tmxFiles = Directory.GetFiles(templatesDir, "*.tmx");

        foreach (var file in tmxFiles)
        {
            var blueprint = LoadTemplate(file);
            if (blueprint == null)
                continue;

            // Determine which types this template applies to
            var applicableTypes = GetApplicableTypes(file, blueprint.Type, typeMapping);

            foreach (var type in applicableTypes)
            {
                if (!_blueprintsByType.TryGetValue(type, out var list))
                {
                    list = [];
                    _blueprintsByType[type] = list;
                }

                list.Add(blueprint);
            }
        }
    }

    /// <summary>
    /// Creates a template room source from a list of TMX files.
    /// </summary>
    public TemplateRoomSource(IEnumerable<string> templateFiles, TilesConfig tiles, string roomType)
    {
        _tiles = tiles;

        foreach (var file in templateFiles)
        {
            if (!File.Exists(file))
                continue;

            var blueprint = LoadTemplate(file, roomType);
            if (blueprint == null)
                continue;

            if (!_blueprintsByType.TryGetValue(roomType, out var list))
            {
                list = [];
                _blueprintsByType[roomType] = list;
            }

            list.Add(blueprint);
        }
    }

    public bool CanProvide(string roomType) => _blueprintsByType.ContainsKey(roomType) && _blueprintsByType[roomType].Count > 0;

    public IEnumerable<RoomBlueprint> GetBlueprints(string roomType, Random random)
    {
        if (!_blueprintsByType.TryGetValue(roomType, out var list) || list.Count == 0)
        {
            yield break;
        }

        // Shuffle and return
        var shuffled = list.OrderBy(_ => random.Next()).ToList();
        foreach (var bp in shuffled)
        {
            yield return bp;
        }
    }

    /// <summary>
    /// Gets the count of templates available for a room type.
    /// </summary>
    public int GetTemplateCount(string roomType)
        => _blueprintsByType.TryGetValue(roomType, out var list) ? list.Count : 0;

    private RoomBlueprint? LoadTemplate(string filePath, string? overrideType = null)
    {
        try
        {
            var map = TmxMap.Load(filePath);
            var tileLayer = map.GetTileLayer("Tiles");

            if (tileLayer == null)
            {
                return null;
            }

            // Extract tile data
            var tiles = new int[map.Width, map.Height];
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    tiles[x, y] = tileLayer[x, y];
                }
            }

            // Determine room type
            string type = overrideType ?? InferTypeFromFileName(filePath);

            // Discover door slots
            var doorSlots = DiscoverDoorSlots(map, tiles);

            return new RoomBlueprint(
                map.Width,
                map.Height,
                tiles,
                type,
                doorSlots,
                Path.GetFileNameWithoutExtension(filePath));
        }
        catch
        {
            return null;
        }
    }

    private DoorSlot[] DiscoverDoorSlots(TmxMap map, int[,] tiles)
    {
        // First, check for explicit "Doors" object group
        var doorsGroup = map.GetObjectGroup("Doors");
        if (doorsGroup != null && doorsGroup.Objects.Count > 0)
        {
            return ParseDoorObjects(doorsGroup, map.Width, map.Height, map.TileWidth, map.TileHeight);
        }

        // Auto-detect by scanning edges for floor tiles
        return AutoDetectDoorSlots(tiles, map.Width, map.Height);
    }

    private static DoorSlot[] ParseDoorObjects(TmxObjectGroup doorsGroup, int mapWidth, int mapHeight, int tileWidth, int tileHeight)
    {
        var slots = new List<DoorSlot>();

        foreach (var obj in doorsGroup.Objects)
        {
            if (!obj.Width.HasValue || !obj.Height.HasValue)
                continue;

            // Convert pixel coordinates to tile coordinates
            int tileX = (int)(obj.X / tileWidth);
            int tileY = (int)(obj.Y / tileHeight);
            int tilW = (int)(obj.Width.Value / tileWidth);
            int tilH = (int)(obj.Height.Value / tileHeight);

            // Determine which edge based on position
            Edge edge;
            int position;
            int width;

            if (tileY == 0)
            {
                edge = Edge.North;
                position = tileX;
                width = tilW;
            }
            else if (tileY + tilH >= mapHeight)
            {
                edge = Edge.South;
                position = tileX;
                width = tilW;
            }
            else if (tileX == 0)
            {
                edge = Edge.West;
                position = tileY;
                width = tilH;
            }
            else if (tileX + tilW >= mapWidth)
            {
                edge = Edge.East;
                position = tileY;
                width = tilH;
            }
            else
            {
                continue; // Not on an edge
            }

            slots.Add(new DoorSlot(edge, position, Math.Max(1, width)));
        }

        return slots.ToArray();
    }

    private DoorSlot[] AutoDetectDoorSlots(int[,] tiles, int width, int height)
    {
        var slots = new List<DoorSlot>();

        // Scan North edge (y=0)
        var northRuns = FindFloorRuns(tiles, width, 0, true);
        slots.AddRange(northRuns.Select(r => new DoorSlot(Edge.North, r.start, r.length)));

        // Scan South edge (y=height-1)
        var southRuns = FindFloorRuns(tiles, width, height - 1, true);
        slots.AddRange(southRuns.Select(r => new DoorSlot(Edge.South, r.start, r.length)));

        // Scan West edge (x=0)
        var westRuns = FindFloorRuns(tiles, height, 0, false);
        slots.AddRange(westRuns.Select(r => new DoorSlot(Edge.West, r.start, r.length)));

        // Scan East edge (x=width-1)
        var eastRuns = FindFloorRuns(tiles, height, width - 1, false);
        slots.AddRange(eastRuns.Select(r => new DoorSlot(Edge.East, r.start, r.length)));

        return slots.ToArray();
    }

    private List<(int start, int length)> FindFloorRuns(int[,] tiles, int edgeLength, int fixedCoord, bool horizontal)
    {
        var runs = new List<(int start, int length)>();
        int runStart = -1;

        for (int i = 0; i < edgeLength; i++)
        {
            int tile = horizontal ? tiles[i, fixedCoord] : tiles[fixedCoord, i];
            bool isFloor = tile == _tiles.Floor || tile == _tiles.Door;

            if (isFloor)
            {
                if (runStart < 0)
                    runStart = i;
            }
            else
            {
                if (runStart >= 0)
                {
                    runs.Add((runStart, i - runStart));
                    runStart = -1;
                }
            }
        }

        // Don't forget run at end
        if (runStart >= 0)
        {
            runs.Add((runStart, edgeLength - runStart));
        }

        return runs;
    }

    private static List<string> GetApplicableTypes(string filePath, string inferredType, Dictionary<string, List<string>>? typeMapping)
    {
        if (typeMapping == null)
        {
            return [inferredType];
        }

        var fileName = Path.GetFileName(filePath);
        var types = new List<string>();

        foreach (var (type, files) in typeMapping)
        {
            if (files.Contains(fileName) || files.Count == 0)
            {
                types.Add(type);
            }
        }

        return types.Count > 0 ? types : [inferredType];
    }

    private static string InferTypeFromFileName(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

        if (name.Contains("spawn"))
            return "spawn";
        if (name.Contains("boss"))
            return "boss";
        if (name.Contains("treasure"))
            return "treasure";

        return "standard";
    }
}
