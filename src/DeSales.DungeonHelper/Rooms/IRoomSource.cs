namespace DeSales.DungeonHelper.Rooms;

/// <summary>
/// Source of room blueprints for dungeon generation.
/// Implementations can be procedural, template-based, or hybrid.
/// </summary>
public interface IRoomSource
{
    /// <summary>
    /// Gets blueprints of the specified type from this source.
    /// </summary>
    /// <param name="roomType">The room type ("spawn", "boss", "treasure", "standard").</param>
    /// <param name="random">Random instance for procedural generation.</param>
    /// <returns>Available blueprints of this type. May be infinite for procedural sources.</returns>
    IEnumerable<RoomBlueprint> GetBlueprints(string roomType, Random random);

    /// <summary>
    /// Returns true if this source can provide blueprints of the given type.
    /// </summary>
    bool CanProvide(string roomType);
}
