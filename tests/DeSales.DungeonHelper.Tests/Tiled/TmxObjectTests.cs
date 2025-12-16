using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Tests.Tiled;

public class TmxObjectTests
{
    [Fact]
    public void Constructor_WithPointData_CreatesPointObject()
    {
        // Arrange & Act
        var obj = new TmxObject(1, "PlayerSpawn", "spawn", 80, 80);

        // Assert
        obj.Id.Should().Be(1);
        obj.Name.Should().Be("PlayerSpawn");
        obj.Type.Should().Be("spawn");
        obj.X.Should().Be(80);
        obj.Y.Should().Be(80);
        obj.Width.Should().BeNull();
        obj.Height.Should().BeNull();
        obj.IsPoint.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithRectData_CreatesRectangleObject()
    {
        // Arrange & Act
        var obj = new TmxObject(1, "Room_0", "spawn", 64, 64, 96, 96);

        // Assert
        obj.Width.Should().Be(96);
        obj.Height.Should().Be(96);
        obj.IsPoint.Should().BeFalse();
    }

    [Fact]
    public void Position_ReturnsXYAsVector2()
    {
        // Arrange
        var obj = new TmxObject(1, "Test", "test", 100, 200);

        // Act
        var pos = obj.Position;

        // Assert
        pos.X.Should().Be(100);
        pos.Y.Should().Be(200);
    }

    [Fact]
    public void Bounds_ForRectangleObject_ReturnsRectangle()
    {
        // Arrange
        var obj = new TmxObject(1, "Room", "room", 64, 64, 96, 80);

        // Act
        var bounds = obj.Bounds;

        // Assert
        bounds.X.Should().Be(64);
        bounds.Y.Should().Be(64);
        bounds.Width.Should().Be(96);
        bounds.Height.Should().Be(80);
    }

    [Fact]
    public void Bounds_ForPointObject_ReturnsZeroSizeRectangle()
    {
        // Arrange
        var obj = new TmxObject(1, "Spawn", "spawn", 100, 100);

        // Act
        var bounds = obj.Bounds;

        // Assert
        bounds.X.Should().Be(100);
        bounds.Y.Should().Be(100);
        bounds.Width.Should().Be(0);
        bounds.Height.Should().Be(0);
    }

    [Fact]
    public void SetProperty_AddsCustomProperty()
    {
        // Arrange
        var obj = new TmxObject(1, "Room", "room", 0, 0, 100, 100);

        // Act
        obj.SetProperty("roomType", "boss");
        obj.SetProperty("difficulty", "5");

        // Assert
        obj.GetProperty("roomType").Should().Be("boss");
        obj.GetProperty("difficulty").Should().Be("5");
    }

    [Fact]
    public void GetProperty_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var obj = new TmxObject(1, "Room", "room", 0, 0, 100, 100);

        // Act
        var value = obj.GetProperty("nonexistent");

        // Assert
        value.Should().BeNull();
    }
}
