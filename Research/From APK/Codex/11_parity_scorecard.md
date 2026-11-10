# ArrowMaze Competitor-Like Parity Scorecard

Target: clean-room competitor-like functionality, gameplay flow, UX structure, and production readiness.

Do not count copied competitor assets, copied levels, copied text, copied IDs, or copied service configuration as progress. Those are release blockers, not parity.

## Current Scores

| Area | Score | Status |
|---|---:|---|
| Research understanding | `90/100` | Strong competitor feature map exists |
| Implementation plan readiness | `82/100` | Strong plan, now needs execution |
| Current implemented game parity | `52/100` strict, `58/100` pending Unity test | Prototype plays in editor, `MoveValidator` owns real tap decisions, static wall/barrier blockers exist, and count-locked `WayBlocker` rules are implemented |
| Current level pipeline | `60/100` pending Unity test | JSON loading, validation, model audit, solvability checking, difficulty estimation, and clean-room generation menus exist |
| Current gameplay core | `70/100` strict, `72/100` pending Unity test | Tap/move/fail route through `MoveValidator`; static wall/barrier blockers and count-locked WayBlockers are now part of model occupancy |
| Current UI/UX flow | `25/100` | Basic menu/HUD panels only |
| Current modes/events | `0/100` | No challenge/rescue/seasonal framework yet |
| Current content scale | `8/100` actual, `25/100` tooling | 25 local JSON files exist; original generation tools can create more after Unity compile/test |
| Current testing/validation | `25/100` | Validator/model menus added, no automated Unity tests yet |
| Current release readiness | `5/100` | Too early for release hardening |
| Current playable visual/UX likeness | `45/100` pending Unity test | Movement feedback improved; wall/barrier blockers spawn/flash and WayBlockers show decrementing text/remove at zero |

Overall current implemented parity: `52/100` strict, because the editor build was reported working before this patch.

Implementation-progress estimate after this patch: `58/100`, pending Unity confirmation.

Overall current playable look-like-competitor score: `35/100`

## Competitor-Like Milestones

### 40/100

Required:

- level file/name/order fixed,
- current levels validate,
- pure `LevelDefinition` loading works,
- basic validator can be run from Unity,
- current prototype still plays.

### 55/100

Required:

- `BoardModel`,
- `BoardOccupancy`,
- `SnakeModel`,
- `MoveValidator`,
- model tests for clear/blocked movement,
- old and new move results match.

### 70/100

Required:

- scene uses the new core model,
- win/fail/replay stable,
- timer implemented,
- hearts implemented in session system,
- gameplay HUD updated,
- input blocked during movement/modals.

### 82/100

Required:

- special objects started and validated,
- hint/path boosters stable,
- 50+ original levels validate,
- mobile camera/zoom/input stable,
- no major board desyncs.

### 90/100

Required:

- main saga mode,
- challenge mode,
- rescue mode,
- seasonal mode shell,
- 100+ original main levels,
- 30+ challenge levels,
- 10+ rescue levels,
- 24 seasonal levels,
- object tutorials,
- playtest pass for first 50 main levels.

### 95/100

Required:

- 300+ main levels,
- difficulty curve reviewed,
- full audio/haptic/effects polish,
- low-end Android performance stable,
- level editor/generator/validator working,
- duplicate layout detection.

### 100/100

Required:

- production release hardening,
- offline play stable,
- save migration stable,
- fake ads/IAP tested before real SDKs,
- monetization/live-ops isolated behind interfaces,
- release build audit clean,
- no competitor identifiers/assets/levels/text,
- device QA passed,
- Play Console declarations match actual SDK behavior.

## Current Phase Status

| Phase | Status | Notes |
|---|---|---|
| P0 Project audit | Completed | Current scripts/scenes/levels audited and level order repaired |
| P1 Level data and validation | Implemented, pending Unity import | New level model, serializer, validator, and Unity menu added |
| P2 Pure board/snake model | Started | Board model, occupancy, snake model, move validator, escape service, and load-time bridge added |
| P3 Connect current scene to new core | Implemented, pending Unity playtest | Real taps now use `MoveValidator` and `SnakeEscapeService`; old tap movement authority removed |
| P4 Presentation split | Not started | Requires stable model |
| P5 Session flow | Not started | Timer/session not modular yet |
| P6 Gameplay UI | Not started | Basic UI exists only |
| P7 Camera/input feel | Not started | Prototype camera exists |
| P8 Special objects | Started, pending Unity test | StaticWall/Barrier blockers plus count-locked WayBlocker data, validation, occupancy, countdown removal, scene feedback, generator support, and smoke tests added |
| P9 Boosters | Not started | Prototype hint/erase/directional tools exist |
| P10 Modes | Not started | No mode framework yet |
| P11 Level tools | Not started | Basic LevelMaker exists, validator started |
| P12 Polish | Not started | Sound manager exists |
| P13 Gameplay release candidate | Not started | Requires systems/content |
| P14 Content scale | Not started | Requires tools |
| P15 Ads/IAP/live-ops | Not started | Intentionally delayed |
| P16 Release hardening | Not started | Too early |

## Latest Implementation Update

2026-04-26:

- Added clean-room level/domain foundation under `Assets/Scripts/ArrowMaze`.
- Added `ArrowMaze/Validate Current Levels` Unity menu.
- Added current level validation report.
- Found and repaired 9 level file/name mismatches.
- Added pure board/snake gameplay model foundation.
- Added `ArrowMaze/Validate Gameplay Model` Unity menu.
- Added load-time `GameManager` bridge to build/log the new gameplay model without changing movement behavior.
- Added safe tap-time `MoveValidator` comparison against the old movement check.
- Bound spawned prototype arrows to model snakes by source index.
- Synced successful arrow escape into the new `BoardModel`.
- Added input blocking during movement so repeated taps cannot corrupt the comparison path.
- Switched real tap movement authority from old `IsPathClear`/moving collision checks to `MoveValidator`.
- Successful validated moves now execute through `SnakeEscapeService.TryEscape`.
- Blocked validated moves now play fail feedback without starting the escape animation.
- Hints now use model validation instead of old path-clear logic.
- Old public `Arrow.TryMove()` bypass and old path-clear helpers were removed from runtime.
- Added stronger blocked bump/return feedback, camera shake, optional haptics, and escape feedback hooks.
- Added `ArrowMaze/Run MoveValidator Smoke Tests` editor menu.
- Added clean-room original level generator, solvability checker, difficulty estimator, JSON export, and level-pack audit menus.
- Added first special-object slice: `StaticWall`/`Barrier` data, occupancy, validation, `MoveValidator` blocking, runtime blocker visuals/flash, generator placement, and smoke-test coverage.
- Added competitor-like `WayBlocker` countdown behavior: lock counts decrease after confirmed snake escapes, blockers are removed from occupancy at zero, runtime text updates, visuals shrink/remove, generator can place them, and smoke tests cover unlock.

## Next Score Target

Next target: move from `40/100` strict / `50/100` pending to `55/100`.

Required next tasks:

1. Open Unity and let the new scripts compile.
2. Run `ArrowMaze/Validate Current Levels`.
3. Run `ArrowMaze/Validate Gameplay Model`.
4. Confirm prototype still plays after level-order repair.
5. Play Levels 1-5 and tap every arrow, including blocked moves.
6. Confirm valid arrows escape and blocked arrows do not move through blockers.
7. Confirm no `MoveValidator authority refused move` logs appear during normal play.
8. Fix only `MoveValidator`, `MovePlanner`, `CollisionResolver`, or model occupancy if a model failure appears.
9. Add Unity tests for clear/blocked movement.
