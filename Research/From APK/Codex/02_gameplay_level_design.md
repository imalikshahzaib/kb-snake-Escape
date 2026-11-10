# Gameplay And Level Design Research

## Core Loop

The competitor is a grid-based snake/arrow escape puzzle. The implementation evidence points to this player loop:

1. A board is generated from level data.
2. Each snake/arrow has a path made of grid nodes.
3. Player taps/selects a snake.
4. The game checks whether the snake's route/escape direction is clear.
5. If clear, the snake moves along its stored path and exits the board.
6. If blocked, it triggers failed tap/collision feedback.
7. The player clears all required snakes before losing hearts/time/moves.
8. Completion gives coins and may offer a rewarded-ad double claim.

Relevant assembly names:

- `GetArrowTracePath`
- `IsAheadClear`
- `MoveAlongPath`
- `MoveBackByPath`
- `ArrowGoAwayEvent`
- `ArrowEscapeFailedEvent`
- `ClearBoardEvent`
- `LevelCompleteEvent`
- `LevelFailedEvent`

## Main Runtime Objects

Serialized/prefab evidence shows these object types:

- Normal snake/arrow: `Arrow`, `Snake`, `SnakeRenderer`, `SnakeBodyRenderer`.
- King/special snake: `KingArrow`, `SnakeKing`, `KingSnakeBodyRenderer`.
- Board: `Board`, `Grid`, `GameController`.
- Barrier: destructible/length/direction obstacle with bar/base sprites and text.
- Way blocker: timed blocker with `lockTime`.
- Black hole: object with size, likely consumes/escapes snakes instantly.
- Portal: two linked positions with color ID.
- Pipe/direction changer: route modifier objects.
- Warning line/sparkle/touch/combo effects.

## Object Serialized Fields

Important field evidence from MonoBehaviour JSON:

- `Arrow.json`
  - `_renderer`
  - `_tracer`
  - `_pos`
  - `_color`
  - `index`
  - `isTapFailed`
- `KingArrow.json`
  - same as `Arrow`, with `_color` preset to `4` in the exported sample.
- `Barrier.json`
  - `_renderer`, `_baseSr`, `_barSr`, `_collider`, `_text`
  - `_destructionVfx`
  - `_barSprite`
  - `_baseSprite`
  - `direction`
  - `length`
- `BlackHole.json`
  - `_sizeHandler`
  - `size`
- `Portal.json`
  - `_colorSprite`
  - `_mainSr`
  - `_sizeHandler`
  - `_other`
  - `_id`
  - `size`
- `WayBlocker.json`
  - `_destructionVfx`
  - `_radioState`
  - `lockTime`
  - `size`
- `GameController.json`
  - `_board`
  - `totalKings`
  - `remainingTime`
  - ad watched flags for grid, hint, revive heart, revive time.

## Level Packs

Evidence:

- `ExportedProject\Assets\Addressables\asset-game-level`
- `asset-game-level-2`
- `asset-game-level-3`
- `asset-game-level-4`

Level inventory:

| Folder | Numeric range | Numeric count | Extra files | Notes |
|---|---:|---:|---|---|
| `asset-game-level` | `lv0` to `lv1450` | 1451 | 11 named files | main saga, encoded/base64-binary style |
| `asset-game-level-2` | `lv1` to `lv80` | 80 | none | mode 2, encoded/base64-binary style |
| `asset-game-level-3` | `lv1` to `lv100` | 100 | `lvportal.json` | plaintext JSON |
| `asset-game-level-4` | `lv1` to `lv24` | 24 | none | plaintext JSON, likely Xmas/Winter |

Named special files in main saga:

- `lvheartArrow.json`
- `lvjp1.json`
- `lvjp1_2.json`
- `lvjp2.json`
- `lvjp3.json`
- `lvjp4.json`
- `lvjp5_1.json`
- `lvjp5_2.json`
- `lvjp5_3.json`
- `lvlipstick.json`
- `lvPokeball.json`

## Plaintext Level Schema

Plaintext level files use this shape:

```json
{
  "size": {"x": 25, "y": 30},
  "timeLimit": 180,
  "name": "",
  "arrows": [
    {
      "nodes": [{"x": 18, "y": 3}, {"x": 18, "y": 4}],
      "color": 13
    }
  ],
  "k": [],
  "wayBlockers": [],
  "blackHoles": [],
  "portals": []
}
```

Meaning inferred from names and prefabs:

- `size`: board width/height in grid cells.
- `timeLimit`: level timer in seconds.
- `arrows`: normal snake/arrow paths.
- `nodes`: ordered grid path for snake body and movement.
- `color`: color ID mapped through `ColorConfig.asset`.
- `k`: special/king snakes.
- `wayBlockers`: timed blockers with position and lock time.
- `blackHoles`: hole positions.
- `portals`: linked portal endpoint pairs.

## Plaintext Mode 3 Stats

Folder: `asset-game-level-3`

