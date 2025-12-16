# Getting Started

Generate your first procedural dungeon in 3 steps.

## Step 1: Install the Package

```bash
dotnet add package DeSales.DungeonHelper
```

Or clone the repository and add a project reference:

```bash
git clone https://github.com/JohnVonDrashek/desales-dungeon-helper.git
```

## Step 2: Create a Configuration File

Create a file called `dungeon.yaml`:

```yaml
dungeon:
  name: my_first_dungeon
  seed: 42
  width: 40
  height: 40

rooms:
  count: 5-7
  types:
    spawn:
      count: 1
      size: 6x6 to 8x8
    boss:
      count: 1
      size: 8x8 to 10x10
    standard:
      count: rest
      size: 5x5 to 8x8

tiles:
  floor: 1
  wall: 2
  door: 3
```

## Step 3: Generate Your Dungeon

```csharp
using DeSales.DungeonHelper.Configuration;
using DeSales.DungeonHelper.Generation;

// Load the configuration
var config = DungeonConfig.LoadFromFile("dungeon.yaml");

// Generate the dungeon
var map = DungeonGenerator.Generate(config);

// Save to TMX file
map.Save("my_first_dungeon.tmx");

Console.WriteLine($"Generated dungeon with {map.GetAllRoomBounds().Count} rooms!");
```

## What You'll Get

The generator creates a TMX map file containing:

- **Tile Layer ("Tiles")** - A grid of tile IDs representing floors, walls, and doors
- **Object Group ("Rooms")** - Rectangle objects marking each room's bounds and type
- **Object Group ("Spawns")** - Point objects for player, boss, and treasure spawn locations

### ASCII Preview

When you run the sandbox project, you'll see output like this:

```
                       ########
                       #......#
                       #......#  ##### #########
                       #......#..+...# #.......#
  #########            #......#. #...# #...P...#
  #.......#            #......+. #.T.+.+.......#
  #.......#            #......#  #...# #.......#
  #.......#            ########  ##### ######+##
  #.......+.                                .
  #########.                                .
           . ########                       .
           ..+......#   #########          #+###
             #......#...+.......#          #...#
             #......+.  #.......#          #...#
             ##+#####   #...B...#..........+...#
               ...      #.......#          #...#
       ##########+##### #.......#          #####
       #..............# #########
       #..............#
       ################

Legend: . = Floor, # = Wall, + = Door, P = Player, B = Boss, T = Treasure
```

## Next Steps

- [[Configuration]] - Learn all the configuration options
- [[TMX Output]] - Understand the generated file format
- [[Runtime Helpers]] - Use the map in your MonoGame project
- [[Examples]] - See more complete examples
