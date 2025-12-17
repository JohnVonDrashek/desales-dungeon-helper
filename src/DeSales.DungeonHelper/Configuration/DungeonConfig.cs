using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DeSales.DungeonHelper.Configuration;

/// <summary>
/// Root configuration for dungeon generation.
/// </summary>
public class DungeonConfig
{
    /// <summary>
    /// Gets or sets the dungeon settings.
    /// </summary>
    public DungeonSettings Dungeon { get; set; } = new();

    /// <summary>
    /// Gets or sets the room configuration.
    /// </summary>
    public RoomsConfig Rooms { get; set; } = new();

    /// <summary>
    /// Gets or sets the corridor configuration.
    /// </summary>
    public CorridorsConfig Corridors { get; set; } = new();

    /// <summary>
    /// Gets or sets the tile ID mapping.
    /// </summary>
    public TilesConfig? Tiles { get; set; }

    /// <summary>
    /// Parses a DungeonConfig from a YAML string.
    /// </summary>
    /// <param name="yaml">The YAML configuration string.</param>
    /// <returns>The parsed configuration.</returns>
    public static DungeonConfig ParseFromYaml(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<DungeonConfig>(yaml);
    }

    /// <summary>
    /// Loads a DungeonConfig from a YAML file.
    /// </summary>
    /// <param name="filePath">The path to the YAML file.</param>
    /// <returns>The parsed configuration.</returns>
    public static DungeonConfig LoadFromFile(string filePath)
    {
        var yaml = File.ReadAllText(filePath);
        return ParseFromYaml(yaml);
    }

    /// <summary>
    /// Validates the configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages. Empty if valid.</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Validate dungeon dimensions
        if (Dungeon.Width < 10)
        {
            errors.Add($"Dungeon width ({Dungeon.Width}) must be at least 10 tiles.");
        }

        if (Dungeon.Height < 10)
        {
            errors.Add($"Dungeon height ({Dungeon.Height}) must be at least 10 tiles.");
        }

        // Validate room count
        if (Rooms.Count.Min < 1)
        {
            errors.Add($"Minimum room count ({Rooms.Count.Min}) must be at least 1.");
        }

        if (Rooms.Count.Max < Rooms.Count.Min)
        {
            errors.Add($"Maximum room count ({Rooms.Count.Max}) cannot be less than minimum ({Rooms.Count.Min}).");
        }

        // Validate room types
        if (Rooms.Types.Count == 0)
        {
            errors.Add("At least one room type must be defined.");
        }

        var hasRestType = false;
        var totalFixedRooms = 0;

        foreach (var (typeName, typeConfig) in Rooms.Types)
        {
            // Check for "rest" type
            if (typeConfig.Count.Equals("rest", StringComparison.OrdinalIgnoreCase))
            {
                hasRestType = true;
            }
            else
            {
                var range = IntRange.Parse(typeConfig.Count);
                totalFixedRooms += range.Min;
            }

            // Validate room sizes
            if (typeConfig.Size.MinWidth < 3)
            {
                errors.Add($"Room type '{typeName}' minimum width ({typeConfig.Size.MinWidth}) must be at least 3 (walls + floor).");
            }

            if (typeConfig.Size.MinHeight < 3)
            {
                errors.Add($"Room type '{typeName}' minimum height ({typeConfig.Size.MinHeight}) must be at least 3 (walls + floor).");
            }

            if (typeConfig.Size.MaxWidth > Dungeon.Width - 2)
            {
                errors.Add($"Room type '{typeName}' maximum width ({typeConfig.Size.MaxWidth}) exceeds dungeon width ({Dungeon.Width}).");
            }

            if (typeConfig.Size.MaxHeight > Dungeon.Height - 2)
            {
                errors.Add($"Room type '{typeName}' maximum height ({typeConfig.Size.MaxHeight}) exceeds dungeon height ({Dungeon.Height}).");
            }

            if (typeConfig.Size.MaxWidth < typeConfig.Size.MinWidth)
            {
                errors.Add($"Room type '{typeName}' max width ({typeConfig.Size.MaxWidth}) cannot be less than min width ({typeConfig.Size.MinWidth}).");
            }

            if (typeConfig.Size.MaxHeight < typeConfig.Size.MinHeight)
            {
                errors.Add($"Room type '{typeName}' max height ({typeConfig.Size.MaxHeight}) cannot be less than min height ({typeConfig.Size.MinHeight}).");
            }
        }

        // Check if fixed room counts exceed total
        if (totalFixedRooms > Rooms.Count.Max)
        {
            errors.Add($"Sum of fixed room counts ({totalFixedRooms}) exceeds maximum room count ({Rooms.Count.Max}).");
        }

        // Warn if no "rest" type and fixed rooms don't match count
        if (!hasRestType && totalFixedRooms < Rooms.Count.Min)
        {
            errors.Add($"No 'rest' room type defined and fixed room counts ({totalFixedRooms}) are less than minimum room count ({Rooms.Count.Min}).");
        }

        return errors;
    }

    /// <summary>
    /// Validates the configuration and throws if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public void ThrowIfInvalid()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Invalid dungeon configuration:\n- {string.Join("\n- ", errors)}");
        }
    }
}

/// <summary>
/// Basic dungeon settings.
/// </summary>
public class DungeonSettings
{
    /// <summary>
    /// Gets or sets the dungeon name.
    /// </summary>
    public string Name { get; set; } = "dungeon";

    /// <summary>
    /// Gets or sets the random seed for generation. Null for random seed.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Gets or sets the dungeon width in tiles.
    /// </summary>
    public int Width { get; set; } = 50;

