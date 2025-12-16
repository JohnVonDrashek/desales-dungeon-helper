using DeSales.DungeonHelper.Tiled;

namespace DeSales.DungeonHelper.Tests.Tiled;

public class TmxTileLayerTests
{
    [Fact]
    public void Constructor_WithValidDimensions_CreatesLayer()
    {
        // Arrange & Act
        var layer = new TmxTileLayer("Floor", 10, 15);

        // Assert
        layer.Name.Should().Be("Floor");
        layer.Width.Should().Be(10);
        layer.Height.Should().Be(15);
    }

    [Fact]
    public void Indexer_WithValidCoordinates_GetsAndSetsTile()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 10, 10);

        // Act
        layer[5, 5] = 1;

        // Assert
        layer[5, 5].Should().Be(1);
    }

    [Fact]
    public void Indexer_DefaultValue_IsZero()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 10, 10);

        // Assert
        layer[0, 0].Should().Be(0);
        layer[9, 9].Should().Be(0);
    }

    [Fact]
    public void Fill_FillsEntireLayerWithValue()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 5, 5);

        // Act
        layer.Fill(1);

        // Assert
        for (var x = 0; x < 5; x++)
        {
            for (var y = 0; y < 5; y++)
            {
                layer[x, y].Should().Be(1);
            }
        }
    }

    [Fact]
    public void FillRect_FillsRectangularArea()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 10, 10);

        // Act
        layer.FillRect(2, 2, 3, 4, tileId: 1);

        // Assert
        layer[2, 2].Should().Be(1);
        layer[4, 5].Should().Be(1);
        layer[0, 0].Should().Be(0);
        layer[5, 2].Should().Be(0);
    }

    [Fact]
    public void ToCsv_ReturnsCommaSeparatedValues()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 3, 2);
        layer[0, 0] = 1;
        layer[1, 0] = 2;
        layer[2, 0] = 3;
        layer[0, 1] = 4;
        layer[1, 1] = 5;
        layer[2, 1] = 6;

        // Act
        var csv = layer.ToCsv();

        // Assert - TMX uses row-major order (y then x)
        csv.Should().Contain("1,2,3");
        csv.Should().Contain("4,5,6");
    }

    [Fact]
    public void FromCsv_ParsesCommaSeparatedValues()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 3, 2);
        var csv = "1,2,3,\n4,5,6";

        // Act
        layer.FromCsv(csv);

        // Assert
        layer[0, 0].Should().Be(1);
        layer[1, 0].Should().Be(2);
        layer[2, 0].Should().Be(3);
        layer[0, 1].Should().Be(4);
        layer[1, 1].Should().Be(5);
        layer[2, 1].Should().Be(6);
    }

    [Fact]
    public void IsInBounds_WithValidCoordinates_ReturnsTrue()
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 10, 10);

        // Assert
        layer.IsInBounds(0, 0).Should().BeTrue();
        layer.IsInBounds(9, 9).Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public void IsInBounds_WithInvalidCoordinates_ReturnsFalse(int x, int y)
    {
        // Arrange
        var layer = new TmxTileLayer("Floor", 10, 10);

        // Assert
        layer.IsInBounds(x, y).Should().BeFalse();
    }
}
