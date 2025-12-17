using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Generation;

namespace DeSales.DungeonHelper.Tests.Generation;

public class DungeonGeneratorTests
{
    [Fact]
    public void Generate_WithConfig_ReturnsValidTmxMap()
    {
        // Arrange
        var config = CreateSimpleConfig();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        map.Should().NotBeNull();
        map.Width.Should().Be(50);
        map.Height.Should().Be(50);
    }

    [Fact]
    public void Generate_CreatesTileLayer()
    {
        // Arrange
        var config = CreateSimpleConfig();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        map.TileLayers.Should().HaveCountGreaterOrEqualTo(1);
        map.GetTileLayer("Tiles").Should().NotBeNull();
    }

    [Fact]
    public void Generate_CreatesRoomsObjectGroup()
    {
        // Arrange
        var config = CreateSimpleConfig();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var roomsGroup = map.GetObjectGroup("Rooms");
        roomsGroup.Should().NotBeNull();
        roomsGroup!.Objects.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public void Generate_CreatesSpawnsObjectGroup()
    {
        // Arrange
        var config = CreateSimpleConfig();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var spawnsGroup = map.GetObjectGroup("Spawns");
        spawnsGroup.Should().NotBeNull();
    }

    [Fact]
    public void Generate_WithSeed_IsDeterministic()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 12345;

        // Act
        var map1 = DungeonGenerator.Generate(config);
        var map2 = DungeonGenerator.Generate(config);

        // Assert - both maps should have identical tile data
        var layer1 = map1.GetTileLayer("Tiles")!;
        var layer2 = map2.GetTileLayer("Tiles")!;

        layer1.ToCsv().Should().Be(layer2.ToCsv());
    }

    [Fact]
    public void Generate_WithoutSeed_ProducesDifferentResults()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = null;

        // Act - generate multiple times
        var maps = Enumerable.Range(0, 5).Select(_ => DungeonGenerator.Generate(config)).ToList();

