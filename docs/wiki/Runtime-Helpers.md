# Runtime Helpers

Using the generated map in your MonoGame game.

The `TmxMap` class provides helper methods that return MonoGame types (`Rectangle`, `Vector2`) for easy integration.

---

## Spawn Points

### Get Player Spawn Position

```csharp
Vector2? playerStart = map.GetSpawnPoint("PlayerSpawn");

if (playerStart.HasValue)
{
    player.Position = playerStart.Value;
}
```

### Get Boss Spawn Position

```csharp
Vector2? bossSpawn = map.GetSpawnPoint("BossSpawn");

if (bossSpawn.HasValue)
{
    SpawnBoss(bossSpawn.Value);
}
```

### Get All Treasure Spawns

```csharp
List<Vector2> treasureLocations = map.GetSpawnPointsByType("treasure");

foreach (var location in treasureLocations)
{
    SpawnTreasureChest(location);
}
```

---

## Collision Detection

### Get Wall Collision Rectangles

```csharp
// Get all wall tiles as collision rectangles
List<Rectangle> walls = map.GetCollisionRectangles("Tiles", tileId: 2);

// Use for collision detection
foreach (var wall in walls)
{
    if (player.Bounds.Intersects(wall))
    {
        // Handle collision
    }
}
```

### Building a Collision Grid

For large maps, you might want a more efficient structure:

```csharp
var tileLayer = map.GetTileLayer("Tiles");
bool[,] walkable = new bool[map.Width, map.Height];

for (int y = 0; y < map.Height; y++)
{
    for (int x = 0; x < map.Width; x++)
    {
        int tileId = tileLayer[x, y];
        // Floor (1) and Door (3) are walkable
        walkable[x, y] = tileId == 1 || tileId == 3;
    }
}
```

---

## Room Queries

### Get All Rooms

```csharp
List<Rectangle> allRooms = map.GetAllRoomBounds();

Console.WriteLine($"Dungeon has {allRooms.Count} rooms");
```

### Get Rooms by Type

```csharp
List<Rectangle> bossRooms = map.GetRoomsByType("boss");
List<Rectangle> treasureRooms = map.GetRoomsByType("treasure");
List<Rectangle> standardRooms = map.GetRoomsByType("standard");
```

### Get Specific Room

```csharp
Rectangle? spawnRoom = map.GetRoomBounds("Room_spawn_0");

if (spawnRoom.HasValue)
{
    // Center camera on spawn room
    camera.CenterOn(spawnRoom.Value.Center.ToVector2());
}
```

### Check Which Room Player Is In

```csharp
Rectangle playerBounds = player.Bounds;

foreach (var room in map.GetAllRoomBounds())
{
    if (room.Contains(playerBounds.Center))
    {
        currentRoom = room;
        break;
    }
}
```

---

## Full MonoGame Integration Example

```csharp
public class DungeonGame : Game
{
    private TmxMap _map;
    private List<Rectangle> _walls;
    private Vector2 _playerPosition;

    protected override void LoadContent()
    {
        // Load dungeon from file
        _map = TmxMap.Load("Content/dungeon.tmx");

        // Cache wall collisions
        _walls = _map.GetCollisionRectangles("Tiles", tileId: 2);

        // Set player start position
        var spawn = _map.GetSpawnPoint("PlayerSpawn");
        _playerPosition = spawn ?? Vector2.Zero;

        // Spawn enemies in boss room
        var bossSpawn = _map.GetSpawnPoint("BossSpawn");
        if (bossSpawn.HasValue)
        {
            SpawnBoss(bossSpawn.Value);
        }

        // Spawn treasure chests
        foreach (var treasurePos in _map.GetSpawnPointsByType("treasure"))
        {
            SpawnChest(treasurePos);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Move player
        var newPosition = _playerPosition + GetInputMovement();

        // Check collisions
        var playerRect = new Rectangle(
            (int)newPosition.X, (int)newPosition.Y, 16, 16);

        bool canMove = true;
        foreach (var wall in _walls)
        {
            if (playerRect.Intersects(wall))
            {
                canMove = false;
                break;
            }
        }

        if (canMove)
        {
            _playerPosition = newPosition;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw tiles
        var tileLayer = _map.GetTileLayer("Tiles");
        for (int y = 0; y < _map.Height; y++)
        {
            for (int x = 0; x < _map.Width; x++)
            {
                int tileId = tileLayer[x, y];
                if (tileId > 0)
                {
                    var destRect = new Rectangle(
                        x * _map.TileWidth,
                        y * _map.TileHeight,
                        _map.TileWidth,
                        _map.TileHeight);

                    // Draw tile from tileset based on tileId
                    DrawTile(tileId, destRect);
                }
            }
        }

        // Draw player
        DrawPlayer(_playerPosition);

        _spriteBatch.End();
    }
}
```

---

## API Reference

### Spawn Point Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetSpawnPoint(name)` | `Vector2?` | Get spawn point by name |
| `GetSpawnPointsByType(type)` | `List<Vector2>` | Get all spawns of a type |

### Room Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetRoomBounds(name)` | `Rectangle?` | Get room bounds by name |
| `GetRoomsByType(type)` | `List<Rectangle>` | Get all rooms of a type |
| `GetAllRoomBounds()` | `List<Rectangle>` | Get all room bounds |

### Collision Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetCollisionRectangles(layer, tileId)` | `List<Rectangle>` | Get all tiles matching ID as rectangles |

All coordinates are in **pixels**, not tiles.
