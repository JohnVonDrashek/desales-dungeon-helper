namespace DeSales.DungeonHelper.Models;

/// <summary>
/// Represents the type/purpose of a room in the dungeon.
/// </summary>
public enum RoomType
{
    /// <summary>A standard room with no special purpose.</summary>
    Standard,

    /// <summary>The player spawn room (starting room).</summary>
    Spawn,

    /// <summary>The boss encounter room.</summary>
    Boss,

    /// <summary>A room containing treasure/loot.</summary>
    Treasure,
}
