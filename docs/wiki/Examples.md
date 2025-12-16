# Examples

Complete working examples for common use cases.

---

## Minimal Dungeon

The simplest possible dungeon - just spawn and standard rooms.

### Configuration

```yaml
dungeon:
  name: minimal
  width: 30
  height: 30
  seed: 42

rooms:
  count: 4
  types:
    spawn:
      count: 1
      size: 5x5
    standard:
      count: rest
      size: 5x5 to 7x7
```

### Code

```csharp
var config = DungeonConfig.LoadFromFile("minimal.yaml");
var map = DungeonGenerator.Generate(config);
map.Save("minimal.tmx");
```

---

## Full Featured Dungeon

A complete dungeon with all room types.

### Configuration

```yaml
dungeon:
  name: crypt_level_1
  seed: 422
  width: 50
  height: 50

rooms:
  count: 8-12
  types:
    spawn:
      count: 1
      size: 7x7 to 10x10
    boss:
      count: 1
      size: 15x15 to 20x20
    treasure:
      count: 1-2
      size: 5x5 to 8x8
    standard:
      count: rest
      size: 5x5 to 12x12

corridors:
  style: winding
  width: 1

tiles:
  floor: 1
  wall: 2
  door: 3
```

### Code

```csharp
using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Generation;

var config = DungeonConfig.LoadFromFile("crypt_level_1.yaml");
var map = DungeonGenerator.Generate(config);

Console.WriteLine($"Generated {map.GetAllRoomBounds().Count} rooms");
Console.WriteLine($"Player spawn: {map.GetSpawnPoint("PlayerSpawn")}");
Console.WriteLine($"Boss spawn: {map.GetSpawnPoint("BossSpawn")}");
Console.WriteLine($"Treasure locations: {map.GetSpawnPointsByType("treasure").Count}");

map.Save("crypt_level_1.tmx");
```

---

## Custom Room Types

Define your own room types for special gameplay.

### Configuration

```yaml
dungeon:
  name: custom_dungeon
  width: 60
  height: 60
  seed: 123

rooms:
  count: 10-15
  types:
    spawn:
      count: 1
      size: 6x6 to 8x8
    boss:
      count: 1
      size: 12x12 to 16x16
    shop:
      count: 1
      size: 8x8 to 10x10
    shrine:
      count: 1-2
      size: 5x5 to 7x7
    challenge:
      count: 2-3
      size: 8x8 to 12x12
    standard:
      count: rest
      size: 5x5 to 10x10

tiles:
  floor: 1
  wall: 2
  door: 3
```

### Code

```csharp
var config = DungeonConfig.LoadFromFile("custom_dungeon.yaml");
var map = DungeonGenerator.Generate(config);

// Query custom room types
var shops = map.GetRoomsByType("shop");
var shrines = map.GetRoomsByType("shrine");
var challenges = map.GetRoomsByType("challenge");

Console.WriteLine($"Shops: {shops.Count}");
Console.WriteLine($"Shrines: {shrines.Count}");
Console.WriteLine($"Challenges: {challenges.Count}");
```

---

## Programmatic Configuration

Create dungeons without YAML files.

```csharp
using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Generation;

var config = new DungeonConfig
{
    Dungeon = new DungeonSettings
    {
        Name = "procedural_dungeon",
        Width = 40,
        Height = 40,
        Seed = Random.Shared.Next() // Random each time
    },
    Rooms = new RoomsConfig
    {
        CountString = "6-10",
        Types = new Dictionary<string, RoomTypeConfig>
        {
            ["spawn"] = new RoomTypeConfig
            {
                Count = "1",
                SizeString = "6x6 to 8x8"
            },
            ["boss"] = new RoomTypeConfig
            {
                Count = "1",
                SizeString = "10x10 to 14x14"
            },
            ["treasure"] = new RoomTypeConfig
            {
                Count = "1-3",
                SizeString = "5x5 to 7x7"
            },
            ["standard"] = new RoomTypeConfig
            {
                Count = "rest",
                SizeString = "5x5 to 10x10"
            }
        }
    },
    Tiles = new TilesConfig
    {
        Floor = 1,
        Wall = 2,
        Door = 3
    }
};

// Validate before generating
var errors = config.Validate();
if (errors.Count > 0)
{
    foreach (var error in errors)
        Console.WriteLine($"Error: {error}");
    return;
}

var map = DungeonGenerator.Generate(config);
map.Save($"dungeons/{config.Dungeon.Name}.tmx");
```

---

## Multiple Dungeons with Different Seeds

Generate variations of the same layout.

