using DeSales.DungeonHelper.Configuration;

namespace DeSales.DungeonHelper.Tests.Configuration;

public class DungeonConfigTests
{
    [Fact]
    public void ParseFromYaml_WithValidConfig_ReturnsConfig()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test_dungeon
              seed: 12345
              width: 50
              height: 50
            rooms:
              count: 8-12
            corridors:
              style: winding
              width: 1
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Dungeon.Name.Should().Be("test_dungeon");
        config.Dungeon.Seed.Should().Be(12345);
        config.Dungeon.Width.Should().Be(50);
        config.Dungeon.Height.Should().Be(50);
    }

    [Fact]
    public void ParseFromYaml_WithRoomCount_ParsesRange()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 8-12
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Rooms.Count.Min.Should().Be(8);
        config.Rooms.Count.Max.Should().Be(12);
    }

    [Fact]
    public void ParseFromYaml_WithSingleRoomCount_ParsesAsSameMinMax()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 10
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Rooms.Count.Min.Should().Be(10);
        config.Rooms.Count.Max.Should().Be(10);
    }

    [Fact]
    public void ParseFromYaml_WithRoomTypes_ParsesAllTypes()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 10
              types:
                spawn:
                  count: 1
                  size: 7x7 to 10x10
                boss:
                  count: 1
                  size: 15x15 to 20x20
                  placement: far_from_spawn
                standard:
                  count: rest
                  size: 5x5 to 12x12
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Rooms.Types.Should().ContainKey("spawn");
        config.Rooms.Types.Should().ContainKey("boss");
        config.Rooms.Types.Should().ContainKey("standard");

        config.Rooms.Types["spawn"].Count.Should().Be("1");
        config.Rooms.Types["boss"].Placement.Should().Be("far_from_spawn");
        config.Rooms.Types["standard"].Count.Should().Be("rest");
    }

    [Fact]
    public void ParseFromYaml_WithSizeRange_ParsesMinAndMax()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 10
              types:
                spawn:
                  count: 1
                  size: 5x5 to 10x8
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        var spawnType = config.Rooms.Types["spawn"];
        spawnType.Size.MinWidth.Should().Be(5);
        spawnType.Size.MinHeight.Should().Be(5);
        spawnType.Size.MaxWidth.Should().Be(10);
        spawnType.Size.MaxHeight.Should().Be(8);
    }

    [Fact]
    public void ParseFromYaml_WithCorridorConfig_ParsesStyle()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 10
            corridors:
              style: winding
              width: 2
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Corridors.Style.Should().Be("winding");
        config.Corridors.Width.Should().Be(2);
    }

    [Fact]
    public void ParseFromYaml_WithTileMapping_ParsesCustomTiles()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 10
            tiles:
              floor: 1
              wall: 2
              door: 3
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Tiles.Should().NotBeNull();
        config.Tiles!.Floor.Should().Be(1);
        config.Tiles.Wall.Should().Be(2);
        config.Tiles.Door.Should().Be(3);
    }

    [Fact]
    public void ParseFromYaml_WithNoSeed_SeedIsNull()
    {
        // Arrange
        var yaml = """
            dungeon:
              name: test
              width: 50
              height: 50
            rooms:
              count: 10
            """;

        // Act
        var config = DungeonConfig.ParseFromYaml(yaml);

        // Assert
        config.Dungeon.Seed.Should().BeNull();
    }

    #region Validation Tests

    [Fact]
    public void Validate_WithValidConfig_ReturnsNoErrors()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50 },
            Rooms = new RoomsConfig
            {
                CountString = "4-6",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "1", SizeString = "5x5 to 8x8" },
                    ["standard"] = new() { Count = "rest", SizeString = "5x5 to 10x10" }
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTooSmallWidth_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 5, Height = 50 },
            Rooms = new RoomsConfig
            {
                CountString = "4",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "1", SizeString = "3x3" }
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("width") && e.Contains("at least 10"));
    }

    [Fact]
    public void Validate_WithTooSmallHeight_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 8 },
            Rooms = new RoomsConfig
            {
                CountString = "4",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "1", SizeString = "3x3" }
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("height") && e.Contains("at least 10"));
    }

    [Fact]
    public void Validate_WithNoRoomTypes_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50 },
            Rooms = new RoomsConfig { CountString = "4" }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("room type") && e.Contains("defined"));
    }

    [Fact]
    public void Validate_WithRoomTooSmall_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50 },
            Rooms = new RoomsConfig
            {
                CountString = "4",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["tiny"] = new() { Count = "1", SizeString = "2x2" }
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("tiny") && e.Contains("at least 3"));
    }

    [Fact]
    public void Validate_WithRoomTooBig_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 20, Height = 20 },
            Rooms = new RoomsConfig
            {
                CountString = "4",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["huge"] = new() { Count = "1", SizeString = "25x25" }
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("huge") && e.Contains("exceeds"));
    }

    [Fact]
    public void Validate_WithFixedRoomsExceedingMax_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50 },
            Rooms = new RoomsConfig
            {
                CountString = "3", // Max 3 rooms
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "2", SizeString = "5x5" },
                    ["boss"] = new() { Count = "2", SizeString = "5x5" }
                    // Total fixed = 4, but max is 3
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("fixed room counts") && e.Contains("exceeds"));
    }

    [Fact]
    public void Validate_WithInvalidRoomCountRange_ReturnsError()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50 },
            Rooms = new RoomsConfig
            {
                CountString = "10-5", // Max < Min
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "1", SizeString = "5x5" }
                }
            }
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("Maximum room count") && e.Contains("cannot be less"));
    }

    [Fact]
    public void ThrowIfInvalid_WithInvalidConfig_ThrowsException()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 5, Height = 5 },
            Rooms = new RoomsConfig { CountString = "4" }
        };

        // Act & Assert
        var action = () => config.ThrowIfInvalid();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid dungeon configuration*");
    }

    [Fact]
    public void ThrowIfInvalid_WithValidConfig_DoesNotThrow()
    {
        // Arrange
        var config = new DungeonConfig
        {
            Dungeon = new DungeonSettings { Width = 50, Height = 50 },
            Rooms = new RoomsConfig
            {
                CountString = "4",
                Types = new Dictionary<string, RoomTypeConfig>
                {
                    ["spawn"] = new() { Count = "1", SizeString = "5x5" },
                    ["standard"] = new() { Count = "rest", SizeString = "5x5" }
                }
            }
        };

        // Act & Assert
        var action = () => config.ThrowIfInvalid();
        action.Should().NotThrow();
    }

    #endregion
}