        // Assert - at least some should be different (probabilistically)
        var csvs = maps.Select(m => m.GetTileLayer("Tiles")!.ToCsv()).ToList();
        csvs.Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Generate_CreatesSpawnRoom()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        var spawnRoom = roomsGroup.Objects.FirstOrDefault(o => o.Type == "spawn");
        spawnRoom.Should().NotBeNull();
    }

    [Fact]
    public void Generate_RoomsAreWithinBounds()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        foreach (var room in roomsGroup.Objects)
        {
            // Convert pixel coordinates back to tile coordinates
            var tileX = room.X / 16;
            var tileY = room.Y / 16;
            var tileW = room.Width!.Value / 16;
            var tileH = room.Height!.Value / 16;

            tileX.Should().BeGreaterOrEqualTo(0);
            tileY.Should().BeGreaterOrEqualTo(0);
            (tileX + tileW).Should().BeLessOrEqualTo(map.Width);
            (tileY + tileH).Should().BeLessOrEqualTo(map.Height);
        }
    }

    [Fact]
    public void Generate_RoomsDoNotOverlap()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        var rooms = roomsGroup.Objects.ToList();

        for (var i = 0; i < rooms.Count; i++)
        {
            for (var j = i + 1; j < rooms.Count; j++)
            {
                var r1 = rooms[i];
                var r2 = rooms[j];

                // Check if rectangles overlap
                var overlaps = r1.X < r2.X + r2.Width!.Value &&
                               r1.X + r1.Width!.Value > r2.X &&
                               r1.Y < r2.Y + r2.Height!.Value &&
                               r1.Y + r1.Height!.Value > r2.Y;

                overlaps.Should().BeFalse($"Room {r1.Name} and {r2.Name} should not overlap");
            }
        }
    }

    [Fact]
    public void Generate_RoomsHaveWallsAndFloors()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var tileLayer = map.GetTileLayer("Tiles")!;
        var roomsGroup = map.GetObjectGroup("Rooms")!;

        foreach (var room in roomsGroup.Objects)
        {
            var tileX = (int)(room.X / 16);
            var tileY = (int)(room.Y / 16);
            var tileW = (int)(room.Width!.Value / 16);
            var tileH = (int)(room.Height!.Value / 16);

            // Check walls on edges
            tileLayer[tileX, tileY].Should().Be(2, "top-left corner should be wall");
            tileLayer[tileX + tileW - 1, tileY].Should().Be(2, "top-right corner should be wall");

            // Check floor in interior (if room is large enough)
            if (tileW > 2 && tileH > 2)
            {
                tileLayer[tileX + 1, tileY + 1].Should().Be(1, "interior should be floor");
            }
        }
    }

    [Fact]
    public void Generate_CreatesPlayerSpawnPoint()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var spawnsGroup = map.GetObjectGroup("Spawns")!;
        var playerSpawn = spawnsGroup.Objects.FirstOrDefault(o => o.Type == "spawn");
        playerSpawn.Should().NotBeNull();
        playerSpawn!.Name.Should().Be("PlayerSpawn");
    }

    [Fact]
    public void Generate_RoomCountWithinConfiguredRange()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Rooms.CountString = "3-5";
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        roomsGroup.Objects.Should().HaveCountGreaterOrEqualTo(3);
        roomsGroup.Objects.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    public void Generate_RoomsAreDirectlyAdjacent()
    {
        // Arrange - Isaac-style generation creates adjacent rooms (no separate corridors)
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - rooms should be adjacent (sharing walls with doors)
        var tileLayer = map.GetTileLayer("Tiles")!;
        var doorCount = 0;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 3) // Door tile
                {
                    doorCount++;
                }
            }
        }

        // With Isaac-style, rooms are connected via doors on shared walls
        doorCount.Should().BeGreaterThan(0, "adjacent rooms should be connected via door tiles");
    }

    [Fact]
    public void Generate_AllRoomsAreReachable()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - use flood fill from spawn room to verify all rooms are reachable
        var tileLayer = map.GetTileLayer("Tiles")!;
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        var spawnsGroup = map.GetObjectGroup("Spawns")!;

        var playerSpawn = spawnsGroup.Objects.First(o => o.Name == "PlayerSpawn");
        var startX = (int)(playerSpawn.X / 16);
        var startY = (int)(playerSpawn.Y / 16);

        // Flood fill to find all reachable floor tiles
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();
        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            if (visited.Contains((x, y)))
            {
                continue;
            }

            if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
            {
                continue;
            }

            var tile = tileLayer[x, y];
            if (tile != 1 && tile != 3) // Not floor or door
            {
                continue;
            }

            visited.Add((x, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x + 1, y));
            queue.Enqueue((x, y - 1));
            queue.Enqueue((x, y + 1));
        }

        // Check that we can reach at least one tile in each room
        foreach (var room in roomsGroup.Objects)
        {
            var rx = (int)(room.X / 16);
            var ry = (int)(room.Y / 16);
            var rw = (int)(room.Width!.Value / 16);
            var rh = (int)(room.Height!.Value / 16);

            // Check interior of room (skip walls)
            var roomReachable = false;
            for (var x = rx + 1; x < rx + rw - 1 && !roomReachable; x++)
            {
                for (var y = ry + 1; y < ry + rh - 1 && !roomReachable; y++)
                {
                    if (visited.Contains((x, y)))
                    {
                        roomReachable = true;
                    }
                }
            }

            roomReachable.Should().BeTrue($"Room {room.Name} should be reachable from spawn");
        }
    }

    [Fact]
    public void Generate_CorridorsHaveDoors()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - there should be door tiles where corridors meet rooms
        var tileLayer = map.GetTileLayer("Tiles")!;
        var doorCount = 0;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 3) // Door tile
                {
                    doorCount++;
                }
            }
        }

        doorCount.Should().BeGreaterThan(0, "there should be door tiles where corridors meet rooms");
    }

    [Fact]
    public void Generate_CreatesBossSpawnInBossRoom()
    {
        // Arrange
        var config = CreateConfigWithAllRoomTypes();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var spawnsGroup = map.GetObjectGroup("Spawns")!;
        var bossSpawn = spawnsGroup.Objects.FirstOrDefault(o => o.Type == "boss");
        bossSpawn.Should().NotBeNull("there should be a boss spawn point");
        bossSpawn!.Name.Should().Be("BossSpawn");

        // Verify it's inside the boss room
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        var bossRoom = roomsGroup.Objects.First(o => o.Type == "boss");
        bossSpawn.X.Should().BeGreaterOrEqualTo(bossRoom.X);
        bossSpawn.X.Should().BeLessThan(bossRoom.X + bossRoom.Width!.Value);
        bossSpawn.Y.Should().BeGreaterOrEqualTo(bossRoom.Y);
        bossSpawn.Y.Should().BeLessThan(bossRoom.Y + bossRoom.Height!.Value);
    }

    [Fact]
    public void Generate_CreatesTreasureSpawnsInTreasureRooms()
    {
        // Arrange
        var config = CreateConfigWithAllRoomTypes();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert
        var spawnsGroup = map.GetObjectGroup("Spawns")!;
        var treasureSpawns = spawnsGroup.Objects.Where(o => o.Type == "treasure").ToList();
        treasureSpawns.Should().HaveCountGreaterOrEqualTo(1, "there should be at least one treasure spawn");

        // Verify each treasure spawn is inside a treasure room
        var roomsGroup = map.GetObjectGroup("Rooms")!;
        var treasureRooms = roomsGroup.Objects.Where(o => o.Type == "treasure").ToList();

        foreach (var spawn in treasureSpawns)
        {
            var isInsideTreasureRoom = treasureRooms.Any(room =>
                spawn.X >= room.X &&
                spawn.X < room.X + room.Width!.Value &&
                spawn.Y >= room.Y &&
                spawn.Y < room.Y + room.Height!.Value);

            isInsideTreasureRoom.Should().BeTrue($"Treasure spawn {spawn.Name} should be inside a treasure room");
        }
    }

    [Fact]
    public void Generate_SpawnPointsAreOnFloorTiles()
    {
        // Arrange
        var config = CreateConfigWithAllRoomTypes();

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - all spawn points should be on floor tiles
        var spawnsGroup = map.GetObjectGroup("Spawns")!;
        var tileLayer = map.GetTileLayer("Tiles")!;

        foreach (var spawn in spawnsGroup.Objects)
        {
            var tileX = (int)(spawn.X / 16);
            var tileY = (int)(spawn.Y / 16);
            var tile = tileLayer[tileX, tileY];

            tile.Should().Be(1, $"Spawn point {spawn.Name} should be on a floor tile, not tile {tile}");
        }
    }

    private static DungeonConfig CreateSimpleConfig()
    {
        return new DungeonConfig
        {
            Dungeon = new DungeonSettings
            {
                Name = "test_dungeon",
                Width = 50,
                Height = 50,
                Seed = 42
            },
            Rooms = new RoomsConfig
            {
                CountString = "4-6",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new RoomTypeConfig { Count = "1", SizeString = "6x6 to 8x8" },
                    ["standard"] = new RoomTypeConfig { Count = "rest", SizeString = "5x5 to 10x10" }
                }
            },
            Tiles = new TilesConfig
            {
                Floor = 1,
                Wall = 2,
                Door = 3
            }
        };
    }

    private static DungeonConfig CreateConfigWithAllRoomTypes()
    {
        return new DungeonConfig
        {
            Dungeon = new DungeonSettings
            {
                Name = "test_dungeon",
                Width = 60,
                Height = 60,
                Seed = 42
            },
            Rooms = new RoomsConfig
            {
                CountString = "6-8",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new RoomTypeConfig { Count = "1", SizeString = "6x6 to 8x8" },
                    ["boss"] = new RoomTypeConfig { Count = "1", SizeString = "10x10 to 12x12" },
                    ["treasure"] = new RoomTypeConfig { Count = "1-2", SizeString = "5x5 to 7x7" },
                    ["standard"] = new RoomTypeConfig { Count = "rest", SizeString = "5x5 to 10x10" }
                }
            },
            Tiles = new TilesConfig
            {
                Floor = 1,
                Wall = 2,
                Door = 3
            }
        };
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Generate_DoorWidthIsRespected(int doorWidth)
    {
        // Arrange - Isaac-style uses door width for room connections
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings
            {
                Name = "test_dungeon",
                Width = 60,
                Height = 60,
                Seed = 12345
            },
            Rooms = new RoomsConfig
            {
                CountString = "3",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new RoomTypeConfig { Count = "1", SizeString = "8x8" },
                    ["standard"] = new RoomTypeConfig { Count = "rest", SizeString = "8x8" }
                }
            },
            Corridors = new CorridorsConfig
            {
                Style = "winding",
                Width = 1,
                Doors = doorWidth
            },
            Tiles = new TilesConfig
            {
                Floor = 1,
                Wall = 2,
                Door = 3
            }
        };

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - count door tiles
        var tileLayer = map.GetTileLayer("Tiles")!;
        var doorTiles = new List<(int x, int y)>();

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 3) // Door tile
                {
                    doorTiles.Add((x, y));
                }
            }
        }

        doorTiles.Should().NotBeEmpty("there should be door tiles between rooms");

        // Wider door widths should produce more door tiles
        if (doorWidth > 1)
        {
            // Count door tiles that are adjacent to other door tiles
            var adjacentDoors = doorTiles.Count(tile =>
                doorTiles.Contains((tile.x - 1, tile.y)) ||
                doorTiles.Contains((tile.x + 1, tile.y)) ||
                doorTiles.Contains((tile.x, tile.y - 1)) ||
                doorTiles.Contains((tile.x, tile.y + 1)));

            adjacentDoors.Should().BeGreaterThan(0,
                $"wider doors (width={doorWidth}) should have adjacent door tiles");
        }
    }

    [Fact]
    public void Generate_WiderCorridorsHaveMoreDoors()
    {
        // Arrange - compare door counts between width 1 and width 3
        var config1 = CreateSimpleConfig();
        config1.Corridors = new CorridorsConfig { Style = "winding", Width = 1 };

        var config3 = CreateSimpleConfig();
        config3.Corridors = new CorridorsConfig { Style = "winding", Width = 3 };

        // Act
        var map1 = DungeonGenerator.Generate(config1);
        var map3 = DungeonGenerator.Generate(config3);

        // Count doors in each map
        var doors1 = CountDoorsInMap(map1);
        var doors3 = CountDoorsInMap(map3);

        // Assert - wider corridors should have more door tiles
        doors3.Should().BeGreaterThan(doors1,
            "corridors with width 3 should have more door tiles than width 1");
    }

    [Fact]
    public void Generate_DoorWidthCanDifferFromCorridorWidth()
    {
        // Arrange - corridor width 1, door width 3 (for larger player sprites)
        var config = CreateSimpleConfig();
        config.Corridors = new CorridorsConfig { Style = "winding", Width = 1, Doors = 3 };

        // Compare to corridor width 1 with default door width (1)
        var configDefault = CreateSimpleConfig();
        configDefault.Corridors = new CorridorsConfig { Style = "winding", Width = 1 };

        // Act
        var mapWithWideDoors = DungeonGenerator.Generate(config);
        var mapWithDefaultDoors = DungeonGenerator.Generate(configDefault);

        // Count doors in each map
        var doorsWide = CountDoorsInMap(mapWithWideDoors);
        var doorsDefault = CountDoorsInMap(mapWithDefaultDoors);

        // Assert - wider doors should have more door tiles even with narrow corridors
        doorsWide.Should().BeGreaterThan(doorsDefault,
            "door width 3 should have more door tiles than default door width 1");
    }

    [Fact]
    public void Generate_AllRoomEntriesHaveDoors()
    {
        // Arrange - config matching the problematic wide-doors.yaml
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings
            {
                Name = "door_test",
                Width = 50,
                Height = 40,
                Seed = 12345
            },
            Rooms = new RoomsConfig
            {
                CountString = "4-6",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new RoomTypeConfig { Count = "1", SizeString = "8x8 to 10x10" },
                    ["boss"] = new RoomTypeConfig { Count = "1", SizeString = "12x12 to 14x14" },
                    ["standard"] = new RoomTypeConfig { Count = "rest", SizeString = "6x6 to 10x10" }
                }
            },
            Corridors = new CorridorsConfig { Style = "winding", Width = 1, Doors = 3 },
            Tiles = new TilesConfig { Floor = 1, Wall = 2, Door = 3 }
        };

        // Act
        var map = DungeonGenerator.Generate(config);
        var tileLayer = map.GetTileLayer("Tiles")!;
        var roomsGroup = map.GetObjectGroup("Rooms")!;

        // Assert - every room should have at least one door on its perimeter
        foreach (var room in roomsGroup.Objects)
        {
            var rx = (int)(room.X / 16);
            var ry = (int)(room.Y / 16);
            var rw = (int)(room.Width!.Value / 16);
            var rh = (int)(room.Height!.Value / 16);

            // Check all wall positions for at least one door
            var hasDoor = false;

            // Top and bottom walls
            for (var x = rx + 1; x < rx + rw - 1; x++)
            {
                if (tileLayer[x, ry] == 3 || tileLayer[x, ry + rh - 1] == 3)
                {
                    hasDoor = true;
                    break;
                }
            }

            // Left and right walls
            if (!hasDoor)
            {
                for (var y = ry + 1; y < ry + rh - 1; y++)
                {
                    if (tileLayer[rx, y] == 3 || tileLayer[rx + rw - 1, y] == 3)
                    {
                        hasDoor = true;
                        break;
                    }
                }
            }

            hasDoor.Should().BeTrue($"Room {room.Name} should have at least one door");
        }
    }

    [Fact]
    public void Generate_DoorWidthDefaultsToCorridorWidth()
    {
        // Arrange - corridor width 2, no door width specified
        var config = CreateSimpleConfig();
        config.Corridors = new CorridorsConfig { Style = "winding", Width = 2 };

        // Explicitly set door width to match corridor width
        var configExplicit = CreateSimpleConfig();
        configExplicit.Corridors = new CorridorsConfig { Style = "winding", Width = 2, Doors = 2 };

        // Act
        var mapImplicit = DungeonGenerator.Generate(config);
        var mapExplicit = DungeonGenerator.Generate(configExplicit);

        // Count doors in each map
        var doorsImplicit = CountDoorsInMap(mapImplicit);
        var doorsExplicit = CountDoorsInMap(mapExplicit);

        // Assert - should be equal since default door width equals corridor width
        doorsImplicit.Should().Be(doorsExplicit,
            "implicit door width should equal explicit door width when both match corridor width");
    }

    private static int CountDoorsInMap(DeSales.DungeonHelper.Tiled.TmxMap map)
    {
        var tileLayer = map.GetTileLayer("Tiles")!;
        var doorCount = 0;
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 3)
                {
                    doorCount++;
                }
            }
        }
        return doorCount;
    }

    [Fact]
    public void Generate_ExteriorWalls_FillsMapWithWalls()
    {
        // Arrange - default exterior is "walls"
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - count wall tiles outside rooms and corridors
        var tileLayer = map.GetTileLayer("Tiles")!;
        var wallCount = 0;
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 2) // Wall tile
                {
                    wallCount++;
                }
            }
        }

        // With exterior: walls, most of the map should be walls
        var totalTiles = map.Width * map.Height;
        wallCount.Should().BeGreaterThan(totalTiles / 2,
            "with exterior: walls (default), most of the map should be walls");
    }

    [Fact]
    public void Generate_ExteriorVoid_LeavesEmptySpace()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;
        config.Dungeon.Exterior = "void";

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - count empty tiles outside rooms and corridors
        var tileLayer = map.GetTileLayer("Tiles")!;
        var emptyCount = 0;
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 0) // Empty tile
                {
                    emptyCount++;
                }
            }
        }

        // With exterior: void, there should be many empty tiles
        var totalTiles = map.Width * map.Height;
        emptyCount.Should().BeGreaterThan(totalTiles / 2,
            "with exterior: void, most of the map should be empty (tile 0)");
    }

    [Fact]
    public void Generate_ExteriorVoid_RoomsStillHaveWalls()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;
        config.Dungeon.Exterior = "void";

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - floor tiles should have wall neighbors (rooms have walls)
        var tileLayer = map.GetTileLayer("Tiles")!;
        var floorTilesWithWallNeighbors = 0;
        var totalFloorTiles = 0;

        for (var y = 1; y < map.Height - 1; y++)
        {
            for (var x = 1; x < map.Width - 1; x++)
            {
                if (tileLayer[x, y] == 1) // Floor tile
                {
                    totalFloorTiles++;
                    // Check if any neighbor is a wall
                    var hasWallNeighbor =
                        tileLayer[x - 1, y] == 2 ||
                        tileLayer[x + 1, y] == 2 ||
                        tileLayer[x, y - 1] == 2 ||
                        tileLayer[x, y + 1] == 2;

                    if (hasWallNeighbor)
                    {
                        floorTilesWithWallNeighbors++;
                    }
                }
            }
        }

        // In void mode, floors should still have wall neighbors (room walls)
        totalFloorTiles.Should().BeGreaterThan(0, "there should be floor tiles");
        var ratio = (double)floorTilesWithWallNeighbors / totalFloorTiles;
        ratio.Should().BeGreaterThan(0.3,
            "floor tiles should have wall neighbors even in 'void' exterior mode");
    }

    [Fact]
    public void Generate_ExteriorWalls_RoomsHaveWallsSurrounding()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;
        config.Dungeon.Exterior = "walls"; // explicit

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - floor tiles should have wall neighbors (rooms have walls)
        var tileLayer = map.GetTileLayer("Tiles")!;
        var floorTilesWithWallNeighbors = 0;
        var totalFloorTiles = 0;

        for (var y = 1; y < map.Height - 1; y++)
        {
            for (var x = 1; x < map.Width - 1; x++)
            {
                if (tileLayer[x, y] == 1) // Floor tile
                {
                    totalFloorTiles++;
                    // Check if any neighbor is a wall
                    var hasWallNeighbor =
                        tileLayer[x - 1, y] == 2 ||
                        tileLayer[x + 1, y] == 2 ||
                        tileLayer[x, y - 1] == 2 ||
                        tileLayer[x, y + 1] == 2;

                    if (hasWallNeighbor)
                    {
                        floorTilesWithWallNeighbors++;
                    }
                }
            }
        }

        // Floor tiles should have wall neighbors (room walls)
        totalFloorTiles.Should().BeGreaterThan(0, "there should be floor tiles");
        var ratio = (double)floorTilesWithWallNeighbors / totalFloorTiles;
        ratio.Should().BeGreaterThan(0.3,
            "floor tiles should have wall neighbors when exterior is 'walls'");
    }

    [Fact]
    public void Generate_DoorsAreOnlyConnectionBetweenRooms()
    {
        // Arrange - with Isaac-style, rooms are directly adjacent via doors
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings
            {
                Name = "door_test",
                Width = 50,
                Height = 40,
                Seed = 12345
            },
            Rooms = new RoomsConfig
            {
                CountString = "4-6",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new RoomTypeConfig { Count = "1", SizeString = "8x8 to 10x10" },
                    ["boss"] = new RoomTypeConfig { Count = "1", SizeString = "12x12 to 14x14" },
                    ["standard"] = new RoomTypeConfig { Count = "rest", SizeString = "6x6 to 10x10" }
                }
            },
            Corridors = new CorridorsConfig { Style = "winding", Width = 1, Doors = 2 },
            Tiles = new TilesConfig { Floor = 1, Wall = 2, Door = 3 }
        };

        // Act
        var map = DungeonGenerator.Generate(config);
        var tileLayer = map.GetTileLayer("Tiles")!;
        var spawnsGroup = map.GetObjectGroup("Spawns")!;

        // Test 1: Flood fill WITH doors should reach all floors
        var playerSpawn = spawnsGroup.Objects.First(o => o.Name == "PlayerSpawn");
        var startX = (int)(playerSpawn.X / 16);
        var startY = (int)(playerSpawn.Y / 16);

        var visitedWithDoors = FloodFill(tileLayer, startX, startY, map.Width, map.Height, includeDoors: true);

        // Count all floor tiles
        var totalFloorTiles = 0;
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 1)
                {
                    totalFloorTiles++;
                }
            }
        }

        // All floor tiles should be reachable when crossing doors
        visitedWithDoors.Count(p => tileLayer[p.x, p.y] == 1).Should().Be(totalFloorTiles,
            "all floor tiles should be reachable when crossing doors");

        // Test 2: Flood fill WITHOUT doors should NOT reach all rooms
        // (only the spawn room's floor tiles should be reachable)
        var visitedWithoutDoors = FloodFill(tileLayer, startX, startY, map.Width, map.Height, includeDoors: false);

        // Without crossing doors, we should only reach tiles in the spawn room
        visitedWithoutDoors.Count.Should().BeLessThan(totalFloorTiles,
            "without crossing doors, we should not reach all floor tiles");
    }

    private static HashSet<(int x, int y)> FloodFill(
        DeSales.DungeonHelper.Tiled.TmxTileLayer layer,
        int startX, int startY,
        int width, int height,
        bool includeDoors)
    {
        var visited = new HashSet<(int x, int y)>();
        var queue = new Queue<(int x, int y)>();
        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            if (visited.Contains((x, y)))
            {
                continue;
            }

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                continue;
            }

            var tile = layer[x, y];
            var isFloor = tile == 1;
            var isDoor = tile == 3;

            if (!isFloor && !(includeDoors && isDoor))
            {
                continue;
            }

            visited.Add((x, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x + 1, y));
            queue.Enqueue((x, y - 1));
            queue.Enqueue((x, y + 1));
        }

        return visited;
    }
}
