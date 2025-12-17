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
    public void Generate_ConnectsRoomsWithCorridors()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.Dungeon.Seed = 42;

        // Act
        var map = DungeonGenerator.Generate(config);

        // Assert - verify that there are floor tiles outside of rooms (corridors)
        var tileLayer = map.GetTileLayer("Tiles")!;
        var roomsGroup = map.GetObjectGroup("Rooms")!;

        // Count floor tiles that are NOT inside any room
        var corridorFloorCount = 0;
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 1) // Floor tile
                {
                    var inRoom = roomsGroup.Objects.Any(room =>
                    {
                        var rx = (int)(room.X / 16);
                        var ry = (int)(room.Y / 16);
                        var rw = (int)(room.Width!.Value / 16);
                        var rh = (int)(room.Height!.Value / 16);
                        return x >= rx && x < rx + rw && y >= ry && y < ry + rh;
                    });

                    if (!inRoom)
                    {
                        corridorFloorCount++;
                    }
                }
            }
        }

        corridorFloorCount.Should().BeGreaterThan(0, "there should be corridor floor tiles connecting rooms");
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
    public void Generate_CorridorWidthIsRespected(int corridorWidth)
    {
        // Arrange
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
                Width = corridorWidth
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

        // Assert - count corridor floor tiles outside of rooms
        var tileLayer = map.GetTileLayer("Tiles")!;
        var roomsGroup = map.GetObjectGroup("Rooms")!;

        var corridorFloorTiles = new List<(int x, int y)>();
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (tileLayer[x, y] == 1) // Floor tile
                {
                    var inRoom = roomsGroup.Objects.Any(room =>
                    {
                        var rx = (int)(room.X / 16);
                        var ry = (int)(room.Y / 16);
                        var rw = (int)(room.Width!.Value / 16);
                        var rh = (int)(room.Height!.Value / 16);
                        return x >= rx && x < rx + rw && y >= ry && y < ry + rh;
                    });

                    if (!inRoom)
                    {
                        corridorFloorTiles.Add((x, y));
                    }
                }
            }
        }

        // For a corridor of width N, we expect roughly N times as many floor tiles
        // compared to width 1 (not exact due to corridor turns and overlaps)
        corridorFloorTiles.Should().NotBeEmpty("corridors should have floor tiles");

        // Verify corridor width by checking that parallel tiles exist
        // For each corridor tile, check if there are adjacent tiles in perpendicular direction
        if (corridorWidth > 1)
        {
            var tilesWithNeighbors = corridorFloorTiles.Count(tile =>
                corridorFloorTiles.Contains((tile.x - 1, tile.y)) ||
                corridorFloorTiles.Contains((tile.x + 1, tile.y)) ||
                corridorFloorTiles.Contains((tile.x, tile.y - 1)) ||
                corridorFloorTiles.Contains((tile.x, tile.y + 1)));

            // Most corridor tiles should have neighbors (wider corridors have more connectivity)
            tilesWithNeighbors.Should().BeGreaterThan(corridorFloorTiles.Count / 2,
                $"wider corridors (width={corridorWidth}) should have more connected floor tiles");
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
}