    /// <summary>
    /// Gets or sets the dungeon height in tiles.
    /// </summary>
    public int Height { get; set; } = 50;

    /// <summary>
    /// Gets or sets the exterior style. "walls" (default) fills the entire map with walls,
    /// creating a classic dungeon look. "void" leaves empty space outside rooms/corridors.
    /// </summary>
    public string Exterior { get; set; } = "walls";
}

/// <summary>
/// Room configuration.
/// </summary>
public class RoomsConfig
{
    private string _countString = "8-12";

    /// <summary>
    /// Gets or sets the room count as a string (e.g., "8-12" or "10").
    /// </summary>
    [YamlMember(Alias = "count")]
    public string CountString
    {
        get => _countString;
        set
        {
            _countString = value;
            Count = IntRange.Parse(value);
        }
    }

    /// <summary>
    /// Gets the parsed room count range.
    /// </summary>
    [YamlIgnore]
    public IntRange Count { get; private set; } = new(8, 12);

    /// <summary>
    /// Gets or sets the room type configurations.
    /// </summary>
    public Dictionary<string, RoomTypeConfig> Types { get; set; } = [];
}

/// <summary>
/// Configuration for a specific room type.
/// </summary>
public class RoomTypeConfig
{
    private string _sizeString = "5x5 to 12x12";

    /// <summary>
    /// Gets or sets the room count (number or "rest").
    /// </summary>
    public string Count { get; set; } = "1";

    /// <summary>
    /// Gets or sets the size range as a string (e.g., "5x5 to 12x12").
    /// </summary>
    [YamlMember(Alias = "size")]
    public string SizeString
    {
        get => _sizeString;
        set
        {
            _sizeString = value;
            Size = SizeRange.Parse(value);
        }
    }

    /// <summary>
    /// Gets the parsed size range.
    /// </summary>
    [YamlIgnore]
    public SizeRange Size { get; private set; } = new();

    /// <summary>
    /// Gets or sets the placement constraint (e.g., "far_from_spawn", "near_center").
    /// </summary>
    public string? Placement { get; set; }
}

/// <summary>
/// Corridor configuration.
/// </summary>
public class CorridorsConfig
{
    /// <summary>
    /// Gets or sets the corridor style (winding, straight, organic).
    /// </summary>
    public string Style { get; set; } = "winding";

    /// <summary>
    /// Gets or sets the corridor width in tiles.
    /// </summary>
    public int Width { get; set; } = 1;
}

/// <summary>
/// Custom tile ID mapping.
/// </summary>
public class TilesConfig
{
    /// <summary>
    /// Gets or sets the floor tile ID.
    /// </summary>
    public int Floor { get; set; } = 1;

    /// <summary>
    /// Gets or sets the wall tile ID.
    /// </summary>
    public int Wall { get; set; } = 2;

    /// <summary>
    /// Gets or sets the door tile ID.
    /// </summary>
    public int Door { get; set; } = 3;

    /// <summary>
    /// Gets or sets the spawn point tile ID.
    /// </summary>
    public int SpawnPoint { get; set; } = 4;

    /// <summary>
    /// Gets or sets the boss spawn tile ID.
    /// </summary>
    public int BossSpawn { get; set; } = 5;
}

/// <summary>
/// Represents a range of integers (min to max).
/// </summary>
public class IntRange
{
    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntRange"/> class.
    /// </summary>
    public IntRange()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntRange"/> class.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Parses a string like "8-12" or "10" into an IntRange.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed range.</returns>
    public static IntRange Parse(string value)
    {
        if (value.Contains('-'))
        {
            var parts = value.Split('-');
            return new IntRange(
                int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
                int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture));
        }

        var singleValue = int.Parse(value.Trim(), CultureInfo.InvariantCulture);
        return new IntRange(singleValue, singleValue);
    }
}

/// <summary>
/// Represents a range of room sizes.
/// </summary>
public class SizeRange
{
    /// <summary>
    /// Gets or sets the minimum width.
    /// </summary>
    public int MinWidth { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum height.
    /// </summary>
    public int MinHeight { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum width.
    /// </summary>
    public int MaxWidth { get; set; } = 12;

    /// <summary>
    /// Gets or sets the maximum height.
    /// </summary>
    public int MaxHeight { get; set; } = 12;

    /// <summary>
    /// Parses a string like "5x5 to 12x12" or "5x5" into a SizeRange.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed size range.</returns>
    public static SizeRange Parse(string value)
    {
        var result = new SizeRange();

        if (value.Contains(" to ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = value.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
            var minParts = parts[0].Trim().ToLowerInvariant().Split('x');
            var maxParts = parts[1].Trim().ToLowerInvariant().Split('x');

            result.MinWidth = int.Parse(minParts[0], CultureInfo.InvariantCulture);
            result.MinHeight = int.Parse(minParts[1], CultureInfo.InvariantCulture);
            result.MaxWidth = int.Parse(maxParts[0], CultureInfo.InvariantCulture);
            result.MaxHeight = int.Parse(maxParts[1], CultureInfo.InvariantCulture);
        }
        else
        {
            var sizeParts = value.Trim().ToLowerInvariant().Split('x');
            var width = int.Parse(sizeParts[0], CultureInfo.InvariantCulture);
            var height = int.Parse(sizeParts[1], CultureInfo.InvariantCulture);

            result.MinWidth = width;
            result.MinHeight = height;
            result.MaxWidth = width;
            result.MaxHeight = height;
        }

        return result;
    }
}
