# Configuration Reference

Complete YAML schema for dungeon configuration.

## Full Example

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
      placement: far_from_spawn
    treasure:
      count: 1-2
      size: 5x5 to 8x8
      placement: near_center
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

---

## Dungeon Section

Basic dungeon settings.

```yaml
dungeon:
  name: my_dungeon      # Name used for output file
  seed: 12345           # Random seed (omit for random)
  width: 50             # Map width in tiles (minimum: 10)
  height: 50            # Map height in tiles (minimum: 10)
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `name` | string | "dungeon" | Used for the TMX filename |
| `seed` | int? | null | Seed for reproducible generation. Omit for random. |
| `width` | int | 50 | Map width in tiles. Must be at least 10. |
| `height` | int | 50 | Map height in tiles. Must be at least 10. |

---

## Rooms Section

Controls room generation.

```yaml
rooms:
  count: 8-12           # Range or single value
  types:
    spawn: ...
    boss: ...
```

### Count Syntax

Room counts support ranges or single values:

| Syntax | Meaning |
|--------|---------|
| `count: 8-12` | Generate between 8 and 12 rooms |
| `count: 10` | Generate exactly 10 rooms |

### Room Types

Each room type defines how many rooms of that type to create and their size range.

```yaml
types:
  spawn:
    count: 1
    size: 6x6 to 8x8
  boss:
    count: 1
    size: 10x10 to 15x15
    placement: far_from_spawn
  treasure:
    count: 1-2
    size: 5x5 to 7x7
  standard:
    count: rest
    size: 5x5 to 10x10
```

#### Type Properties

| Property | Type | Description |
|----------|------|-------------|
| `count` | string | Number of rooms: `"1"`, `"2-4"`, or `"rest"` |
| `size` | string | Size range: `"5x5"` or `"5x5 to 10x10"` |
| `placement` | string? | Placement hint (not yet fully implemented) |

#### Special Count Value: "rest"

Use `count: rest` to fill remaining room slots. After all fixed-count rooms are generated, "rest" rooms fill the remaining count.

Example: If total count is 8-12, and you have:
- spawn: 1
- boss: 1
- treasure: 1-2

Then standard rooms with `count: rest` will generate 5-9 rooms.

#### Size Syntax

| Syntax | Meaning |
|--------|---------|
| `size: 8x8` | Fixed 8x8 tiles |
| `size: 5x5 to 10x10` | Random size between 5x5 and 10x10 |
| `size: 5x8 to 10x12` | Width 5-10, height 8-12 |

**Note:** Minimum room size is 3x3 (walls + at least 1 floor tile).

#### Built-in Room Types

These types have special handling:

| Type | Description |
|------|-------------|
| `spawn` | Player starting room. Gets a "PlayerSpawn" point. |
| `boss` | Boss encounter room. Gets a "BossSpawn" point. |
| `treasure` | Loot room. Gets "TreasureSpawn_N" points. |
| `standard` | Regular dungeon room. No special spawn point. |

You can define custom types with any name - they work like `standard` rooms.

---

## Corridors Section

Controls corridor generation.

```yaml
corridors:
  style: winding
  width: 1
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `style` | string | "winding" | Corridor style (currently only "winding" supported) |
| `width` | int | 1 | Corridor width in tiles |

**Note:** Corridors use Prim's minimum spanning tree algorithm to ensure all rooms are connected with minimal total corridor length.

---

## Tiles Section

Custom tile ID mapping.

```yaml
tiles:
  floor: 1
  wall: 2
  door: 3
  spawn_point: 4
  boss_spawn: 5
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `floor` | int | 1 | Tile ID for floor tiles |
| `wall` | int | 2 | Tile ID for wall tiles |
| `door` | int | 3 | Tile ID for door tiles |
| `spawn_point` | int | 4 | Tile ID for spawn markers (not currently used) |
| `boss_spawn` | int | 5 | Tile ID for boss spawn markers (not currently used) |

These IDs should match your tileset in the game engine.

---

## Validation

The library validates configurations before generating. Common errors:

| Error | Cause |
|-------|-------|
| "width must be at least 10" | Dungeon too small |
| "minimum width must be at least 3" | Room size too small |
| "maximum width exceeds dungeon width" | Room bigger than map |
| "fixed room counts exceeds maximum" | Too many required rooms |

Use `config.Validate()` to get a list of errors, or `config.ThrowIfInvalid()` to throw on first error.

```csharp
var errors = config.Validate();
if (errors.Count > 0)
{
    foreach (var error in errors)
        Console.WriteLine(error);
}
```

---

## Loading Configuration

### From YAML File

```csharp
var config = DungeonConfig.LoadFromFile("dungeon.yaml");
```

### From YAML String

```csharp
var yaml = @"
dungeon:
  name: test
  width: 30
  height: 30
rooms:
  count: 4
  types:
    spawn:
      count: 1
      size: 5x5
    standard:
      count: rest
      size: 5x5
";

var config = DungeonConfig.ParseFromYaml(yaml);
```

### Programmatically

```csharp
var config = new DungeonConfig
{
    Dungeon = new DungeonSettings
    {
        Name = "my_dungeon",
        Width = 40,
        Height = 40,
        Seed = 42
    },
    Rooms = new RoomsConfig
    {
        CountString = "5-8",
        Types = new Dictionary<string, RoomTypeConfig>
        {
            ["spawn"] = new() { Count = "1", SizeString = "6x6 to 8x8" },
            ["standard"] = new() { Count = "rest", SizeString = "5x5 to 10x10" }
        }
    },
    Tiles = new TilesConfig { Floor = 1, Wall = 2, Door = 3 }
};
```