- Files parsed: `101` (`100` numeric plus `lvportal`).
- All parsed levels use `timeLimit: 180`.
- Board sizes:
  - `25x30`: 61 files
  - `26x30`: 7 files
  - `26x33`: 32 files
  - `5x5`: 1 file (`lvportal`)
- Snakes per level:
  - minimum: `1`
  - maximum: `141`
  - average: `85.1`
- Total path nodes per level:
  - minimum: `5`
  - maximum: `858`
  - average: `768.7`
- Special objects across this pack:
  - way blockers: `76`
  - black holes: `71`
  - portals: `1`

Examples of special object placement:

- `lvportal`: one portal pair: `pos1 (0,4)`, `pos2 (4,2)`, `colorID 0`.
- `lv3`: one blocker at `(13,11)` with `lockTime 90`, one black hole at `(13,24)`.
- `lv6`: blockers at `(17,13)` lock `40` and `(1,29)` lock `41`; one black hole at `(15,13)`.
- `lv22`: blockers at `(5,16)`, `(5,5)`, `(19,21)` with locks `67`, `46`, `76`; black holes at `(19,10)` and `(5,27)`.

## Plaintext Xmas/Winter Mode Stats

Folder: `asset-game-level-4`

- Files parsed: `24`.
- All parsed levels use `timeLimit: 180`.
- Board sizes:
  - `25x25`: 2
  - `25x29`: 1
  - `25x30`: 5
  - `25x31`: 1
  - `25x33`: 1
  - `26x35`: 1
  - `30x30`: 5
  - `30x34`: 2
  - `35x30`: 1
  - `35x35`: 3
  - `40x40`: 2
- Snakes per level:
  - minimum: `48`
  - maximum: `123`
  - average: `69.7`
- Total path nodes per level:
  - minimum: `410`
  - maximum: `942`
  - average: `609.5`
- Special objects:
  - way blockers: `4`
  - black holes: `0`
  - portals: `0`

Xmas/Winter per-level summary:

| Level | Size | Time | Arrows | Special/king | Blockers | Holes | Portals | Colors |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| lv1 | 25x25 | 180 | 60 | 1 | 0 | 0 | 0 | 1,3,4,8,28 |
| lv2 | 25x25 | 180 | 52 | 1 | 0 | 0 | 0 | 1,12,15 |
| lv3 | 25x30 | 180 | 66 | 1 | 0 | 0 | 0 | 1,9,12,15,28 |
| lv4 | 25x30 | 180 | 47 | 1 | 0 | 0 | 0 | 1,3,4,8,12 |
| lv5 | 25x30 | 180 | 66 | 1 | 0 | 0 | 0 | 2,4,7,8,10,12,29 |
| lv6 | 25x30 | 180 | 55 | 1 | 1 | 0 | 0 | 1,3,4,6,9,13,28 |
| lv7 | 25x31 | 180 | 59 | 1 | 0 | 0 | 0 | 14,15 |
| lv8 | 35x30 | 180 | 55 | 1 | 0 | 0 | 0 | 0,1,8,12 |
| lv9 | 25x30 | 180 | 57 | 1 | 0 | 0 | 0 | 1,4,12,14 |
| lv10 | 25x29 | 180 | 55 | 1 | 0 | 0 | 0 | 1,3,4,12 |
| lv11 | 30x30 | 180 | 89 | 1 | 0 | 0 | 0 | 1,4,6,12,14,15 |
| lv12 | 30x30 | 180 | 60 | 0 | 0 | 0 | 0 | 0,1,3,12,15 |
| lv13 | 30x30 | 180 | 56 | 1 | 0 | 0 | 0 | 1,9,12,15 |
| lv14 | 30x30 | 180 | 69 | 1 | 0 | 0 | 0 | 1,4,12,28 |
| lv15 | 30x30 | 180 | 81 | 1 | 0 | 0 | 0 | 1,4,12,15,28 |
| lv16 | 40x40 | 180 | 123 | 0 | 1 | 0 | 0 | 1,12,14,15 |
| lv17 | 26x35 | 180 | 69 | 1 | 0 | 0 | 0 | 0,1,9,12,15,26,32 |
| lv18 | 25x33 | 180 | 56 | 1 | 0 | 0 | 0 | 1,2,4,7,8,14 |
| lv19 | 30x34 | 180 | 87 | 1 | 0 | 0 | 0 | 0,1,4,9,12,15,28 |
| lv20 | 30x34 | 180 | 65 | 0 | 0 | 0 | 0 | 1,12,14,28 |
| lv21 | 35x35 | 180 | 56 | 1 | 0 | 0 | 0 | 12,14 |
| lv22 | 35x35 | 180 | 78 | 1 | 1 | 0 | 0 | 1,3,4,8,12 |
| lv23 | 35x35 | 180 | 72 | 0 | 1 | 0 | 0 | 7 |
| lv24 | 40x40 | 180 | 120 | 0 | 0 | 0 | 0 | 12,15,27 |

## Color System

Evidence:

- `ExportedProject\Assets\Resources\ColorConfig.asset`

