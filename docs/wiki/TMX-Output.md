# TMX Output Format

Understanding the generated TMX map file.

## What is TMX?

TMX is the native format for [Tiled Map Editor](https://www.mapeditor.org/), an industry-standard tool for 2D tile maps. The format is XML-based and widely supported by game engines.

You can open generated `.tmx` files directly in Tiled to visualize and inspect your dungeons.

## Generated Structure

Each generated dungeon contains:

```
TmxMap
├── Tile Layer: "Tiles"       (floor, wall, door tile IDs)
├── Object Group: "Rooms"     (room boundary rectangles)
└── Object Group: "Spawns"    (spawn point markers)
```

---

## Tile Layer: "Tiles"

A 2D grid of tile IDs representing the dungeon geometry.

### Default Tile IDs

| ID | Meaning |
|----|---------|
| 0 | Empty (void) |
| 1 | Floor |
| 2 | Wall |
| 3 | Door |

These can be customized in the [[Configuration|configuration]] `tiles` section.

### Accessing Tile Data

```csharp
var tileLayer = map.GetTileLayer("Tiles");

// Get tile at position
int tileId = tileLayer[x, y];

// Check if walkable
bool isFloor = tileId == 1;
bool isDoor = tileId == 3;
bool isWalkable = tileId == 1 || tileId == 3;
```

---

## Object Group: "Rooms"

Contains rectangle objects marking each room's boundaries.

### Room Object Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | string | Unique identifier (e.g., "Room_spawn_0") |
| `Type` | string | Room type (e.g., "spawn", "boss", "treasure") |
| `X`, `Y` | float | Position in pixels |
| `Width`, `Height` | float | Size in pixels |

### Accessing Room Data

```csharp
var rooms = map.GetObjectGroup("Rooms");

foreach (var room in rooms.Objects)
{
    Console.WriteLine($"{room.Name} ({room.Type}): {room.Width}x{room.Height} at ({room.X}, {room.Y})");
}
```

Or use the runtime helpers:

```csharp
// Get all rooms of a type
var bossRooms = map.GetRoomsByType("boss");

// Get a specific room by name
var spawnRoom = map.GetRoomBounds("Room_spawn_0");
```

---

## Object Group: "Spawns"

Contains point objects marking spawn locations.

### Spawn Point Names

| Name | Type | Description |
|------|------|-------------|
| `PlayerSpawn` | spawn | Player starting position |
| `BossSpawn` | boss | Boss enemy spawn location |
| `TreasureSpawn_0`, `TreasureSpawn_1`, ... | treasure | Treasure chest locations |

### Accessing Spawn Points

```csharp
var spawns = map.GetObjectGroup("Spawns");

foreach (var spawn in spawns.Objects)
{
    Console.WriteLine($"{spawn.Name} ({spawn.Type}) at ({spawn.X}, {spawn.Y})");
}
```

Or use the runtime helpers:

```csharp
// Get player spawn
var playerStart = map.GetSpawnPoint("PlayerSpawn");
if (playerStart.HasValue)
{
    player.Position = playerStart.Value;
}

// Get all treasure locations
var treasures = map.GetSpawnPointsByType("treasure");
```

---

## Coordinate System

### Pixel Coordinates

Object positions (`X`, `Y`, `Width`, `Height`) are in **pixel coordinates**.

```
Pixel Position = Tile Position × TileSize
```

Default tile size is 16×16 pixels.

### Converting to Tile Coordinates

```csharp
int tileX = (int)(pixelX / map.TileWidth);
int tileY = (int)(pixelY / map.TileHeight);
```

### Example

A room at tile position (5, 10) with size 8×6 tiles:
- `X` = 5 × 16 = 80 pixels
- `Y` = 10 × 16 = 160 pixels
- `Width` = 8 × 16 = 128 pixels
- `Height` = 6 × 16 = 96 pixels

---

## TMX XML Structure

The generated XML follows this structure:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<map version="1.10" tiledversion="1.10.2" orientation="orthogonal"
     renderorder="right-down" width="50" height="50"
     tilewidth="16" tileheight="16" infinite="0">

  <tileset firstgid="1" name="dungeon" tilewidth="16" tileheight="16"
           tilecount="256" columns="16"/>

  <layer name="Tiles" width="50" height="50">
    <data encoding="csv">
      0,0,0,0,0,...
    </data>
  </layer>

  <objectgroup name="Rooms">
    <object id="1" name="Room_spawn_0" type="spawn"
            x="80" y="160" width="128" height="96"/>
  </objectgroup>

  <objectgroup name="Spawns">
    <object id="10" name="PlayerSpawn" type="spawn" x="144" y="208"/>
  </objectgroup>
</map>
```

---

## Loading and Saving

### Save to File

```csharp
map.Save("output/dungeon.tmx");
```

### Load from File

```csharp
var map = TmxMap.Load("output/dungeon.tmx");
```

### Parse from String

```csharp
var map = TmxMap.FromXml(xmlString);
```

### Get XML String

```csharp
string xml = map.ToXml();
```

---

## Opening in Tiled

1. Download [Tiled](https://www.mapeditor.org/)
2. Open the generated `.tmx` file
3. Create a tileset image if you want visual tiles
4. Inspect layers, objects, and properties

The map will display with placeholder graphics until you assign a tileset image.
