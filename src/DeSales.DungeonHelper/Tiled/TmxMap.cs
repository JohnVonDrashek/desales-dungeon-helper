using System.Globalization;
using System.Xml.Linq;

namespace DeSales.DungeonHelper.Tiled;

/// <summary>
/// Represents a Tiled TMX map file.
/// This is the core format for dungeon representation.
/// </summary>
public class TmxMap
{
    private readonly List<TmxTileLayer> _tileLayers = [];
    private readonly List<TmxObjectGroup> _objectGroups = [];

    /// <summary>
    /// Gets the width of the map in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the map in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the width of each tile in pixels.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Gets the height of each tile in pixels.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Gets all tile layers in the map.
    /// </summary>
    public IReadOnlyList<TmxTileLayer> TileLayers => _tileLayers;

    /// <summary>
    /// Gets all object groups in the map.
    /// </summary>
    public IReadOnlyList<TmxObjectGroup> ObjectGroups => _objectGroups;

    /// <summary>
    /// Initializes a new TMX map.
    /// </summary>
    public TmxMap(int width, int height, int tileWidth, int tileHeight)
    {
        Width = width;
        Height = height;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
    }

    /// <summary>
    /// Adds a new tile layer to the map.
    /// </summary>
    public TmxTileLayer AddTileLayer(string name)
    {
        var layer = new TmxTileLayer(name, Width, Height);
        _tileLayers.Add(layer);
        return layer;
    }

    /// <summary>
    /// Gets a tile layer by name, or null if not found.
    /// </summary>
    public TmxTileLayer? GetTileLayer(string name)
    {
        return _tileLayers.FirstOrDefault(l => l.Name == name);
    }

    /// <summary>
    /// Adds a new object group to the map.
    /// </summary>
    public TmxObjectGroup AddObjectGroup(string name)
    {
        var group = new TmxObjectGroup(name);
        _objectGroups.Add(group);
        return group;
    }

    /// <summary>
    /// Gets an object group by name, or null if not found.
    /// </summary>
    public TmxObjectGroup? GetObjectGroup(string name)
    {
        return _objectGroups.FirstOrDefault(g => g.Name == name);
    }

