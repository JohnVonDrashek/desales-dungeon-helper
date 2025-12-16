# DeSales Dungeon Helper

A procedural dungeon generation library for MonoGame games. Define your dungeon layout in human-readable YAML, get industry-standard TMX output.

## Features

- **YAML Configuration** - Define dungeons in simple, readable config files
- **TMX Output** - Standard Tiled Map Editor format, works with any game engine
- **Seed-based Determinism** - Same seed = same dungeon, every time
- **Room Types** - Spawn rooms, boss rooms, treasure rooms, and custom types
- **Automatic Corridors** - Rooms connected via minimum spanning tree algorithm
- **Runtime Helpers** - Query spawn points, collision rectangles, and room bounds

## Quick Example

```yaml
dungeon:
  name: my_dungeon
  seed: 12345
  width: 50
  height: 50

rooms:
  count: 6-8
  types:
    spawn:
      count: 1
      size: 6x6 to 8x8
    boss:
      count: 1
      size: 10x10 to 12x12
    standard:
      count: rest
      size: 5x5 to 8x8
```

```csharp
var config = DungeonConfig.LoadFromFile("dungeon.yaml");
var map = DungeonGenerator.Generate(config);
map.Save("output/my_dungeon.tmx");

// Use in your game
var playerStart = map.GetSpawnPoint("PlayerSpawn");
var walls = map.GetCollisionRectangles("Tiles", wallTileId: 2);
```

## Documentation

- [[Getting Started]] - Installation and your first dungeon
- [[Configuration]] - Complete YAML schema reference
- [[TMX Output]] - Understanding the generated map format
- [[Runtime Helpers]] - Using the map in your game
- [[API Reference]] - Full API documentation
- [[Examples]] - Complete working examples

## Installation

```bash
dotnet add package DeSales.DungeonHelper
```

Or add a project reference if building from source.

## Requirements

- .NET 10.0 or later
- MonoGame 3.8.2 or later (for runtime helpers using `Rectangle` and `Vector2`)

## License

MIT License - See the repository for details.
