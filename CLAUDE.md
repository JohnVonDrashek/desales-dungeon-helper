# Claude Instructions for DeSales.DungeonHelper

## Project Overview

This is a procedural dungeon generation library for MonoGame that generates Isaac-style grid-based dungeons and exports to Tiled-compatible TMX files.

## Git Workflow Rules

**IMPORTANT: Do not push changes without explicit user consent.**

- Commits are allowed without asking
- `git push` requires explicit user approval
- Always ask before pushing to remote repositories

## Build & Test Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run sandbox with a config
dotnet run --project samples/DeSales.DungeonHelper.Sandbox -- /path/to/config.yaml

# Pack for NuGet
dotnet pack src/DeSales.DungeonHelper/DeSales.DungeonHelper.csproj -c Release
```

## Project Structure

- `src/DeSales.DungeonHelper/` - Main library
  - `Configuration/` - YAML config parsing (DungeonConfig, etc.)
  - `Generation/` - Dungeon generation (DungeonGenerator)
  - `Tiled/` - TMX file I/O (TmxMap, TmxTileLayer, etc.)
- `tests/DeSales.DungeonHelper.Tests/` - xUnit tests
- `samples/DeSales.DungeonHelper.Sandbox/` - CLI tool for testing generation
  - `sample-configs/` - Example YAML configurations

## Generation Algorithm

Uses Isaac-style grid growth:
- Rooms are placed on a uniform grid (cell_size controls room size)
- Rooms are directly adjacent (no separate corridors)
- Doors connect adjacent rooms at shared walls
- Boss room placed at farthest point from spawn (BFS distance)
- Treasure rooms placed at dead ends (cells with 1 neighbor)

## Key Configuration Options

```yaml
dungeon:
  cell_size: 10    # Room size in tiles (8=compact, 10=default, 14=spacious)
  exterior: walls  # "walls" or "void"

corridors:
  doors: 1         # Door width in tiles
```

Note: `corridors.style` and `corridors.width` are ignored (kept for backwards compatibility).

## CI/CD

- `.github/workflows/ci.yml` - Runs on PRs, builds and tests
- `.github/workflows/publish.yml` - Runs on push to main, publishes to NuGet
  - Auto-versions as `1.0.0-preview.{run_number}`
