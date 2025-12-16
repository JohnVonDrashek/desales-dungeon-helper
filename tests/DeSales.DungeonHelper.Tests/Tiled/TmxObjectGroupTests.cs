using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Tests.Tiled;

public class TmxObjectGroupTests
{
    [Fact]
    public void Constructor_WithName_CreatesGroup()
    {
        // Arrange & Act
        var group = new TmxObjectGroup("Spawns");

        // Assert
        group.Name.Should().Be("Spawns");
        group.Objects.Should().BeEmpty();
    }

    [Fact]
    public void AddObject_AddsPointObject()
    {
        // Arrange
        var group = new TmxObjectGroup("Spawns");

        // Act
        var obj = group.AddObject("PlayerSpawn", "spawn", x: 80, y: 80);

        // Assert
        obj.Should().NotBeNull();
        obj.Name.Should().Be("PlayerSpawn");
        obj.Type.Should().Be("spawn");
        obj.X.Should().Be(80);
        obj.Y.Should().Be(80);
        group.Objects.Should().ContainSingle();
    }

    [Fact]
    public void AddObject_WithDimensions_AddsRectangleObject()
    {
        // Arrange
        var group = new TmxObjectGroup("Rooms");

        // Act
        var obj = group.AddObject("Room_0", "spawn", x: 64, y: 64, width: 96, height: 96);

        // Assert
        obj.Width.Should().Be(96);
        obj.Height.Should().Be(96);
    }

    [Fact]
    public void GetObject_ReturnsExistingObject()
    {
        // Arrange
        var group = new TmxObjectGroup("Spawns");
        group.AddObject("PlayerSpawn", "spawn", 80, 80);

        // Act
        var obj = group.GetObject("PlayerSpawn");

        // Assert
        obj.Should().NotBeNull();
        obj!.Name.Should().Be("PlayerSpawn");
    }

    [Fact]
    public void GetObject_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var group = new TmxObjectGroup("Spawns");

        // Act
        var obj = group.GetObject("NonExistent");

        // Assert
        obj.Should().BeNull();
    }

    [Fact]
    public void GetObjectsByType_ReturnsMatchingObjects()
    {
        // Arrange
        var group = new TmxObjectGroup("Rooms");
        group.AddObject("Room_0", "spawn", 64, 64, 96, 96);
        group.AddObject("Room_1", "standard", 200, 64, 80, 80);
        group.AddObject("Room_2", "standard", 64, 200, 80, 80);
        group.AddObject("Room_3", "boss", 300, 300, 128, 128);

        // Act
        var standardRooms = group.GetObjectsByType("standard").ToList();

        // Assert
        standardRooms.Should().HaveCount(2);
        standardRooms.Should().OnlyContain(r => r.Type == "standard");
    }

    [Fact]
    public void Objects_AssignsIncrementingIds()
    {
        // Arrange
        var group = new TmxObjectGroup("Spawns");

        // Act
        var obj1 = group.AddObject("First", "spawn", 0, 0);
        var obj2 = group.AddObject("Second", "spawn", 10, 10);
        var obj3 = group.AddObject("Third", "spawn", 20, 20);

        // Assert
        obj1.Id.Should().Be(1);
        obj2.Id.Should().Be(2);
        obj3.Id.Should().Be(3);
    }
}
