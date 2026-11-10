# Current ArrowMaze Vs Competitor Rating

Date: 2026-04-26

Scope: file-based project audit of `C:\Users\Muhammad Ali\ArrowMaze` compared against the saved Snake Escape competitor research. This is not a runtime/video visual QA pass because Unity Editor was not available from the shell.

## Short Answer

Current implemented project rating: `40/100`

Current playable visual/UX likeness rating: `30/100`

Reason: ArrowMaze now has the basic tap-to-escape prototype, 25 validated JSON levels, level-order repair, and the first pure board/snake model foundation. But the actual playable scene is still a prototype. Most competitor-like systems, modes, special objects, polished panels, content scale, and production flow are not implemented yet.

## Important Clean-Room Warning

The project contains many competitor-style sprite names under `Assets/Sprites/Snake`, including winter, challenge, rescue, booster, no-ads, shop, portal, blackhole, pipe, wall, barrier, and starter-pack style assets.

If any of these images came from the competitor APK/export, do not ship them. They can be used as local research/reference only. For release, create original ArrowMaze art with similar functional categories, not copied visuals.

## Evidence From Current Project

### Current Implemented Gameplay

Present:

- `GameManager.cs` controls level loading, board spawning, current progress, hearts, win/fail panels, hint, erase mode, directional clear mode, and progression.
- `Arrow.cs` controls snake/arrow input, body rendering, movement, collision feedback, failed return, forced escape, and erase animation.
- `CameraController.cs` provides pan/zoom.
- `LevelImporter.cs` reads the current JSON level schema.
- New pure model foundation exists under `Assets/Scripts/ArrowMaze`.

Missing:

- runtime movement is not yet fully routed through `MoveValidator`,
- no automated Unity tests,
- no special object runtime rules,
- no mode progression framework,
- no production UI panel system,
- no content generator/validator workflow beyond the first validator,
- no real challenge/rescue/seasonal scene flow.

### Current Level Content

Current ArrowMaze local levels:

- `25` JSON files in `Assets/Resources/Levels`.
- File/name mismatch repair is complete.
- Current boards range from `11x11` to `20x20`.
- Current arrow counts range from `1` to `44`.

Competitor local level evidence:

- main saga: about `1462` files,
- mode 2: `80` files,
- mode 3: `100` files plus `lvportal`,
- Xmas/Winter: `24` files,
- total local level files: about `1667`.

Content scale rating: `8/100`

ArrowMaze has usable prototype content, but it is far from competitor scale.

### Current Scenes

Project scenes:

- `Assets/Scenes/GameScene.unity`
- `Assets/Scenes/LevelEditor.unity`
- `Assets/Scenes/SampleScene.unity`

Competitor scene structure:

- `InitScene`
- `LoadingScene`
- `MainScene`

Scene-flow rating: `25/100`

ArrowMaze has a playable game scene and editor scene, but no production boot/loading/main scene flow yet.

### Current UI/UX

Present:

- main menu,
- top bar,
- settings panel,
- level complete panel,
- game over panel,
- hearts,
- arrow count,
- simple buttons for play/settings/directional tools.

Missing compared to competitor:

- polished home screen,
- bottom tabs,
- challenge panel,
- rescue panel,
- winter/event panel,
- shop,
- no-ads panel,
- starter pack panel,
- new-object intro panel,
- booster unlock panel,
- timeout panel,
- revive panel,
- language panel,
- review prompt,
- offline popup,
- polished win/reward/fail loops.

UI/UX rating: `25/100`

### Current Visual Likeness

Present:

- many competitor-style UI/snake images exist in `Assets/Sprites/Snake`,
- sprite names indicate challenge, rescue, winter, boosters, shop, no-ads, special objects, and endgame assets,
- basic arrow/snake prefabs exist.

Missing:

- these assets are not yet clearly wired into full production scene flow,
- actual runtime still appears prototype-driven,
- no full competitor-like home/challenge/rescue/winter screen flow,
- no polished object views or booster panels in active gameplay scripts.

Visual likeness rating: `30/100` for the playable project.

Asset-library likeness is higher, but unused assets do not make the game itself look like the competitor.

### Current Gameplay Mechanics

Present:

