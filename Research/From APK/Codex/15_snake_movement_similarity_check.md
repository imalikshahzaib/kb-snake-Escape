# Snake Movement Similarity Check

Date: 2026-04-26

Question: how similar is ArrowMaze snake movement to the competitor?

## Short Answer

Current movement similarity:

- Normal tap movement rules: `75/100` pending Unity playtest
- Movement visual feel: `58/100` pending Unity playtest
- Overall snake movement similarity: `66/100`

This means ArrowMaze now has the same basic movement idea and a better blocked/escape feel, but not yet the same full polished mobile feel.

Implementation update: see `16_movement_feedback_polish_implementation.md`, `18_static_wall_blocker_implementation.md`, and `19_wayblocker_countdown_implementation.md`.

## Competitor Movement Target

From the competitor research, the target movement behavior is:

1. Board loads from level data.
2. Each snake/arrow is an ordered grid-node path.
3. Player taps a snake.
4. Game checks whether the route/escape direction is clear.
5. If clear, the snake animates along its stored path and exits the board.
6. If blocked, the tap gives fail feedback.
7. Player clears all required snakes before hearts/time run out.

## Current ArrowMaze Movement

Current runtime flow:

```text
Player taps arrow/body segment
-> GameManager.TryMoveArrowWithGameplayModel
-> MoveValidator checks BoardModel occupancy
-> SnakeEscapeService removes snake from BoardModel if valid
-> Arrow.MoveRoutine animates the snake off-board
-> OnArrowEscaped updates remaining count/win flow
```

This is structurally close to the competitor's core movement loop.

## Similar Parts

Implemented and similar:

- Tap on head/body segment selects the snake.
- Movement is grid based.
- Each snake uses ordered body/path nodes.
- Escape validity is now deterministic through `MoveValidator`.
- Occupancy/model state is now the main move authority.
- Valid snake escapes off board.
- Blocked snake does not escape.
- Blocked snake gets visual fail feedback.
- The blocking snake can flash.
- Static walls/barriers can block movement.
- WayBlockers can block movement, count down after confirmed snake escapes, and unlock/remove at zero.
- Input is locked during move/fail return.
- Board clear still drives win flow.

## Not Similar Enough Yet

Still weaker than competitor:

1. Failed movement feel is improved but still not final.
   - Current blocked feedback now includes forward bump, return, red fail, blocker flash, camera shake, collision sound, and optional haptic.
   - It still needs dedicated pooled hit effects and final device tuning.

2. Movement animation is still prototype.
   - Current head/body follows the path, but the body segments are simple objects.
   - Competitor likely uses richer `SnakeRenderer`/`SnakeBodyRenderer` style polish, smoother timing, segment skins, and stronger escape effects.

3. Object-aware movement is started but incomplete.
   - Current `MoveValidator` handles snake occupancy, static walls/barriers, and count-locked WayBlockers.
   - Competitor movement still also needs black holes, portals, pipes, direction changers, richer barriers, and king snakes.

4. Forced/directional movement is still prototype-style.
   - `ForceMoveRoutine` exists as a special action.
   - It is not yet designed as a competitor-like booster/ruler/path-preview system.

5. Timing and progression context are incomplete.
   - The competitor movement exists inside timer/hearts/objective pressure.
   - ArrowMaze has hearts, but full session/timer/objective structure is not yet production-grade.

6. Camera and zoom feel are missing.
   - Competitor has evidence for pinch zoom/tutorial/zoom bar.
   - Current movement is not yet tested against large boards with camera fit/zoom.

## Movement Similarity Rating

| Area | Rating | Reason |
|---|---:|---|
| Tap selection | `75/100` | Head/body tap works, but touch polish is basic |
| Move validation | `82/100` | Deterministic model path exists and blockers started, but advanced objects are still missing |
| Escape animation | `55/100` | Moves along path and exits, but lacks premium timing/effects |
| Blocked feedback | `55/100` | Bump/return, blocker flash, shake, and haptic hooks exist, but final VFX/audio polish is missing |
| Board sync | `72/100` | BoardModel sync exists and WayBlockers remove from occupancy, pending Unity verification |
| Special movement rules | `25/100` | Static walls/barriers and WayBlockers exist; portals/black holes/pipes/king rules still missing |
| Overall movement | `66/100` | Core behavior is close; advanced object depth and premium feel are not yet close |

## Strict Verdict

ArrowMaze movement is now similar in logic, but not yet similar in feel.

The current system is a good clean-room foundation. It should not be considered competitor-like complete until:

- Unity confirms levels 1-5 play without movement bugs,
- blocked moves feel immediate and polished,
- remaining special object movement rules are added,
- movement effects/audio/haptics are separated and tuned,
- large-board camera/zoom is stable.

## Next Fixes To Raise Movement Similarity

Do these next, in order:

1. Unity playtest Levels 1-5 with valid and blocked taps.
2. Run `ArrowMaze/Run MoveValidator Smoke Tests`.
3. Tune blocked bump and shake values on device.
4. Replace reused click particle with dedicated pooled hit/escape effects.
5. Add next special objects: black holes, portals, and king snake rules.

Expected score after these fixes:

- movement rules: `85/100`
- movement feel: `62/100`
- overall movement similarity: `70-74/100`