Color IDs `1-15` have names:

- `1` Red
- `2` Blue
- `3` Green
- `4` Yellow
- `5` Purple
- `6` Pink
- `7` Cyan
- `8` Dark Green
- `9` Orange
- `10` Lime
- `11` Dark Violet
- `12` Black
- `13` Gray
- `14` Brown
- `15` White

Color IDs `16-32` exist but have blank names in the recovered config. They still map to sprite sets and are used by seasonal/special levels.

Rainbow color list found in config:

- Red, Orange, Yellow, Green, Cyan, Blue, Purple.

Implementation implication:

- Use a color ID -> head/body/tail/tail-joint sprite mapping.
- Keep level data independent from actual sprite assets.
- Support unnamed/special theme color IDs separately from the basic named palette.

## Difficulty And Modes

Mode/difficulty evidence:

- `Game.Enum.dll` strings include `Mode1`, `Mode2`, `Mode3`, `Xmas`, `Normal`, `Hard`, `Super Hard` UI text.
- `HomePanel.prefab` contains Normal/Hard/Super Hard sections.
- `ChallengeModePanel.prefab` shows chapters and locked chapter text.
- `RescueModePanel.json` references a level map and unlock chapter text.
- `XmasModePanel.json`, `CurrentXmasLevelView.json`, and `asset-ui-xmas` exist.

Inferred mode structure:

- Main saga: many levels, likely normal/hard/super-hard variations.
- Challenge mode: chapter-based, with 10 visible level nodes per chapter and later chapter unlocks.
- Rescue mode: separate map/progression and endgame.
- Xmas/Winter mode: seasonal limited mode with 24 plaintext levels.

Known unlock text from prefabs:

- Challenge unlock: `Unlock at Lv.50`.
- Third challenge chapter text: `Chapter 3\nUnlock At Lv.75`.
- Some home edge modes show `Lv.25`.
- Booster unlocks:
  - Hint: level `6`.
  - Ruler/grid booster: level `12`.

## Failure And Recovery

Failure systems found:

- Hearts: `HeartView.json` has three heart references.
- Fail panels: `LevelFailedPanel`, `LevelFailedMode2Panel`, `LevelFailedMode3Panel`, `LevelFailedMode4Panel`.
- Timeout panel: `TimeOutPanel`.
- Revive panel: `RevivePanel`.
- GameController tracks `remainingTime`, revive ad watched flags, and booster ad watched flags.

Recovered UI text and config imply:

- Base hearts: 3.
- Out of hearts and timeout are separate fail reasons.
- Revive can add `+1` via free/reward flow.
- `+3` hearts option appears with `300` coin price.
- Revive rewarded ads unlock from level `3` and allow `2` uses per level.

## Boosters

Evidence:

- `DataBooster.json`
- `HintButton.json`
- `RulerBoosterButton.json`
- `AdConfig_Default.asset`
- `PurchaseBoosterPanel.prefab`

Booster data:

| Resource type | Booster type | Price |
|---:|---:|---:|
| 4 | 1 | 300 |
| 2 | 2 | 900 |
| 5 | 4 | 900 |

Serialized button unlocks:

- Hint button unlocks at level `6`.
- Ruler/grid booster button unlocks at level `12`.

Ad reward booster config:

- Hint: show from level `6`, limit `2` per level.
- Ruler: show from level `12`, limit `2` per level.
- ReviveTime: show from level `3`, limit `2` per level.
- ReviveLive: show from level `3`, limit `2` per level.

Likely booster functions from naming and UI:

- Hint: shows a valid snake/arrow move.
- Ruler/grid booster: visualizes direction/path/grid escape guidance.
- Add time/revive time: extends timer after timeout.
- Revive live: restores hearts.
- Obstacle removal appears in purchase panel text and enum/resource names.

## Movement Feel

Assembly names suggest:

- `MoveBaseSpeed`
- `ArrowMoveAcceleration`
- `MovementData`
- `MovementRecord`
- `moveSequence`
- `moveState`
- `HoldArrowEvent`
- `CancelHoldEvent`
- `CameraShakeEvent`

Implementation implication:

- Movement should not be instant. It should animate along the stored node path.
- Failed tap should give feedback, probably shake/haptic/sound/color flash.
- Camera/board should support zoom/pan because there is `PinchZoomTutorialPanel`, `ZoomBar`, and `CameraController`.

## Original Implementation Notes For ArrowMaze

To get similar gameplay without copying levels:

- Store original ArrowMaze levels in the same conceptual schema:
  - board size,
  - time limit,
  - snakes as ordered node paths,
  - color ID,
  - special snake flag,
  - blockers,
  - black holes,
  - portals.
- Build procedural/internal tooling to author levels instead of importing competitor data.
- Use original color art and original level layouts.
- Implement a level validation pass:
  - every snake path stays in board or exits intentionally,
  - no overlapping body cells unless a special rule permits,
  - blocker/portal/hole positions are in-bounds,
  - at least one valid first move exists,
  - level is solvable with deterministic rules.