    /// <summary>
    /// Converts the map to TMX XML format.
    /// </summary>
    public string ToXml()
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            CreateMapElement());

        return doc.Declaration + "\n" + doc.Root;
    }

    private XElement CreateMapElement()
    {
        var mapElement = new XElement("map",
            new XAttribute("version", "1.10"),
            new XAttribute("tiledversion", "1.10.2"),
            new XAttribute("orientation", "orthogonal"),
            new XAttribute("renderorder", "right-down"),
            new XAttribute("width", Width),
            new XAttribute("height", Height),
            new XAttribute("tilewidth", TileWidth),
            new XAttribute("tileheight", TileHeight),
            new XAttribute("infinite", "0"));

        // Add a basic tileset reference
        mapElement.Add(new XElement("tileset",
            new XAttribute("firstgid", "1"),
            new XAttribute("name", "dungeon"),
            new XAttribute("tilewidth", TileWidth),
            new XAttribute("tileheight", TileHeight),
            new XAttribute("tilecount", "256"),
            new XAttribute("columns", "16")));

        // Add tile layers
        foreach (var layer in _tileLayers)
        {
            mapElement.Add(CreateLayerElement(layer));
        }

        // Add object groups
        foreach (var group in _objectGroups)
        {
            mapElement.Add(CreateObjectGroupElement(group));
        }

        return mapElement;
    }

    private static XElement CreateLayerElement(TmxTileLayer layer)
    {
        return new XElement("layer",
            new XAttribute("name", layer.Name),
            new XAttribute("width", layer.Width),
            new XAttribute("height", layer.Height),
            new XElement("data",
                new XAttribute("encoding", "csv"),
                "\n" + layer.ToCsv() + "\n"));
    }

    private static XElement CreateObjectGroupElement(TmxObjectGroup group)
    {
        var groupElement = new XElement("objectgroup",
            new XAttribute("name", group.Name));

        foreach (var obj in group.Objects)
        {
            groupElement.Add(CreateObjectElement(obj));
        }

        return groupElement;
    }

    private static XElement CreateObjectElement(TmxObject obj)
    {
        var element = new XElement("object",
            new XAttribute("id", obj.Id),
            new XAttribute("name", obj.Name),
            new XAttribute("type", obj.Type),
            new XAttribute("x", obj.X.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("y", obj.Y.ToString(CultureInfo.InvariantCulture)));

        if (obj.Width.HasValue)
        {
            element.Add(new XAttribute("width", obj.Width.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (obj.Height.HasValue)
        {
            element.Add(new XAttribute("height", obj.Height.Value.ToString(CultureInfo.InvariantCulture)));
        }

        // Add properties if any
        if (obj.Properties.Count > 0)
        {
            var propsElement = new XElement("properties");
            foreach (var prop in obj.Properties)
            {
                propsElement.Add(new XElement("property",
                    new XAttribute("name", prop.Key),
                    new XAttribute("value", prop.Value)));
            }

            element.Add(propsElement);
        }

        return element;
    }

    /// <summary>
    /// Saves the map to a TMX file.
    /// </summary>
    public void Save(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, ToXml());
    }

    #region Runtime Helpers

    /// <summary>
    /// Gets collision rectangles for all tiles matching the specified tile ID.
    /// </summary>
    /// <param name="layerName">The name of the tile layer to check.</param>
    /// <param name="tileId">The tile ID to treat as collision (e.g., wall tiles).</param>
    /// <returns>A list of rectangles in pixel coordinates.</returns>
    public List<Rectangle> GetCollisionRectangles(string layerName, int tileId)
    {
        var rectangles = new List<Rectangle>();
        var layer = GetTileLayer(layerName);
        if (layer == null)
        {
            return rectangles;
        }

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (layer[x, y] == tileId)
                {
                    rectangles.Add(new Rectangle(x * TileWidth, y * TileHeight, TileWidth, TileHeight));
                }
            }
        }

        return rectangles;
    }

    /// <summary>
    /// Gets a spawn point position by name.
    /// </summary>
    /// <param name="name">The name of the spawn point (e.g., "PlayerSpawn").</param>
    /// <returns>The spawn point position in pixel coordinates, or null if not found.</returns>
    public Vector2? GetSpawnPoint(string name)
    {
        var spawns = GetObjectGroup("Spawns");
        var spawn = spawns?.Objects.FirstOrDefault(o => o.Name == name);
        return spawn != null ? new Vector2(spawn.X, spawn.Y) : null;
    }

    /// <summary>
    /// Gets all spawn points of a specific type.
    /// </summary>
    /// <param name="type">The spawn type (e.g., "spawn", "boss", "treasure").</param>
    /// <returns>A list of spawn point positions in pixel coordinates.</returns>
    public List<Vector2> GetSpawnPointsByType(string type)
    {
        var spawns = GetObjectGroup("Spawns");
        if (spawns == null)
        {
            return [];
        }

        return spawns.Objects
            .Where(o => o.Type == type)
            .Select(o => new Vector2(o.X, o.Y))
            .ToList();
    }

    /// <summary>
    /// Gets a room's bounding rectangle by name.
    /// </summary>
    /// <param name="name">The name of the room object.</param>
    /// <returns>The room bounds in pixel coordinates, or null if not found.</returns>
    public Rectangle? GetRoomBounds(string name)
    {
        var rooms = GetObjectGroup("Rooms");
        var room = rooms?.Objects.FirstOrDefault(o => o.Name == name);
        if (room?.Width == null || room.Height == null)
        {
            return null;
        }

        return new Rectangle(
            (int)room.X,
            (int)room.Y,
            (int)room.Width.Value,
            (int)room.Height.Value);
    }

    /// <summary>
    /// Gets all rooms of a specific type.
    /// </summary>
    /// <param name="type">The room type (e.g., "spawn", "boss", "treasure", "standard").</param>
    /// <returns>A list of room bounds in pixel coordinates.</returns>
    public List<Rectangle> GetRoomsByType(string type)
    {
        var rooms = GetObjectGroup("Rooms");
        if (rooms == null)
        {
            return [];
        }

        return rooms.Objects
            .Where(o => o.Type == type && o.Width.HasValue && o.Height.HasValue)
            .Select(o => new Rectangle((int)o.X, (int)o.Y, (int)o.Width!.Value, (int)o.Height!.Value))
            .ToList();
    }

    /// <summary>
    /// Gets all room bounds in the map.
    /// </summary>
    /// <returns>A list of all room bounds in pixel coordinates.</returns>
    public List<Rectangle> GetAllRoomBounds()
    {
        var rooms = GetObjectGroup("Rooms");
        if (rooms == null)
        {
            return [];
        }

        return rooms.Objects
            .Where(o => o.Width.HasValue && o.Height.HasValue)
            .Select(o => new Rectangle((int)o.X, (int)o.Y, (int)o.Width!.Value, (int)o.Height!.Value))
            .ToList();
    }

    #endregion

    /// <summary>
    /// Loads a TMX map from a file.
    /// </summary>
    public static TmxMap Load(string path)
    {
        var xml = File.ReadAllText(path);
        return FromXml(xml);
    }

    /// <summary>
    /// Parses a TMX map from XML string.
    /// </summary>
    public static TmxMap FromXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        var mapElement = doc.Root ?? throw new InvalidOperationException("Invalid TMX: no root element");

        var width = int.Parse(mapElement.Attribute("width")?.Value ?? "0", CultureInfo.InvariantCulture);
        var height = int.Parse(mapElement.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture);
        var tileWidth = int.Parse(mapElement.Attribute("tilewidth")?.Value ?? "16", CultureInfo.InvariantCulture);
        var tileHeight = int.Parse(mapElement.Attribute("tileheight")?.Value ?? "16", CultureInfo.InvariantCulture);

        var map = new TmxMap(width, height, tileWidth, tileHeight);

        // Parse tile layers
        foreach (var layerElement in mapElement.Elements("layer"))
        {
            var layer = map.AddTileLayer(layerElement.Attribute("name")?.Value ?? "Unnamed");
            var dataElement = layerElement.Element("data");
            if (dataElement != null)
            {
                layer.FromCsv(dataElement.Value);
            }
        }

        // Parse object groups
        foreach (var groupElement in mapElement.Elements("objectgroup"))
        {
            var group = map.AddObjectGroup(groupElement.Attribute("name")?.Value ?? "Unnamed");

            foreach (var objElement in groupElement.Elements("object"))
            {
                var id = int.Parse(objElement.Attribute("id")?.Value ?? "0", CultureInfo.InvariantCulture);
                var name = objElement.Attribute("name")?.Value ?? "";
                var type = objElement.Attribute("type")?.Value ?? "";
                var x = float.Parse(objElement.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture);
                var y = float.Parse(objElement.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture);
                var widthAttr = objElement.Attribute("width")?.Value;
                var heightAttr = objElement.Attribute("height")?.Value;

                TmxObject obj;
                if (widthAttr != null && heightAttr != null)
                {
                    var objWidth = float.Parse(widthAttr, CultureInfo.InvariantCulture);
                    var objHeight = float.Parse(heightAttr, CultureInfo.InvariantCulture);
                    obj = new TmxObject(id, name, type, x, y, objWidth, objHeight);
                }
                else
                {
                    obj = new TmxObject(id, name, type, x, y);
                }

                // Parse properties
                var propsElement = objElement.Element("properties");
                if (propsElement != null)
                {
                    foreach (var propElement in propsElement.Elements("property"))
                    {
                        var propName = propElement.Attribute("name")?.Value ?? "";
                        var propValue = propElement.Attribute("value")?.Value ?? "";
                        obj.SetProperty(propName, propValue);
                    }
                }

                group.AddExistingObject(obj);
            }
        }

        return map;
    }
}
