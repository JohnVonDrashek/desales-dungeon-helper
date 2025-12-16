using DeSales.DungeonHelper.Models;

namespace DeSales.DungeonHelper.Tests.Models;

public class TileMapTests
{
    [Fact]
    public void Constructor_WithValidDimensions_CreatesTileMap()
    {
        // Arrange & Act
        var tileMap = new TileMap(10, 15);

        // Assert
        tileMap.Width.Should().Be(10);
        tileMap.Height.Should().Be(15);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    public void Constructor_WithInvalidDimensions_ThrowsArgumentOutOfRangeException(int width, int height)
    {
        // Act
        var act = () => new TileMap(width, height);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Indexer_WithValidCoordinates_GetsAndSetsTile()
    {
        // Arrange
        var tileMap = new TileMap(10, 10);

        // Act
        tileMap[5, 5] = TileType.Wall;

        // Assert
        tileMap[5, 5].Should().Be(TileType.Wall);
    }

    [Fact]
    public void Indexer_DefaultValue_IsEmpty()
    {
        // Arrange
        var tileMap = new TileMap(10, 10);

        // Assert
        tileMap[0, 0].Should().Be(TileType.Empty);
        tileMap[9, 9].Should().Be(TileType.Empty);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public void Indexer_WithOutOfBoundsCoordinates_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        // Arrange
        var tileMap = new TileMap(10, 10);

        // Act
        var act = () => tileMap[x, y];

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Fill_FillsEntireMapWithTileType()
    {
        // Arrange
        var tileMap = new TileMap(5, 5);

        // Act
        tileMap.Fill(TileType.Floor);

        // Assert
        for (var x = 0; x < 5; x++)
        {
            for (var y = 0; y < 5; y++)
            {
                tileMap[x, y].Should().Be(TileType.Floor);
            }
        }
    }

    [Fact]
    public void FillRect_FillsRectangularAreaWithTileType()
    {
        // Arrange
        var tileMap = new TileMap(10, 10);
        var rect = new Rectangle(2, 2, 3, 4);

        // Act
        tileMap.FillRect(rect, TileType.Wall);

        // Assert
        // Inside the rectangle should be Wall
        tileMap[2, 2].Should().Be(TileType.Wall);
        tileMap[4, 5].Should().Be(TileType.Wall);

        // Outside should still be Empty
        tileMap[0, 0].Should().Be(TileType.Empty);
        tileMap[5, 2].Should().Be(TileType.Empty);
    }

    [Fact]
    public void IsInBounds_WithValidCoordinates_ReturnsTrue()
    {
        // Arrange
        var tileMap = new TileMap(10, 10);

        // Assert
        tileMap.IsInBounds(0, 0).Should().BeTrue();
        tileMap.IsInBounds(9, 9).Should().BeTrue();
        tileMap.IsInBounds(5, 5).Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public void IsInBounds_WithInvalidCoordinates_ReturnsFalse(int x, int y)
    {
        // Arrange
        var tileMap = new TileMap(10, 10);

        // Assert
        tileMap.IsInBounds(x, y).Should().BeFalse();
    }
}
