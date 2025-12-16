using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Generation;

Console.WriteLine("DeSales Dungeon Helper - Sandbox");
Console.WriteLine("================================");
Console.WriteLine();

// Test 1: Load and parse a YAML config
Console.WriteLine("Test 1: YAML Configuration Parsing");
Console.WriteLine("-----------------------------------");

var configPath = Path.Combine(AppContext.BaseDirectory, "sample-configs", "complex.yaml");
if (!File.Exists(configPath))
{
    Console.WriteLine($"Config file not found: {configPath}");
    return;
}

var config = DungeonConfig.LoadFromFile(configPath);
Console.WriteLine($"Loaded config: {config.Dungeon.Name}");
Console.WriteLine($"  Size: {config.Dungeon.Width}x{config.Dungeon.Height}");
Console.WriteLine($"  Seed: {config.Dungeon.Seed}");
Console.WriteLine($"  Room count: {config.Rooms.Count.Min}-{config.Rooms.Count.Max}");
Console.WriteLine($"  Room types: {string.Join(", ", config.Rooms.Types.Keys)}");
Console.WriteLine($"  Corridor style: {config.Corridors.Style}");
Console.WriteLine();

// Test 2: Generate dungeon from config
Console.WriteLine("Test 2: Dungeon Generation from Config");
Console.WriteLine("---------------------------------------");

var tmxMap = DungeonGenerator.Generate(config);

var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);
var outputPath = Path.Combine(outputDir, $"{config.Dungeon.Name}.tmx");
tmxMap.Save(outputPath);

var roomsGroup = tmxMap.GetObjectGroup("Rooms")!;
var spawnsGroup = tmxMap.GetObjectGroup("Spawns")!;

Console.WriteLine($"Generated TMX map: {outputPath}");
Console.WriteLine($"  Size: {tmxMap.Width}x{tmxMap.Height} tiles");
Console.WriteLine($"  Tile size: {tmxMap.TileWidth}x{tmxMap.TileHeight} pixels");
Console.WriteLine($"  Tile layers: {tmxMap.TileLayers.Count}");
Console.WriteLine($"  Object groups: {tmxMap.ObjectGroups.Count}");
Console.WriteLine($"  Rooms: {roomsGroup.Objects.Count}");
Console.WriteLine($"  Spawn points: {spawnsGroup.Objects.Count}");
Console.WriteLine();

// Build spawn point lookup for ASCII preview
var spawnLookup = new Dictionary<(int x, int y), char>();
foreach (var spawn in spawnsGroup.Objects)
{
    var tileX = (int)(spawn.X / tmxMap.TileWidth);
    var tileY = (int)(spawn.Y / tmxMap.TileHeight);
    var marker = spawn.Type switch
    {
        "spawn" => 'P',    // Player spawn
        "boss" => 'B',     // Boss spawn
        "treasure" => 'T', // Treasure spawn
        _ => 'S'           // Generic spawn
    };
    spawnLookup[(tileX, tileY)] = marker;
}

// ASCII preview from TMX
Console.WriteLine("ASCII Preview:");
var tileLayer = tmxMap.GetTileLayer("Tiles")!;
for (var y = 0; y < tmxMap.Height; y++)
{
    for (var x = 0; x < tmxMap.Width; x++)
    {
        // Check for spawn point first
        if (spawnLookup.TryGetValue((x, y), out var spawnChar))
        {
            Console.Write(spawnChar);
            continue;
        }

        var tileId = tileLayer[x, y];
        var ch = tileId switch
        {
            0 => ' ',
            1 => '.',
            2 => '#',
            3 => '+',
            _ => '?',
        };
        Console.Write(ch);
    }

    Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine("Legend: . = Floor, # = Wall, + = Door, P = Player, B = Boss, T = Treasure");
Console.WriteLine();

// List generated rooms
Console.WriteLine("Rooms:");
foreach (var room in roomsGroup.Objects)
{
    var tileX = (int)(room.X / tmxMap.TileWidth);
    var tileY = (int)(room.Y / tmxMap.TileHeight);
    var tileW = (int)(room.Width!.Value / tmxMap.TileWidth);
    var tileH = (int)(room.Height!.Value / tmxMap.TileHeight);
    Console.WriteLine($"  {room.Name} ({room.Type}): {tileW}x{tileH} at ({tileX}, {tileY})");
}

Console.WriteLine();

// List spawn points
Console.WriteLine("Spawn Points:");
foreach (var spawn in spawnsGroup.Objects)
{
    var tileX = (int)(spawn.X / tmxMap.TileWidth);
    var tileY = (int)(spawn.Y / tmxMap.TileHeight);
    Console.WriteLine($"  {spawn.Name} ({spawn.Type}): at ({tileX}, {tileY})");
}

Console.WriteLine();
Console.WriteLine($"Open {outputPath} in Tiled to inspect the generated map!");