```csharp
var baseConfig = DungeonConfig.LoadFromFile("template.yaml");

for (int i = 0; i < 10; i++)
{
    // Override seed for each variation
    baseConfig.Dungeon.Seed = 1000 + i;
    baseConfig.Dungeon.Name = $"dungeon_variant_{i}";

    var map = DungeonGenerator.Generate(baseConfig);
    map.Save($"dungeons/{baseConfig.Dungeon.Name}.tmx");

    Console.WriteLine($"Generated variant {i} with {map.GetAllRoomBounds().Count} rooms");
}
```

---

## ASCII Dungeon Preview

Print a text representation of your dungeon.

```csharp
var config = DungeonConfig.LoadFromFile("dungeon.yaml");
var map = DungeonGenerator.Generate(config);

// Build spawn lookup for markers
var spawns = map.GetObjectGroup("Spawns")!;
var spawnLookup = new Dictionary<(int x, int y), char>();

foreach (var spawn in spawns.Objects)
{
    int tileX = (int)(spawn.X / map.TileWidth);
    int tileY = (int)(spawn.Y / map.TileHeight);
    char marker = spawn.Type switch
    {
        "spawn" => 'P',
        "boss" => 'B',
        "treasure" => 'T',
        _ => 'S'
    };
    spawnLookup[(tileX, tileY)] = marker;
}

// Print ASCII map
var tileLayer = map.GetTileLayer("Tiles")!;

for (int y = 0; y < map.Height; y++)
{
    for (int x = 0; x < map.Width; x++)
    {
        if (spawnLookup.TryGetValue((x, y), out char spawnChar))
        {
            Console.Write(spawnChar);
        }
        else
        {
            char ch = tileLayer[x, y] switch
            {
                0 => ' ',
                1 => '.',
                2 => '#',
                3 => '+',
                _ => '?'
            };
            Console.Write(ch);
        }
    }
    Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine("Legend: . = Floor, # = Wall, + = Door");
Console.WriteLine("        P = Player, B = Boss, T = Treasure");
```

---

## Level Progression

Generate increasingly difficult dungeons.

```csharp
void GenerateDungeonForLevel(int level)
{
    var config = new DungeonConfig
    {
        Dungeon = new DungeonSettings
        {
            Name = $"level_{level}",
            Width = 30 + level * 5,  // Larger maps at higher levels
            Height = 30 + level * 5,
            Seed = level * 1000
        },
        Rooms = new RoomsConfig
        {
            CountString = $"{4 + level}-{6 + level * 2}",  // More rooms
            Types = new Dictionary<string, RoomTypeConfig>
            {
                ["spawn"] = new() { Count = "1", SizeString = "6x6 to 8x8" },
                ["boss"] = new() { Count = "1", SizeString = $"{8 + level}x{8 + level} to {12 + level}x{12 + level}" },
                ["treasure"] = new() { Count = $"1-{1 + level / 2}", SizeString = "5x5 to 7x7" },
                ["standard"] = new() { Count = "rest", SizeString = "5x5 to 10x10" }
            }
        },
        Tiles = new TilesConfig { Floor = 1, Wall = 2, Door = 3 }
    };

    var map = DungeonGenerator.Generate(config);
    map.Save($"levels/level_{level}.tmx");

    Console.WriteLine($"Level {level}: {map.Width}x{map.Height}, {map.GetAllRoomBounds().Count} rooms");
}

// Generate 10 levels
for (int level = 1; level <= 10; level++)
{
    GenerateDungeonForLevel(level);
}
```

---

## Integration with MonoGame Content Pipeline

Load dungeons as game content.

```csharp
public class DungeonGame : Game
{
    private TmxMap _currentDungeon;
    private int _currentLevel = 1;

    protected override void LoadContent()
    {
        LoadDungeon(_currentLevel);
    }

    private void LoadDungeon(int level)
    {
        string path = Path.Combine(Content.RootDirectory, $"dungeons/level_{level}.tmx");

        if (File.Exists(path))
        {
            // Load pre-generated dungeon
            _currentDungeon = TmxMap.Load(path);
        }
        else
        {
            // Generate on-the-fly
            var config = CreateConfigForLevel(level);
            _currentDungeon = DungeonGenerator.Generate(config);
        }

        InitializeLevel();
    }

    private void InitializeLevel()
    {
        // Set player position
        var spawn = _currentDungeon.GetSpawnPoint("PlayerSpawn");
        if (spawn.HasValue)
            _player.Position = spawn.Value;

        // Spawn boss
        var bossSpawn = _currentDungeon.GetSpawnPoint("BossSpawn");
        if (bossSpawn.HasValue)
            SpawnBoss(bossSpawn.Value);

        // Spawn treasures
        foreach (var pos in _currentDungeon.GetSpawnPointsByType("treasure"))
            SpawnChest(pos);

        // Build collision
        _walls = _currentDungeon.GetCollisionRectangles("Tiles", 2);
    }

    private void OnLevelComplete()
    {
        _currentLevel++;
        LoadDungeon(_currentLevel);
    }
}
```