- tap/select arrow/snake,
- body segment tap support,
- path movement,
- collision/blocked feedback,
- hearts,
- win/fail,
- hint-like solvable-arrow highlight,
- erase/directional prototype tools,
- pan/zoom camera.

Started:

- pure `BoardModel`,
- `BoardOccupancy`,
- `SnakeModel`,
- `MoveValidator`,
- `SnakeEscapeService`,
- validation menus.

Missing:

- timer-based level fail,
- model-driven runtime movement,
- way blockers,
- black holes,
- portals,
- pipes,
- direction changers,
- king snake,
- walls/barriers,
- object tutorials,
- booster inventory/unlock/use flow,
- challenge/rescue/seasonal rules.

Core gameplay rating: `40/100`

### Current Modes And Events

Present:

- `LevelMode` enum has main/challenge/rescue/seasonal values.
- Assets for challenge/rescue/winter exist.

Missing:

- no `MainSagaMode`,
- no `ChallengeMode`,
- no `RescueMode`,
- no `SeasonalMode`,
- no separate mode progress,
- no challenge chapters,
- no 24-level event pack loader,
- no mode unlock gates.

Modes/events rating: `5/100`

### Current Monetization/Live-Ops

Present:

- no real ad/IAP/analytics implementation,
- some ad/shop/no-ads style sprites exist,
- this is good for now because gameplay is not stable enough for SDKs.

Missing compared to competitor:

- ads,
- IAP,
- no-ads entitlement,
- rewarded revive/reward flow,
- analytics,
- remote config,
- notifications,
- review prompt,
- live content/hotfix systems.

Monetization/live-ops rating: `0/100`

This should intentionally remain low until gameplay is stable.

## Category Ratings

| Category | Score | Reason |
|---|---:|---|
| Research understanding | `90/100` | Competitor systems and content are well documented |
| Current core gameplay | `40/100` | Prototype loop exists; pure model started |
| Current playable visual likeness | `30/100` | Some style/assets exist, but scene is still prototype |
| UI/UX flow | `25/100` | Basic panels only |
| Level/content scale | `8/100` | 25 levels vs competitor's 1600+ local files |
| Special objects | `5/100` | Assets/data plan exist, runtime rules missing |
| Boosters/economy | `10/100` | Hint/erase prototypes exist, no real inventory/economy |
| Modes/events | `5/100` | Enum/assets exist, no mode flow |
| Testing/validation | `25/100` | Validators added, no automated Unity tests yet |
| Release readiness | `5/100` | Too early |

Overall implemented competitor-like parity: `40/100`

Overall actual look-like-competitor score in playable state: `30/100`

## Why The Score Is Not Higher

The project has many visual assets that look like they belong to a Snake Escape style product, but the active game flow is still mostly:

```text
MainMenu -> GameScene -> tap arrows -> complete/fail
```

The competitor is closer to:

```text
Boot/Loading -> Home -> Main/Challenge/Rescue/Event modes
-> polished gameplay HUD -> object tutorials -> boosters
-> fail/revive/reward/shop loops -> live-ops shell
```

That whole product shell is not implemented yet.

## What Will Raise The Score Fastest

To reach `55/100`:

1. Confirm Unity compile after the new scripts.
2. Run `ArrowMaze/Validate Current Levels`.
3. Run `ArrowMaze/Validate Gameplay Model`.
4. Route runtime movement checks through the new `MoveValidator`.
5. Add timer fail and proper session state.
6. Add model tests for clear/blocked/escape behavior.

To reach `70/100`:

1. Replace prototype UI with real gameplay HUD.
2. Add polished fail/complete/revive panels.
3. Add first 3 special objects: wall, barrier, king snake.
4. Add booster inventory and hint/path booster.
5. Expand to 50+ original levels.

To reach `85/100`:

1. Add black hole, portal, pipe, direction changer, warning lines.
2. Add challenge/rescue/seasonal mode shells.
3. Add 100+ original main levels.
4. Add 30+ challenge levels.
5. Add 24 seasonal event levels.
6. Add full object intro/tutorial sequence.

## Immediate Recommendation

Do not focus on visual polish first. The next work should be gameplay architecture:

1. Unity compile check.
2. Validator menu checks.
3. Integrate `MoveValidator` into actual runtime movement.
4. Add timer/session state.
5. Add wall/barrier object support.

Once the runtime behaves like the competitor, then wire the original art/UI into a polished experience.

