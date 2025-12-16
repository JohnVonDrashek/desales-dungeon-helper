using DeSales.DungeonHelper.Models;

namespace DeSales.DungeonHelper.Tests.Models;

public class RoomTests
{
    [Fact]
    public void Constructor_WithValidBounds_CreatesRoom()
    {
        // Arrange
        var bounds = new Rectangle(5, 5, 10, 8);

        // Act
        var room = new Room(bounds, RoomType.Standard);

        // Assert
        room.Bounds.Should().Be(bounds);
        room.Type.Should().Be(RoomType.Standard);
    }

    [Fact]
    public void Center_ReturnsCorrectCenterPoint()
    {
        // Arrange
        var bounds = new Rectangle(0, 0, 10, 10);
        var room = new Room(bounds, RoomType.Standard);

        // Act
        var center = room.Center;

        // Assert
        center.Should().Be(new Point(5, 5));
    }

    [Fact]
    public void Center_WithOddDimensions_ReturnsTruncatedCenter()
    {
        // Arrange
        var bounds = new Rectangle(0, 0, 7, 9);
        var room = new Room(bounds, RoomType.Standard);

        // Act
        var center = room.Center;

        // Assert
        center.Should().Be(new Point(3, 4));
    }

    [Fact]
    public void Doors_InitiallyEmpty()
    {
        // Arrange
        var room = new Room(new Rectangle(0, 0, 5, 5), RoomType.Standard);

        // Assert
        room.Doors.Should().BeEmpty();
    }

    [Fact]
    public void AddDoor_AddsDoorToRoom()
    {
        // Arrange
        var room = new Room(new Rectangle(0, 0, 5, 5), RoomType.Standard);
        var doorPosition = new Point(2, 0);

        // Act
        room.AddDoor(doorPosition);

        // Assert
        room.Doors.Should().ContainSingle()
            .Which.Should().Be(doorPosition);
    }

    [Fact]
    public void Contains_PointInsideRoom_ReturnsTrue()
    {
        // Arrange
        var room = new Room(new Rectangle(5, 5, 10, 10), RoomType.Standard);

        // Assert
        room.Contains(new Point(10, 10)).Should().BeTrue();
        room.Contains(new Point(5, 5)).Should().BeTrue();
        room.Contains(new Point(14, 14)).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointOutsideRoom_ReturnsFalse()
    {
        // Arrange
        var room = new Room(new Rectangle(5, 5, 10, 10), RoomType.Standard);

        // Assert
        room.Contains(new Point(0, 0)).Should().BeFalse();
        room.Contains(new Point(15, 15)).Should().BeFalse();
    }

    [Theory]
    [InlineData(RoomType.Spawn)]
    [InlineData(RoomType.Boss)]
    [InlineData(RoomType.Treasure)]
    [InlineData(RoomType.Standard)]
    public void Constructor_WithAllRoomTypes_CreatesCorrectType(RoomType roomType)
    {
        // Arrange & Act
        var room = new Room(new Rectangle(0, 0, 5, 5), roomType);

        // Assert
        room.Type.Should().Be(roomType);
    }
}
