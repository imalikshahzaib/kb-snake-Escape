# MoveValidator Runtime Authority Switch

Date: 2026-04-26

Goal: replace the old prototype movement decision with the new clean-room gameplay model.

## Current Result

Status: implemented in code, pending Unity compile and playtest.

The real tap path now uses `MoveValidator` and `SnakeEscapeService` as the gameplay authority.

## Runtime Flow Now

```text
Player taps arrow
-> Arrow.HandleChildClick
-> GameManager.TryMoveArrowWithGameplayModel
-> MoveValidator validates BoardModel occupancy
-> if clear: SnakeEscapeService marks snake escaped in BoardModel
-> Arrow plays existing escape animation
-> OnArrowEscaped updates remaining count and win flow
-> if blocked: blocker flashes and selected arrow plays fail feedback
```

## Old Authority Removed From Runtime

Removed from the active tap path:

- direct `Arrow.TryMove()` tap bypass,
- old `GameManager.IsPathClear()` movement decision,
- old moving-collision rejection inside `Arrow.MoveRoutine()`,
- old `IsBlockedByOther()` collision authority,
- old tap-time comparison logging as the default path.

The scene grid dictionary still exists for visual registration, editor tools, and current prototype rendering. It is no longer the tap movement decision source.

## Files Changed

### `Assets/Scripts/GameManager.cs`

Added:

- `TryMoveArrowWithGameplayModel(Arrow arrow, Vector3 clickWorldPos)`
- model-authority validation through `MoveValidator`
- successful execution through `SnakeEscapeService.TryEscape`
- blocked feedback routing through the selected `Arrow`
- model-based hint search in `FindSolvableArrow`

Kept:

- `RegisterArrowInGrid`
- `ClearArrowFromGrid`
- `GetArrowAtPos`

Those are still needed by current visuals and `LevelMaker`.

### `Assets/Scripts/Arrow.cs`

Changed:

- tap input now calls `GameManager.TryMoveArrowWithGameplayModel`
- successful movement starts only through `PlayValidatedEscape`
- blocked movement starts through `PlayBlockedFeedback`
- `MoveRoutine` now only animates a validated escape
- old public `TryMove()` bypass was removed
- input now ignores taps during appear animation

## Unity Test Checklist

Open Unity and test:

1. Wait for C# compile.
2. Run `ArrowMaze/Validate Current Levels`.
3. Run `ArrowMaze/Validate Gameplay Model`.
4. Play Level 1.
5. Tap a clearly valid arrow.
6. Confirm it escapes.
7. Tap a blocked arrow.
8. Confirm it does not pass through the blocker.
9. Confirm selected arrow bumps/red-fails and loses heart once.
10. Confirm blocker flashes.
11. Confirm input is locked during movement and fail return.
12. Complete Levels 1-5.
13. Confirm win panel/progression still works.
14. Use hint and confirm it highlights a valid model-selected arrow.

## Expected Console

Good successful move:

```text
MoveValidator authority: snake_003 can escape.
Gameplay model confirmed escaped: snake_003. Remaining model snakes: 8.
Arrow Escaped! 8 arrows remaining.
```

Good blocked move:

```text
MoveValidator authority: snake_004 blocked at 5,3.
```

Bad:

```text
MoveValidator authority refused move: No snake model is bound to this arrow.
```

If that appears, the arrow-to-model binding failed and must be fixed before continuing.

## Current Score Impact

Strict score remains gated by Unity runtime verification.

Code implementation estimate:

- system parity: `50/100`
- gameplay core: `65/100`
- visual/game feel: `38/100`

After Unity confirms Levels 1-5 compile and play correctly:

- system parity can be raised to about `55/100`
- visual/game feel can be raised to about `45/100`

## Next Step After Validation

Do not add ads, shop, modes, or special objects yet.

Next engineering step after this passes:

1. Add automated tests for `MoveValidator` clear/blocked cases.
2. Add a small PlayMode smoke test for tap -> escape and tap -> blocked feedback.
3. Start special object modeling only after basic movement is stable.
