using DeSales.DungeonHelper.Models;

namespace DeSales.DungeonHelper.Tests.Models;

public class DungeonTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesDungeon()
    {
        // Arrange & Act
        var dungeon = new Dungeon("test_dungeon", 50, 50, seed: 12345);

        // Assert
        dungeon.Name.Should().Be("test_dungeon");
        dungeon.Width.Should().Be(50);
        dungeon.Height.Should().Be(50);
        dungeon.Seed.Should().Be(12345);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act
        var act = () => new Dungeon(null!, 50, 50, seed: 12345);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(50, 0)]
    [InlineData(-1, 50)]
    public void Constructor_WithInvalidDimensions_ThrowsArgumentOutOfRangeException(int width, int height)
    {
        // Act
        var act = () => new Dungeon("test", width, height, seed: 12345);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TileMap_HasCorrectDimensions()
    {
        // Arrange
        var dungeon = new Dungeon("test", 30, 40, seed: 12345);

        // Assert
        dungeon.TileMap.Width.Should().Be(30);
        dungeon.TileMap.Height.Should().Be(40);
    }

    [Fact]
    public void Rooms_InitiallyEmpty()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);

        // Assert
        dungeon.Rooms.Should().BeEmpty();
    }

    [Fact]
    public void Corridors_InitiallyEmpty()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);

        // Assert
        dungeon.Corridors.Should().BeEmpty();
    }

    [Fact]
    public void AddRoom_AddsRoomToCollection()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var room = new Room(new Rectangle(5, 5, 10, 10), RoomType.Standard);

        // Act
        dungeon.AddRoom(room);

        // Assert
        dungeon.Rooms.Should().ContainSingle()
            .Which.Should().Be(room);
    }

    [Fact]
    public void AddCorridor_AddsCorridorToCollection()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var corridor = new Corridor(new Point(0, 0), new Point(10, 10), width: 1);

        // Act
        dungeon.AddCorridor(corridor);

        // Assert
        dungeon.Corridors.Should().ContainSingle()
            .Which.Should().Be(corridor);
    }

    [Fact]
    public void SpawnRoom_ReturnsFirstSpawnTypeRoom()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var standardRoom = new Room(new Rectangle(0, 0, 5, 5), RoomType.Standard);
        var spawnRoom = new Room(new Rectangle(10, 10, 5, 5), RoomType.Spawn);

        dungeon.AddRoom(standardRoom);
        dungeon.AddRoom(spawnRoom);

        // Act
        var result = dungeon.SpawnRoom;

        // Assert
        result.Should().Be(spawnRoom);
    }

    [Fact]
    public void SpawnRoom_WhenNoSpawnRoom_ReturnsNull()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var standardRoom = new Room(new Rectangle(0, 0, 5, 5), RoomType.Standard);
        dungeon.AddRoom(standardRoom);

        // Act
        var result = dungeon.SpawnRoom;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BossRoom_ReturnsFirstBossTypeRoom()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var standardRoom = new Room(new Rectangle(0, 0, 5, 5), RoomType.Standard);
        var bossRoom = new Room(new Rectangle(10, 10, 15, 15), RoomType.Boss);

        dungeon.AddRoom(standardRoom);
        dungeon.AddRoom(bossRoom);

        // Act
        var result = dungeon.BossRoom;

        // Assert
        result.Should().Be(bossRoom);
    }

    [Fact]
    public void GetRoomAt_ReturnsRoomContainingPoint()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var room = new Room(new Rectangle(10, 10, 5, 5), RoomType.Standard);
        dungeon.AddRoom(room);

        // Act
        var result = dungeon.GetRoomAt(new Point(12, 12));

        // Assert
        result.Should().Be(room);
    }

    [Fact]
    public void GetRoomAt_WhenNoRoomAtPoint_ReturnsNull()
    {
        // Arrange
        var dungeon = new Dungeon("test", 50, 50, seed: 12345);
        var room = new Room(new Rectangle(10, 10, 5, 5), RoomType.Standard);
        dungeon.AddRoom(room);

        // Act
        var result = dungeon.GetRoomAt(new Point(0, 0));

        // Assert
        result.Should().BeNull();
    }
}
