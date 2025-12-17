namespace DeSales.DungeonHelper.Rooms;

/// <summary>
/// Combines multiple room sources with configurable weights.
/// Supports mixing procedural and template-based rooms.
/// </summary>
public sealed class CompositeRoomSource : IRoomSource
{
    private readonly List<(IRoomSource source, double weight)> _sources = [];

    /// <summary>
    /// Adds a source with the given weight.
    /// </summary>
    public void AddSource(IRoomSource source, double weight = 1.0)
    {
        if (weight > 0)
        {
            _sources.Add((source, weight));
        }
    }

    public bool CanProvide(string roomType) => _sources.Any(s => s.source.CanProvide(roomType));

    public IEnumerable<RoomBlueprint> GetBlueprints(string roomType, Random random)
    {
        var applicableSources = _sources.Where(s => s.source.CanProvide(roomType)).ToList();

        if (applicableSources.Count == 0)
        {
            yield break;
        }

        // Normalize weights
        double totalWeight = applicableSources.Sum(s => s.weight);

        while (true)
        {
            // Pick a source based on weights
            double roll = random.NextDouble() * totalWeight;
            double cumulative = 0;

            foreach (var (source, weight) in applicableSources)
            {
                cumulative += weight;
                if (roll < cumulative)
                {
                    // Get a blueprint from this source
                    var blueprint = source.GetBlueprints(roomType, random).FirstOrDefault();
                    if (blueprint != null)
                    {
                        yield return blueprint;
                    }

                    break;
                }
            }
        }
    }
}
