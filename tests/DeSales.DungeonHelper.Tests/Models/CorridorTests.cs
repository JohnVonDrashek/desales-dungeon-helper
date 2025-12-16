using DeSales.DungeonHelper.Models;

namespace DeSales.DungeonHelper.Tests.Models;

public class CorridorTests
{
    [Fact]
    public void Constructor_WithValidPoints_CreatesCorridor()
    {
        // Arrange
        var start = new Point(0, 0);
        var end = new Point(10, 10);

        // Act
        var corridor = new Corridor(start, end, width: 1);

        // Assert
        corridor.Start.Should().Be(start);
        corridor.End.Should().Be(end);
        corridor.Width.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidWidth_ThrowsArgumentOutOfRangeException(int width)
    {
        // Act
        var act = () => new Corridor(new Point(0, 0), new Point(10, 10), width);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Path_ReturnsAllPointsInCorridor()
    {
        // Arrange - horizontal corridor
        var corridor = new Corridor(new Point(0, 5), new Point(5, 5), width: 1);

        // Act
        var path = corridor.GetPath().ToList();

        // Assert - should contain points from (0,5) to (5,5)
        path.Should().Contain(new Point(0, 5));
        path.Should().Contain(new Point(5, 5));
        path.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ConnectedRoomIds_InitiallyEmpty()
    {
        // Arrange
        var corridor = new Corridor(new Point(0, 0), new Point(10, 10), width: 1);

        // Assert
        corridor.ConnectedRoomIds.Should().BeEmpty();
    }

    [Fact]
    public void ConnectRooms_AddsRoomIdsToCollection()
    {
        // Arrange
        var corridor = new Corridor(new Point(0, 0), new Point(10, 10), width: 1);

        // Act
        corridor.ConnectRooms(1, 2);

        // Assert
        corridor.ConnectedRoomIds.Should().HaveCount(2);
        corridor.ConnectedRoomIds.Should().Contain(1);
        corridor.ConnectedRoomIds.Should().Contain(2);
    }
}
