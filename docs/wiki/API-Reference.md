# API Reference

Complete reference for all public types and methods.

---

## DungeonGenerator

Main entry point for dungeon generation.

**Namespace:** `DeSales.DungeonHelper.Generation`

### Methods

#### Generate

```csharp
public static TmxMap Generate(DungeonConfig config, bool validateConfig = true)
```

Generates a dungeon based on the provided configuration.

| Parameter | Type | Description |
|-----------|------|-------------|
| `config` | `DungeonConfig` | The dungeon configuration |
| `validateConfig` | `bool` | Whether to validate config before generating (default: true) |

**Returns:** `TmxMap` containing the generated dungeon.

**Throws:** `InvalidOperationException` if configuration is invalid and `validateConfig` is true.

```csharp
var config = DungeonConfig.LoadFromFile("dungeon.yaml");
var map = DungeonGenerator.Generate(config);
```

---

## DungeonConfig

Root configuration object.

**Namespace:** `DeSales.DungeonHelper.Configuration`

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Dungeon` | `DungeonSettings` | Basic dungeon settings |
| `Rooms` | `RoomsConfig` | Room configuration |
| `Corridors` | `CorridorsConfig` | Corridor configuration |
| `Tiles` | `TilesConfig?` | Custom tile ID mapping |

### Static Methods

#### LoadFromFile

```csharp
public static DungeonConfig LoadFromFile(string filePath)
```

Loads configuration from a YAML file.

#### ParseFromYaml

```csharp
public static DungeonConfig ParseFromYaml(string yaml)
```

Parses configuration from a YAML string.

### Instance Methods

#### Validate

```csharp
public List<string> Validate()
```

Returns a list of validation error messages. Empty list if valid.

#### ThrowIfInvalid

```csharp
public void ThrowIfInvalid()
```

Throws `InvalidOperationException` if configuration is invalid.

---

## DungeonSettings

Basic dungeon settings.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Name` | `string` | "dungeon" | Dungeon name |
| `Seed` | `int?` | null | Random seed (null for random) |
| `Width` | `int` | 50 | Map width in tiles |
| `Height` | `int` | 50 | Map height in tiles |

---

## RoomsConfig

Room generation settings.

| Property | Type | Description |
|----------|------|-------------|
| `CountString` | `string` | Room count as string (e.g., "8-12") |
| `Count` | `IntRange` | Parsed room count range |
| `Types` | `Dictionary<string, RoomTypeConfig>` | Room type definitions |

---

## RoomTypeConfig

Configuration for a specific room type.

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `string` | Number of rooms (e.g., "1", "2-4", "rest") |
| `SizeString` | `string` | Size range (e.g., "5x5 to 10x10") |
| `Size` | `SizeRange` | Parsed size range |
| `Placement` | `string?` | Placement constraint (not yet implemented) |

---

## TilesConfig

Custom tile ID mapping.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Floor` | `int` | 1 | Floor tile ID |
| `Wall` | `int` | 2 | Wall tile ID |
| `Door` | `int` | 3 | Door tile ID |
| `SpawnPoint` | `int` | 4 | Spawn marker tile ID |
| `BossSpawn` | `int` | 5 | Boss spawn marker tile ID |

---

## TmxMap

Represents a TMX map file.

**Namespace:** `DeSales.DungeonHelper.Tiled`

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Width` | `int` | Map width in tiles |
| `Height` | `int` | Map height in tiles |
| `TileWidth` | `int` | Tile width in pixels |
| `TileHeight` | `int` | Tile height in pixels |
| `TileLayers` | `IReadOnlyList<TmxTileLayer>` | All tile layers |
| `ObjectGroups` | `IReadOnlyList<TmxObjectGroup>` | All object groups |

### Static Methods

#### Load

```csharp
public static TmxMap Load(string path)
```

Loads a TMX map from file.

#### FromXml

```csharp
public static TmxMap FromXml(string xml)
```

Parses a TMX map from XML string.

### Instance Methods

#### Save

```csharp
public void Save(string path)
```

Saves the map to a TMX file.

#### ToXml

```csharp
public string ToXml()
```

Converts the map to TMX XML string.

#### GetTileLayer

```csharp
public TmxTileLayer? GetTileLayer(string name)
```

Gets a tile layer by name.

#### GetObjectGroup

```csharp
public TmxObjectGroup? GetObjectGroup(string name)
```

Gets an object group by name.

### Runtime Helper Methods

#### GetSpawnPoint

```csharp
public Vector2? GetSpawnPoint(string name)
```

Gets a spawn point position by name.

```csharp
var playerStart = map.GetSpawnPoint("PlayerSpawn");
```

#### GetSpawnPointsByType

```csharp
public List<Vector2> GetSpawnPointsByType(string type)
```

Gets all spawn points of a specific type.

```csharp
var treasures = map.GetSpawnPointsByType("treasure");
```

#### GetCollisionRectangles

```csharp
public List<Rectangle> GetCollisionRectangles(string layerName, int tileId)
```

Gets rectangles for all tiles matching the specified ID.

```csharp
var walls = map.GetCollisionRectangles("Tiles", tileId: 2);
```

#### GetRoomBounds

```csharp
public Rectangle? GetRoomBounds(string name)
```

Gets a room's bounding rectangle by name.

#### GetRoomsByType

```csharp
public List<Rectangle> GetRoomsByType(string type)
```

Gets all rooms of a specific type.

#### GetAllRoomBounds

```csharp
public List<Rectangle> GetAllRoomBounds()
```

Gets all room bounding rectangles.

---

## TmxTileLayer

A 2D grid of tile IDs.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Layer name |
| `Width` | `int` | Layer width in tiles |
| `Height` | `int` | Layer height in tiles |

### Indexer

```csharp
public int this[int x, int y] { get; set; }
```

Gets or sets the tile ID at the specified position.

```csharp
int tileId = layer[5, 10];
layer[5, 10] = 1; // Set to floor
```

---

## TmxObjectGroup

A collection of map objects.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Group name |
| `Objects` | `IReadOnlyList<TmxObject>` | All objects in the group |

### Methods

#### AddObject (Point)

```csharp
public TmxObject AddObject(string name, string type, float x, float y)
```

Adds a point object.

#### AddObject (Rectangle)

```csharp
public TmxObject AddObject(string name, string type, float x, float y, float width, float height)
```

Adds a rectangle object.

---

## TmxObject

A map object (point or rectangle).

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Unique object ID |
| `Name` | `string` | Object name |
| `Type` | `string` | Object type |
| `X` | `float` | X position in pixels |
| `Y` | `float` | Y position in pixels |
| `Width` | `float?` | Width in pixels (null for points) |
| `Height` | `float?` | Height in pixels (null for points) |
| `Properties` | `IReadOnlyDictionary<string, string>` | Custom properties |

### Methods

#### SetProperty

```csharp
public void SetProperty(string name, string value)
```

Sets a custom property.

---

## Helper Types

### IntRange

Represents a range of integers.

| Property | Type | Description |
|----------|------|-------------|
| `Min` | `int` | Minimum value |
| `Max` | `int` | Maximum value |

```csharp
var range = IntRange.Parse("8-12");
// range.Min = 8, range.Max = 12
```

### SizeRange

Represents a range of sizes.

| Property | Type | Description |
|----------|------|-------------|
| `MinWidth` | `int` | Minimum width |
| `MinHeight` | `int` | Minimum height |
| `MaxWidth` | `int` | Maximum width |
| `MaxHeight` | `int` | Maximum height |

```csharp
var size = SizeRange.Parse("5x5 to 10x10");
```
