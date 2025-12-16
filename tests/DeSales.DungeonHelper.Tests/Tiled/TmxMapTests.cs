using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Generation;
using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Tests.Tiled;

public class TmxMapTests
{
    [Fact]
    public void Constructor_WithValidDimensions_CreatesMap()
    {
        // Arrange & Act
        var map = new TmxMap(50, 50, tileWidth: 16, tileHeight: 16);

        // Assert
        map.Width.Should().Be(50);
        map.Height.Should().Be(50);
        map.TileWidth.Should().Be(16);
        map.TileHeight.Should().Be(16);
    }

    [Fact]
    public void AddTileLayer_AddsLayerToMap()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);

        // Act
        var layer = map.AddTileLayer("Floor");

        // Assert
        layer.Should().NotBeNull();
        layer.Name.Should().Be("Floor");
        map.TileLayers.Should().ContainSingle();
    }

    [Fact]
    public void AddObjectGroup_AddsGroupToMap()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);

        // Act
        var group = map.AddObjectGroup("Spawns");

        // Assert
        group.Should().NotBeNull();
        group.Name.Should().Be("Spawns");
        map.ObjectGroups.Should().ContainSingle();
    }

    [Fact]
    public void GetTileLayer_ReturnsExistingLayer()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        map.AddTileLayer("Floor");

        // Act
        var layer = map.GetTileLayer("Floor");

        // Assert
        layer.Should().NotBeNull();
        layer!.Name.Should().Be("Floor");
    }

    [Fact]
    public void GetTileLayer_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);

        // Act
        var layer = map.GetTileLayer("NonExistent");

        // Assert
        layer.Should().BeNull();
    }

    [Fact]
    public void GetObjectGroup_ReturnsExistingGroup()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        map.AddObjectGroup("Spawns");

        // Act
        var group = map.GetObjectGroup("Spawns");

        // Assert
        group.Should().NotBeNull();
        group!.Name.Should().Be("Spawns");
    }

    [Fact]
    public void ToXml_GeneratesValidTmxStructure()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        map.AddTileLayer("Floor");
        map.AddObjectGroup("Spawns");

        // Act
        var xml = map.ToXml();

        // Assert
        xml.Should().Contain("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.Should().Contain("<map");
        xml.Should().Contain("width=\"10\"");
        xml.Should().Contain("height=\"10\"");
        xml.Should().Contain("tilewidth=\"16\"");
        xml.Should().Contain("tileheight=\"16\"");
        xml.Should().Contain("<layer name=\"Floor\"");
        xml.Should().Contain("<objectgroup name=\"Spawns\"");
    }

    [Fact]
    public void Save_WritesFileToPath()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        map.AddTileLayer("Floor");
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.tmx");

        try
        {
            // Act
            map.Save(tempPath);

            // Assert
            File.Exists(tempPath).Should().BeTrue();
            var content = File.ReadAllText(tempPath);
            content.Should().Contain("<map");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void Load_ReadsExistingTmxFile()
    {
        // Arrange
        var originalMap = new TmxMap(20, 15, 16, 16);
        var floor = originalMap.AddTileLayer("Floor");
        floor[5, 5] = 1;
        var spawns = originalMap.AddObjectGroup("Spawns");
        spawns.AddObject("PlayerSpawn", "spawn", 80, 80);

        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.tmx");

        try
        {
            originalMap.Save(tempPath);

            // Act
            var loadedMap = TmxMap.Load(tempPath);

            // Assert
            loadedMap.Width.Should().Be(20);
            loadedMap.Height.Should().Be(15);
            loadedMap.TileWidth.Should().Be(16);
            loadedMap.TileHeight.Should().Be(16);

            var loadedFloor = loadedMap.GetTileLayer("Floor");
            loadedFloor.Should().NotBeNull();
            loadedFloor![5, 5].Should().Be(1);

            var loadedSpawns = loadedMap.GetObjectGroup("Spawns");
            loadedSpawns.Should().NotBeNull();
            loadedSpawns!.Objects.Should().ContainSingle();
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    #region Runtime Helper Tests

    [Fact]
    public void GetCollisionRectangles_ReturnsWallTileRectangles()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        var layer = map.AddTileLayer("Tiles");
        layer[0, 0] = 2; // Wall
        layer[1, 0] = 2; // Wall
        layer[5, 5] = 1; // Floor (not collision)

        // Act
        var collisions = map.GetCollisionRectangles("Tiles", tileId: 2);

        // Assert
        collisions.Should().HaveCount(2);
        collisions[0].X.Should().Be(0);
        collisions[0].Y.Should().Be(0);
        collisions[0].Width.Should().Be(16);
        collisions[0].Height.Should().Be(16);
        collisions[1].X.Should().Be(16);
        collisions[1].Y.Should().Be(0);
    }

    [Fact]
    public void GetCollisionRectangles_WithNonexistentLayer_ReturnsEmpty()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);

        // Act
        var collisions = map.GetCollisionRectangles("NonExistent", 2);

        // Assert
        collisions.Should().BeEmpty();
    }

    [Fact]
    public void GetSpawnPoint_ReturnsCorrectPosition()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        var spawns = map.AddObjectGroup("Spawns");
        spawns.AddObject("PlayerSpawn", "spawn", 80, 96);

        // Act
        var position = map.GetSpawnPoint("PlayerSpawn");

        // Assert
        position.Should().NotBeNull();
        position!.Value.X.Should().Be(80);
        position.Value.Y.Should().Be(96);
    }

    [Fact]
    public void GetSpawnPoint_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        map.AddObjectGroup("Spawns");

        // Act
        var position = map.GetSpawnPoint("NonExistent");

        // Assert
        position.Should().BeNull();
    }

    [Fact]
    public void GetSpawnPointsByType_ReturnsAllMatchingSpawns()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        var spawns = map.AddObjectGroup("Spawns");
        spawns.AddObject("TreasureSpawn_0", "treasure", 32, 48);
        spawns.AddObject("TreasureSpawn_1", "treasure", 80, 96);
        spawns.AddObject("PlayerSpawn", "spawn", 16, 16);

        // Act
        var treasureSpawns = map.GetSpawnPointsByType("treasure");

        // Assert
        treasureSpawns.Should().HaveCount(2);
        treasureSpawns[0].X.Should().Be(32);
        treasureSpawns[1].X.Should().Be(80);
    }

    [Fact]
    public void GetRoomBounds_ReturnsCorrectRectangle()
    {
        // Arrange
        var map = new TmxMap(20, 20, 16, 16);
        var rooms = map.AddObjectGroup("Rooms");
        rooms.AddObject("Room_spawn_0", "spawn", 32, 48, 128, 96);

        // Act
        var bounds = map.GetRoomBounds("Room_spawn_0");

        // Assert
        bounds.Should().NotBeNull();
        bounds!.Value.X.Should().Be(32);
        bounds.Value.Y.Should().Be(48);
        bounds.Value.Width.Should().Be(128);
        bounds.Value.Height.Should().Be(96);
    }

    [Fact]
    public void GetRoomBounds_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var map = new TmxMap(10, 10, 16, 16);
        map.AddObjectGroup("Rooms");

        // Act
        var bounds = map.GetRoomBounds("NonExistent");

        // Assert
        bounds.Should().BeNull();
    }

    [Fact]
    public void GetRoomsByType_ReturnsAllMatchingRooms()
    {
        // Arrange
        var map = new TmxMap(30, 30, 16, 16);
        var rooms = map.AddObjectGroup("Rooms");
        rooms.AddObject("Room_standard_0", "standard", 0, 0, 80, 80);
        rooms.AddObject("Room_standard_1", "standard", 160, 0, 96, 64);
        rooms.AddObject("Room_boss_0", "boss", 160, 160, 128, 128);

        // Act
        var standardRooms = map.GetRoomsByType("standard");

        // Assert
        standardRooms.Should().HaveCount(2);
        standardRooms[0].Width.Should().Be(80);
        standardRooms[1].Width.Should().Be(96);
    }

    [Fact]
    public void GetAllRoomBounds_ReturnsAllRooms()
    {
        // Arrange
        var map = new TmxMap(30, 30, 16, 16);
        var rooms = map.AddObjectGroup("Rooms");
        rooms.AddObject("Room_spawn_0", "spawn", 0, 0, 80, 80);
        rooms.AddObject("Room_boss_0", "boss", 160, 160, 128, 128);

        // Act
        var allRooms = map.GetAllRoomBounds();

        // Assert
        allRooms.Should().HaveCount(2);
    }

    [Fact]
    public void RuntimeHelpers_WorkWithGeneratedDungeon()
    {
        // Arrange - generate a real dungeon
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50, Seed = 42 },
            Rooms = new RoomsConfig
            {
                CountString = "4-6",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "1", SizeString = "6x6 to 8x8" },
                    ["boss"] = new() { Count = "1", SizeString = "8x8 to 10x10" },
                    ["standard"] = new() { Count = "rest", SizeString = "5x5 to 8x8" }
                }
            },
            Tiles = new TilesConfig { Floor = 1, Wall = 2, Door = 3 }
        };

        var map = DungeonGenerator.Generate(config);

        // Act & Assert - test all runtime helpers
        var playerSpawn = map.GetSpawnPoint("PlayerSpawn");
        playerSpawn.Should().NotBeNull("generated dungeon should have PlayerSpawn");

        var bossSpawn = map.GetSpawnPoint("BossSpawn");
        bossSpawn.Should().NotBeNull("generated dungeon should have BossSpawn");

        var wallCollisions = map.GetCollisionRectangles("Tiles", 2);
        wallCollisions.Should().NotBeEmpty("generated dungeon should have wall tiles");

        var allRooms = map.GetAllRoomBounds();
        allRooms.Should().HaveCountGreaterOrEqualTo(4, "generated dungeon should have rooms");

        var bossRooms = map.GetRoomsByType("boss");
        bossRooms.Should().ContainSingle("generated dungeon should have one boss room");
    }

    #endregion
}
