# Static Wall And Barrier Blocker Implementation

Date: `2026-04-26`

Target: move ArrowMaze toward clean-room competitor-like special-object gameplay without copying competitor assets, code, levels, or layouts.

## What Was Implemented

This patch starts the special object system with deterministic movement blockers:

- `StaticWall`
- `Barrier`

These are now gameplay objects, not only visual decorations.

## Code Added Or Changed

### Level Data

- Added `BoardObjectData` to `Assets/Scripts/Data/LevelData.cs`.
- Added JSON import/export support for an `Objects` array in `Assets/Scripts/LevelImporter.cs`.
- Added clean-room object constants in `Assets/Scripts/ArrowMaze/LevelContent/BoardObjectTypes.cs`.
- Added object parse/export support in `LevelJsonSerializer`.
- Added object mapping in `LegacyLevelAdapter`.

Current supported JSON shape:

```json
{
  "Name": "Level_Example",
  "XSize": 10,
  "YSize": 10,
  "TimeLimit": 180,
  "Arrows": [],
  "Objects": [
    {
      "Id": "wall_000",
      "ObjectType": "StaticWall",
      "Indices": [42]
    }
  ]
}
```

### Gameplay Model

- `BoardModel` now stores board objects.
- `BoardOccupancy` now tracks object cells separately from snake cells.
- `MoveValidator` now blocks escape routes when a snake hits a `StaticWall` or `Barrier`.
- `SnakeMoveResult` now reports:
  - `BlockerId`
  - `BlockerType`
  - `BlockedCell`
- `CollisionResolver` now exposes object-blocker lookup.

### Validation

`LevelValidator` now checks board objects for:

- missing object type,
- unsupported object type warnings,
- missing cells,
- out-of-bounds cells,
- duplicate cells inside the same object,
- overlap with snake cells,
- overlap with other objects,
- legal-first-move checks that include wall/barrier blockers.

### Runtime Scene Integration

`GameManager` now:

- includes object cells in visible board grid generation,
- spawns basic wall/barrier views using the existing grid prefab,
- colors static walls dark and barriers brown,
- flashes the blocking object when a snake hits it,
- logs the number of board objects when the gameplay model loads.

### Level Generation

`CleanRoomLevelGenerator` now:

- can place original static walls from level 12 onward,
- serializes generated walls into the level JSON,
- validates and solvability-checks generated levels after wall placement.

Generated/audit menu logs now include object counts.

### Smoke Test

`ArrowMaze/Run MoveValidator Smoke Tests` now includes:

- clear route can escape,
- snake blocker blocks route,
- static wall blocks route,
- escaped snake is removed from occupancy.

## Current Parity Impact

Before this patch, special movement rules were effectively `0-10/100`: the project only understood snake-vs-snake blockers.

After this patch:

- static obstacle blocker logic exists,
- level data can represent blockers,
- runtime can display and flash blockers,
- generator can create levels with blockers,
- validator can reject illegal blocker placement.

Estimated special-object parity after Unity compile/playtest: `15/100`.

Overall gameplay parity target impact after Unity compile/playtest:

- strict current parity can move from about `40/100` toward `43-45/100`,
- movement/system foundation can move closer to the next `55/100` gate.

## What Is Still Missing

These competitor-like special systems are still not implemented:

- king/special snake rules,
- way blockers with unlock/destruction rules,
- black holes,
- portals,
- pipes,
- direction changers,
- warning lines,
- object intro tutorials,
- object-specific animation and audio,
- proper object prefabs/art instead of temporary grid-prefab visuals.

## Unity Editor Checks Needed

Open Unity and run:

1. `ArrowMaze/Run MoveValidator Smoke Tests`
2. `ArrowMaze/Validate Current Levels`
3. `ArrowMaze/Validate Gameplay Model`
4. `ArrowMaze/Generate Original Levels/Audit Current Level Pack`

Then manually test:

1. Add a small `Objects` entry to a test JSON level.
2. Place the wall directly in front of one snake head.
3. Start the level.
4. Tap that snake.
5. Confirm the snake bumps/returns and the wall flashes.
6. Tap a valid snake and confirm it still escapes.

## Clean-Room Boundary

This implementation uses original ArrowMaze code and a generic object schema. It does not copy competitor levels, textures, package identifiers, SDK IDs, UI text, or proprietary logic.
