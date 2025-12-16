namespace DeSales.DungeonHelper.Models;

/// <summary>
/// Represents the type of a tile in the dungeon map.
/// </summary>
public enum TileType
{
    /// <summary>Nothing / void.</summary>
    Empty = 0,

    /// <summary>Walkable floor tile.</summary>
    Floor = 1,

    /// <summary>Solid wall tile (collision).</summary>
    Wall = 2,

    /// <summary>Door / room connection point.</summary>
    Door = 3,

    /// <summary>Player spawn location.</summary>
    SpawnPoint = 4,

    /// <summary>Boss spawn location.</summary>
    BossSpawn = 5,

    /// <summary>Loot/treasure point.</summary>
    LootPoint = 6,
}
