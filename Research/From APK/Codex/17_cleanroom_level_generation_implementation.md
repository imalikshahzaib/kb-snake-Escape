# Clean-Room Level Generation Implementation

Date: 2026-04-26

Goal: start filling the biggest competitor gap: scalable original level creation.

## Boundary

This is not competitor level copying. The generator creates original ArrowMaze JSON levels using the current project schema:

```text
Name
XSize
YSize
TimeLimit
Arrows
  Dx
  Dy
  X
  Y
  Indices
```

## Implemented Files

### Level Export

- `Assets/Scripts/ArrowMaze/LevelContent/LevelJsonSerializer.cs`

Added:

- `ToCurrentJson(LevelDefinition level, bool prettyPrint = true)`

This lets generated clean-room `LevelDefinition` data save into the same JSON format the current game loads from `Assets/Resources/Levels`.

### Solvability

- `Assets/Scripts/ArrowMaze/LevelContent/LevelSolvabilityChecker.cs`

Added:

- greedy solve/check loop,
- solution snake order,
- stuck snake reporting,
- escaped/total count.

This follows the required generate -> validate -> solve -> save-only-if-good workflow.

### Difficulty Estimate

- `Assets/Scripts/ArrowMaze/LevelContent/DifficultyEstimator.cs`

Added:

- snake count,
- total path nodes,
- board area,
- numeric score,
- normal/hard/super hard estimate.

### Generator

- `Assets/Scripts/ArrowMaze/Tools/LevelGeneration/GeneratedLevelSettings.cs`
- `Assets/Scripts/ArrowMaze/Tools/LevelGeneration/CleanRoomLevelGenerator.cs`

Generator behavior:

- uses original random construction,
- creates ordered snake paths,
- keeps paths contiguous,
- prevents illegal overlap,
- only places snakes whose escape route is clear at the time they are added,
- validates level after construction,
- runs solvability check after construction,
- returns failure instead of saving bad content.

### Unity Menus

- `Assets/Scripts/ArrowMaze/Editor/GeneratedLevelPackMenu.cs`

Adds:

```text
ArrowMaze/Generate Original Levels/Generate One Next Level
ArrowMaze/Generate Original Levels/Generate Next 25 Main Levels
```

- `Assets/Scripts/ArrowMaze/Editor/GeneratedLevelAuditMenu.cs`

Adds:

```text
ArrowMaze/Generate Original Levels/Audit Current Level Pack
```

The audit writes:

```text
Research/From APK/Codex/generated_level_audit_report.md
```

## How To Use In Unity

1. Open Unity and wait for C# compile.
2. Run:

```text
ArrowMaze/Run MoveValidator Smoke Tests
```

3. Run:

```text
ArrowMaze/Generate Original Levels/Generate One Next Level
```

4. If good, run:

```text
ArrowMaze/Generate Original Levels/Generate Next 25 Main Levels
```

5. Run:

```text
ArrowMaze/Generate Original Levels/Audit Current Level Pack
```

6. Play the generated levels.

## What This Adds Toward Competitor Similarity

Before this implementation:

- the project had only manually created/imported local JSON levels,
- no repeatable generation path,
- no solve-before-save generator gate,
- no difficulty estimation.

After this implementation:

- ArrowMaze can generate original levels in the current runtime format,
- generated levels are validated,
- generated levels are checked for solvability,
- difficulty tags can be estimated,
- content scale can grow without copying competitor levels.

## Score Impact

Pending Unity compile/playtest:

- level pipeline: `45/100` -> `60/100`
- content-production capability: `15/100` -> `45/100`
- actual content scale remains low until generated levels are created and playtested.

Actual content scale should only increase after:

- generated files exist,
- audit passes,
- at least 20 generated levels are playtested.

## Still Missing For 100 Similarity

This generator currently supports normal snake levels only.

Still needed:

- static wall generation,
- barrier generation,
- portal generation,
- black hole generation,
- way blocker generation,
- mode-specific packs,
- generated tutorial/introduction levels,
- duplicate layout hash detection,
- author solution replay storage,
- large-scale batch balancing.

## Next Recommended Implementation

Implement the first special object model:

1. `StaticWall` data definition.
2. Occupancy support for wall cells.
3. `MoveValidator` checks wall blockers.
4. Level validator rejects illegal wall overlap.
5. Generator can place walls.
6. Add 5 original wall tutorial levels.
