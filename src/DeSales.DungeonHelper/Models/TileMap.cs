namespace DeSales.DungeonHelper.Models;

/// <summary>
/// Represents a 2D grid of tiles.
/// </summary>
public class TileMap
{
    private readonly TileType[,] _tiles;

    /// <summary>
    /// Gets the width of the tile map.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the tile map.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TileMap"/> class.
    /// </summary>
    /// <param name="width">The width of the tile map.</param>
    /// <param name="height">The height of the tile map.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height is less than 1.</exception>
    public TileMap(int width, int height)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be at least 1.");
        }

        Width = width;
        Height = height;
        _tiles = new TileType[width, height];
    }

    /// <summary>
    /// Gets or sets the tile at the specified coordinates.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The tile type at the specified coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of bounds.</exception>
    public TileType this[int x, int y]
    {
        get
        {
            if (!IsInBounds(x, y))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    $"Coordinates ({x}, {y}) are out of bounds for tile map of size ({Width}, {Height}).");
            }

            return _tiles[x, y];
        }
        set
        {
            if (!IsInBounds(x, y))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    $"Coordinates ({x}, {y}) are out of bounds for tile map of size ({Width}, {Height}).");
            }

            _tiles[x, y] = value;
        }
    }

    /// <summary>
    /// Checks if the specified coordinates are within the bounds of the tile map.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>True if the coordinates are in bounds, false otherwise.</returns>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Fills the entire tile map with the specified tile type.
    /// </summary>
    /// <param name="tileType">The tile type to fill with.</param>
    public void Fill(TileType tileType)
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                _tiles[x, y] = tileType;
            }
        }
    }

    /// <summary>
    /// Fills a rectangular area with the specified tile type.
    /// </summary>
    /// <param name="rect">The rectangle defining the area to fill.</param>
    /// <param name="tileType">The tile type to fill with.</param>
    public void FillRect(Rectangle rect, TileType tileType)
    {
        for (var x = rect.X; x < rect.X + rect.Width; x++)
        {
            for (var y = rect.Y; y < rect.Y + rect.Height; y++)
            {
                if (IsInBounds(x, y))
                {
                    _tiles[x, y] = tileType;
                }
            }
        }
    }
}
