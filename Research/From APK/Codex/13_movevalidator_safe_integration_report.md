# MoveValidator Safe Integration Report

Date: 2026-04-26

Target phase: connect the new clean-room gameplay model to the current playable prototype without breaking runtime behavior.

Superseded by: `14_movevalidator_runtime_authority_switch.md`, where the runtime authority was switched from the old path-clear system to `MoveValidator`.

## Current Result

Status: implemented in comparison mode, pending Unity compile and playtest.

This is not the full switch yet. The old prototype movement still decides whether an arrow moves. The new `MoveValidator` now runs on real player taps and compares its answer against the old `GameManager.IsPathClear` logic.

## Code Changes Applied

### `Assets/Scripts/GameManager.cs`

Added migration controls:

- `validateGameplayModelOnLoad`
- `compareMoveValidatorOnTap`

Added runtime model binding:

- Each spawned `Arrow` is bound to the matching `SnakeModel` by source index.
- `currentBoardModel` is cleared when the level clears.
- Escaped arrows now remove their matching snake from `BoardModel`.

Added comparison entrypoint:

- `CompareMoveValidatorForArrow(Arrow arrow)`

Comparison behavior:

- Runs `MoveValidator.Validate(currentBoardModel, snake)`.
- Runs old `IsPathClear(arrow)`.
- Logs `MoveValidator match ...` when both systems agree.
- Logs `MoveValidator mismatch ...` as an error when they disagree.

### `Assets/Scripts/Arrow.cs`

Tap behavior now calls:

```csharp
gameManager.CompareMoveValidatorForArrow(this);
```

before running the existing prototype move.

Escape paths now call:

```csharp
gameManager.OnArrowEscaped(this);
```

so the new model can remove the same snake that the old scene logic removed.

Input protection was strengthened:

- Tap input now checks `GameManager.IsGameplayInputBlocked`.
- Movement and forced movement set `GameManager.IsBusy` while animating.
- Failed movement clears the busy state after the arrow returns.
- Successful movement clears the busy state before the arrow object is destroyed.

## What This Means

Current runtime authority:

```text
Player tap
-> MoveValidator comparison log
-> old prototype TryMove / IsPathClear still controls movement
-> old visuals animate
-> escaped arrow updates both old grid and new BoardModel
```

This is the correct safe step. It lets us find logic gaps in `MoveValidator`, `MovePlanner`, or `CollisionResolver` without breaking the playable game.

## Unity Test Checklist

Before raising the parity score, test in Unity:

1. Open the project and wait for C# compile.
2. Select the scene object with `GameManager`.
3. Confirm these are enabled:
   - `Validate Gameplay Model On Load`
   - `Compare Move Validator On Tap`
4. Clear the Console.
5. Play Level 1.
6. Tap every arrow, including arrows that should be blocked.
7. Repeat for Levels 2, 3, 4, and 5.
8. Confirm there are zero console errors containing:
   - `MoveValidator mismatch`
9. Confirm successful arrows still move and leave the board.
10. Confirm blocked arrows still fail visually and input unlocks after the return animation.
11. Confirm the board can still complete and show the win flow.

## Expected Logs

Good:

```text
MoveValidator match on snake_0: canMove=True, status=CanEscape.
MoveValidator match on snake_4: canMove=False, status=BlockedBySnake.
Gameplay model marked escaped: snake_0. Remaining model snakes: 8.
```

Bad:

```text
MoveValidator mismatch on snake_2: old=True, new=False, status=BlockedBySnake, blocker=snake_5 at (3, 4), head=(1, 4), direction=Right, level=2
```

If a mismatch appears, do not change the old movement code. Fix only:

- `Assets/Scripts/ArrowMaze/Gameplay/Snakes/MoveValidator.cs`
- `Assets/Scripts/ArrowMaze/Gameplay/Snakes/MovePlanner.cs`
- `Assets/Scripts/ArrowMaze/Gameplay/Snakes/CollisionResolver.cs`
- board occupancy/model code if the mismatch is caused by model state, not scene state

## Do Not Do Yet

Do not remove the old movement check yet.

Do not add:

- ads,
- shop,
- boosters,
- modes,
- new UI,
- special objects.

Those come after the movement authority is proven stable.

## Score Impact

Current strict score:

- System parity remains `40/100` until Unity confirms compilation and zero mismatch logs.
- This implementation is the bridge needed to reach `55/100`.

Recommended score after Unity confirms zero mismatches on Levels 1-5:

- System parity: `48/100`
- Visual/game feel: `35/100`

Recommended score after the full switch removes the old movement authority:

- System parity: about `55/100`
- Visual/game feel: about `45/100`

## Next Implementation Step

After zero mismatches:

1. Replace old movement decision with `MoveValidator`.
2. Route success through `SnakeEscapeService`.
3. Route failed result through fail feedback.
4. Keep model occupancy as the source of truth.
5. Then start extracting prototype visuals into `SnakeMovementAnimator`.
