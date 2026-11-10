# WayBlocker Countdown Implementation

Date: `2026-04-26`

Target: implement the competitor-like removable blocker where a visible number decreases after snake escapes and the blocker is removed at zero.

## Clean-Room Plan

1. Keep `StaticWall` permanent.
2. Keep `Barrier` as a blocker foundation for later breakable rules.
3. Add `WayBlocker` as a count-locked blocker:
   - starts with `LockCount`,
   - blocks movement while `LockCount > 0`,
   - decreases only after a real snake escape is confirmed in `BoardModel`,
   - removes itself from occupancy at zero,
   - removes/hides the runtime visual at zero.
4. Support both ArrowMaze `Objects` JSON and competitor-style `wayBlockers` import for clean-room compatibility.
5. Add smoke-test coverage for blocker unlock behavior.

## Implemented

### Data And JSON

- Added `WayBlocker` to `BoardObjectTypes`.
- `BoardObjectTypes` now understands count-locked blockers.
- `LevelImporter` now imports competitor-style:

```json
"wayBlockers": [
  {
    "position": { "x": 13, "y": 11 },
    "lockTime": 90
  }
]
```

- `LevelJsonSerializer` also supports `wayBlockers` / `WayBlockers`.
- Existing ArrowMaze `Objects` JSON can now use:

```json
{
  "Id": "way_blocker_000",
  "ObjectType": "WayBlocker",
  "LockCount": 10,
  "LockTime": 10,
  "Indices": [42]
}
```

### Model Rules

- `BoardModel.TickWayBlockersAfterSnakeEscape()` decreases every active `WayBlocker`.
- When `LockCount` reaches zero:
  - the blocker is removed from `BoardOccupancy`,
  - the blocker is removed from the live board object list.
- `MoveValidator` blocks a route if it hits an active `WayBlocker`.
- `LevelSolvabilityChecker` now ticks way blockers after simulated snake escapes.

### Runtime

`GameManager` now:

- renders `WayBlocker` objects in a distinct color,
- adds a visible lock-count text mesh,
- decrements blocker text after confirmed snake escapes,
- flashes blockers on decrement,
- shrinks/removes blockers at zero,
- leaves failed taps unable to decrement blockers.

### Generator

`CleanRoomLevelGenerator` now:

- starts adding `WayBlocker` objects from generated level 18 onward,
- assigns original count values,
- validates and solvability-checks generated levels after way blocker placement.

### Smoke Test

`ArrowMaze/Run MoveValidator Smoke Tests` now includes:

- `WayBlocker` blocks the route before enough escapes,
- one confirmed escape decrements the blocker,
- blocker is removed from occupancy at zero,
- blocked snake becomes escapable after the blocker unlocks.

## Current Rules

Current behavior:

```text
On real snake escape:
  for each active WayBlocker:
    LockCount -= 1
    if LockCount <= 0:
      remove from occupancy
      remove visual
```

Failed taps do not reduce the count.

Erase/forced escape paths that call the normal escape completion flow can reduce the count because they remove a snake from the board.

## Still Missing

This is still a first production slice, not the final competitor-like polish:

- no custom WayBlocker prefab/art yet,
- no destruction VFX asset yet,
- no object intro tutorial yet,
- no per-blocker custom decrement rules,
- no special sound for blocker unlock yet,
- no barrier-specific break rule yet.

## Unity Editor Checks Needed

Run:

1. `ArrowMaze/Run MoveValidator Smoke Tests`
2. `ArrowMaze/Validate Current Levels`
3. `ArrowMaze/Validate Gameplay Model`
4. `ArrowMaze/Generate Original Levels/Audit Current Level Pack`

Manual test:

1. Add one `WayBlocker` with `LockCount: 2` in front of a snake route.
2. Tap that snake and confirm it is blocked.
3. Escape two other snakes.
4. Confirm blocker text goes `2 -> 1 -> 0`.
5. Confirm blocker disappears.
6. Tap the previously blocked snake and confirm it escapes.

## Parity Impact

This moves ArrowMaze closer to the competitor because the competitor evidence shows `WayBlocker`, visible text/count fields, `lockTime`, and destruction VFX.

Estimated effect after Unity compile/playtest:

- overall similarity: `52/100` strict to about `58/100` pending,
- gameplay core/code similarity: about `70-72/100` pending,
- special object parity: about `25/100` pending.
