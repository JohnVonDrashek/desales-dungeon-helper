using System.Globalization;
using System.Text;

namespace DeSales.DungeonHelper.Tiled;

/// <summary>
/// Represents a tile layer in a TMX map.
/// Stores tile IDs in a 2D grid.
/// </summary>
public class TmxTileLayer
{
    private readonly int[,] _tiles;

    /// <summary>
    /// Gets the name of the layer.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the width of the layer in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the layer in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Initializes a new tile layer.
    /// </summary>
    public TmxTileLayer(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
        _tiles = new int[width, height];
    }

    /// <summary>
    /// Gets or sets the tile ID at the specified coordinates.
    /// </summary>
    public int this[int x, int y]
    {
        get => IsInBounds(x, y) ? _tiles[x, y] : 0;
        set
        {
            if (IsInBounds(x, y))
            {
                _tiles[x, y] = value;
            }
        }
    }

    /// <summary>
    /// Checks if coordinates are within bounds.
    /// </summary>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Fills the entire layer with a tile ID.
    /// </summary>
    public void Fill(int tileId)
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                _tiles[x, y] = tileId;
            }
        }
    }

    /// <summary>
    /// Fills a rectangular area with a tile ID.
    /// </summary>
    public void FillRect(int startX, int startY, int width, int height, int tileId)
    {
        for (var x = startX; x < startX + width; x++)
        {
            for (var y = startY; y < startY + height; y++)
            {
                if (IsInBounds(x, y))
                {
                    _tiles[x, y] = tileId;
                }
            }
        }
    }

    /// <summary>
    /// Converts the tile data to CSV format (TMX standard).
    /// </summary>
    public string ToCsv()
    {
        var sb = new StringBuilder();

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                sb.Append(_tiles[x, y].ToString(CultureInfo.InvariantCulture));
                if (x < Width - 1)
                {
                    sb.Append(',');
                }
            }

            if (y < Height - 1)
            {
                sb.Append(",\n");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Loads tile data from CSV format.
    /// </summary>
    public void FromCsv(string csv)
    {
        var values = csv
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture))
            .ToArray();

        var index = 0;
        for (var y = 0; y < Height && index < values.Length; y++)
        {
            for (var x = 0; x < Width && index < values.Length; x++)
            {
                _tiles[x, y] = values[index++];
            }
        }
    }
}
