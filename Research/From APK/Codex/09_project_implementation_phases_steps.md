# ArrowMaze Project Implementation Phases And Steps

Source research folder: `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Codex`

Target Unity project: `C:\Users\Muhammad Ali\ArrowMaze`

Purpose: this is the practical follow-up plan for applying the competitor research into the current ArrowMaze project. It is written as a step-by-step implementation checklist, not a theory document.

## Clean-Room Rule

Use the competitor only as a behavior and structure reference.

Do not copy competitor assets, exact levels, UI layout, text, package name, ad IDs, product IDs, service IDs, audio, Spine files, or proprietary code.

Build original ArrowMaze code, original levels, original visuals, original UI copy, and ArrowMaze-owned service configuration.

## Current ArrowMaze Project State

Current project evidence:

- Main gameplay scripts are still flat under `Assets/Scripts`.
- Current core scripts:
  - `GameManager.cs`
  - `Arrow.cs`
  - `ArrowSegment.cs`
  - `CameraController.cs`
  - `LevelImporter.cs`
  - `LevelMaker.cs`
  - `SettingsUI.cs`
  - `SoundManager.cs`
  - `Data/LevelData.cs`
- Current scenes:
  - `Assets/Scenes/GameScene.unity`
  - `Assets/Scenes/LevelEditor.unity`
  - `Assets/Scenes/SampleScene.unity`
- Current level loading uses JSON files from `Assets/Resources/Levels`.
- Current level files are not fully clean yet. Some file names and internal `Name` fields do not match. Example: `Level_1.json` currently contains `Name: Level_22`.
- Current packages include Unity Input System, URP, UGUI, and Unity Test Framework. Addressables and monetization SDKs are not currently required for the first gameplay implementation pass.

Important first rule: do not build hundreds of levels on the current level folder until the level audit/fix phase is complete.

## Competitor Reference To Keep In Mind

The competitor folder proves these feature categories exist:

| Competitor area | Evidence from research | ArrowMaze use |
|---|---:|---|
| Main saga | About `1462` local main level files | Build a large original main progression |
| Mode 2 | `80` level files | Use as proof of an extra mode pack |
| Mode 3 | `100` level files plus `lvportal` | Use as proof of another extra mode pack |
| Xmas/Winter event | `24` level files | Build 24-level seasonal packs |
| Challenge UI | Chapter UI, 10 visible nodes, unlock text | Build chapter-based challenge mode |
| Rescue UI | Separate rescue assets/panel evidence | Build rescue/special mode shell |

ArrowMaze should not try to produce 1600+ levels first. The correct order is:

1. Build the same type of gameplay system.
2. Build original authoring/validation tools.
3. Produce original level content at scale.
4. Add monetization/live-ops only after gameplay is stable.

## Target Content Counts

### First Playable Internal Build

- Main levels: `20`
- Challenge levels: `0`
- Rescue levels: `0`
- Seasonal levels: `0`
- Purpose: prove the new gameplay core works.

### Gameplay MVP

- Main levels: `50`
- Challenge levels: `10`
- Rescue levels: `0-5`
- Seasonal levels: `0`
- Purpose: prove progression, UI flow, timer, hearts, boosters, and save data.

### Gameplay Release Candidate

- Main levels: `100+`
- Challenge levels: `30+`
- Rescue levels: `10+`
- Seasonal levels: `24`
- Total player-facing content: about `164+`
- Purpose: strong no-ads gameplay build.

### First Production Content Target

- Main levels: `300+`
- Challenge levels: `30+`
- Rescue levels: `10-30`
- Seasonal levels: `24`
- Total player-facing content: about `364-384+`
- Purpose: serious launch-level content volume while staying realistic.

### Long-Term Competitor-Scale Target

- Main levels: `1000+`
- Extra mode levels: `150+`
- Seasonal/event packs: repeated `24` level packs
- Purpose: later parity with competitor content scale after tools and validation are stable.

## Recommended Level Folder Structure

Move from one flat `Resources/Levels` folder to mode-based folders after the current loader is stable.

```text
Assets/Resources/Levels/
  Main/
    Level_001.json
    Level_002.json
    ...
  Challenge/
    Chapter_01/
      Level_001.json
      ...
    Chapter_02/
    Chapter_03/
  Rescue/
    Rescue_001.json
    ...
  Seasonal/
    Winter_01/
      Level_001.json
      ...
```

Keep the old flat folder only as a temporary compatibility source while migrating.

## Phase P0: Project Audit And Prototype Lock

Goal: freeze the current game behavior and clean level data before refactoring.

Steps:

1. Open `GameScene.unity` and confirm the current prototype still plays.
2. Test this loop manually:
   - main menu opens,
   - play starts the saved/current level,
   - arrow tap moves or fails,
   - level complete panel appears,
   - next level unlocks,
   - restart works,
   - game over works after hearts are lost.
3. Audit all current JSON levels in `Assets/Resources/Levels`.
4. Fix file/name mismatches:
   - file `Level_001.json` should contain `Name: Level_001`,
   - no duplicate internal names,
   - no missing required fields,
   - no broken JSON.
5. Decide the current prototype level order:
   - either restore continuous `Level_1` to `Level_25`,
   - or intentionally move old test levels into `Levels/Prototype`.
6. Record the current behavior as the temporary reference.
7. Create module folders under `Assets/Scripts/ArrowMaze`.
8. Do not delete current flat scripts yet.

Files touched in this phase:

- `Assets/Resources/Levels/*.json`
- `Assets/Scripts/ArrowMaze/` new folders
- optional `Research/From APK/Codex/project_baseline_checklist.md`

Gate:

- Current prototype is playable.
- Level JSON names and file names are consistent.
- There is a clear list of usable prototype levels.
- No competitor files were imported.

## Phase P1: Level Data And Validation Layer

Goal: make level loading safe before changing gameplay.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Common/
  GridCoord.cs
  GridDirection.cs
  Result.cs

Assets/Scripts/ArrowMaze/LevelContent/
  LevelDefinition.cs
  SnakeDefinition.cs
  BoardObjectDefinition.cs
  LevelMode.cs
  LevelDifficulty.cs
  LevelJsonSerializer.cs
  LegacyLevelImporter.cs
  LevelValidationResult.cs
  LevelValidator.cs
  LevelCatalog.cs
  ResourcesLevelProvider.cs
```

Steps:

1. Keep current `LevelImporter` working.
2. Create a new ArrowMaze-owned level model.
3. Create a serializer that can read the current JSON schema:
   - `Name`
   - `XSize`
   - `YSize`
   - `Arrows`
   - `Dx`
   - `Dy`
   - `X`
   - `Y`
   - `Indices`
4. Add schema version support for future fields.
5. Add validation checks:
   - JSON parses,
   - board size is valid,
   - every arrow has at least one cell,
   - all indices convert to valid coordinates,
   - no illegal overlap,
   - direction is valid,
   - file name and level name match,
   - at least one move is possible.
6. Add a Unity editor menu item or test runner that validates all levels.
7. Save validation output to a report file.

Gate:

- Every current level either passes validation or is marked as prototype/broken.
- The game still loads levels using the old path.
- New data classes can load current levels without spawning Unity objects.

## Phase P2: Pure Board And Snake Model

Goal: move core gameplay decisions out of `GameManager` and `Arrow`.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Gameplay/Board/
  BoardModel.cs
  BoardCell.cs
  BoardOccupancy.cs
  BoardBounds.cs
  BoardFactory.cs

Assets/Scripts/ArrowMaze/Gameplay/Snakes/
  SnakeModel.cs
  SnakePath.cs
  SnakeMoveCommand.cs
  SnakeMoveResult.cs
  MoveValidator.cs
  MovePlanner.cs
  CollisionResolver.cs
```

Steps:

1. Convert `LevelDefinition` into `BoardModel`.
2. Register every snake segment into `BoardOccupancy`.
3. Implement clear-path checks using occupancy, not physics.
4. Implement blocked-path result without changing board state.
5. Implement successful escape result that removes the snake from occupancy.
6. Keep movement deterministic.
7. Add EditMode tests for:
   - clear escape,
   - blocked escape,
   - outside-board escape,
   - occupancy removal,
   - invalid overlap rejection,
   - no mutation on failed move.

Gate:

- Core move logic works without loading `GameScene`.
- `GameManager.gridContent` behavior has an equivalent in `BoardOccupancy`.
- No rendering or animation code is inside the model.

## Phase P3: Connect Current Prototype To New Core

Goal: use the new model inside the existing scene before replacing visuals.

Steps:

1. Add `LevelSessionController` as a wrapper around current level play.
2. During level load, create:
   - `LevelDefinition`,
   - `BoardModel`,
   - `BoardOccupancy`,
   - `SnakeModel` list.
3. Keep existing arrow prefabs and visuals.
4. Replace or mirror these current `GameManager` responsibilities:
   - `IsPathBlocked`
   - `IsPathClear`
   - `ClearArrowFromGrid`
   - `RegisterArrowInGrid`
   - `OnArrowEscaped`
5. Route tap decisions through `MoveValidator`.
6. Keep `Arrow.cs` animation for now.
7. Add debug comparison logs for old result vs new result during testing.

Gate:

- Current visuals still play.
- New model and old visuals agree on clear/blocked moves.
- No level completion or next-level unlock regression.

## Phase P4: Presentation Split

Goal: separate gameplay logic from visual objects.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Presentation/Board/
  BoardView.cs
  GridView.cs
  GridCellView.cs
  BoardFitService.cs

Assets/Scripts/ArrowMaze/Presentation/Snakes/
  SnakeView.cs
  SnakeSegmentView.cs
  SnakeViewPool.cs
  SnakeMovementAnimator.cs
  SnakeFailAnimator.cs
  SnakeColorPalette.cs
```

Steps:

1. Move grid spawning out of `GameManager`.
2. Move arrow/snake visual spawning out of `GameManager`.
3. Move body segment display logic out of `Arrow`.
4. Make `SnakeView` display a `SnakeModel`.
5. Make `SnakeMovementAnimator` animate from a move command.
6. Use object pools for snake segments and effects.
7. Make failed move animation visual-only.
8. Ensure input is blocked while movement animation is active.

Gate:

- At least 20 original levels are playable visually.
- No board/model desync after repeated moves.
- Failed moves do not modify occupancy.
- No input is accepted during animation.

## Phase P5: Session Flow, Timer, Hearts, And Progression

Goal: match the competitor-like core gameplay flow.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Gameplay/Session/
  LevelSessionController.cs
  LevelStateMachine.cs
  LevelTimer.cs
  HeartManager.cs
  ObjectiveTracker.cs
  LevelProgressionService.cs
  FailReason.cs
  LevelResult.cs
```

Steps:

1. Add `LevelState`:
   - `Loading`
   - `Spawning`
   - `Playing`
   - `ResolvingMove`
   - `Paused`
   - `Complete`
   - `Failed`
2. Add timer support.
3. Keep default timer near competitor style, commonly `180` seconds unless the level overrides it.
4. Add 3-heart level starts.
5. Decide the bad-tap penalty rule:
   - early levels can ignore heart loss,
   - later levels can lose hearts on blocked move.
6. Track progress as escaped snakes over required snakes.
7. Save next unlocked level immediately when the level is completed.
8. Keep current `CurrentLevelIndex` behavior until migration is complete.

Gate:

- Win works.
- Timeout fail works.
- Out-of-hearts fail works.
- Pause/resume does not corrupt timer or movement state.
- Next level is unlocked after completion.

## Phase P6: Gameplay UI Flow

Goal: create the usable player flow around gameplay.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/UI/Core/
  PanelSystem.cs
  PanelId.cs
  PanelBase.cs
  ModalStack.cs
  SafeAreaFitter.cs

Assets/Scripts/ArrowMaze/UI/Screens/
  HomeScreen.cs
  ModeSelectPanel.cs
  GameHud.cs
  PausePanel.cs
  LevelCompletePanel.cs
  LevelFailedPanel.cs
  TimeoutPanel.cs
  RevivePanel.cs
  SettingsPanel.cs
  TutorialPanel.cs
  NewObjectIntroPanel.cs
  QuickMessagePanel.cs
```

Steps:

1. Home screen opens first.
2. Play button starts main saga current level.
3. HUD shows:
   - level label,
   - timer,
   - progress,
   - hearts,
   - pause,
   - hint,
   - path/ruler booster.
4. Win panel supports:
   - continue,
   - replay if needed,
   - coin reward display later.
5. Fail panel supports:
   - replay,
   - revive later,
   - home.
6. Modal panels block board input.
7. Safe area works on notched phones.
8. UI does not overlap at common portrait resolutions.

Gate:

- Cold launch -> Home -> Play -> Win -> Next works.
- Fail -> Replay works.
- Pause -> Resume/Home works.
- UI never triggers board taps underneath.

## Phase P7: Camera, Zoom, And Input Feel

Goal: make large boards playable like the competitor.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Presentation/Input/
  InputRouter.cs
  TouchInputReader.cs
  MouseEditorInputReader.cs
  TapGestureStrategy.cs

Assets/Scripts/ArrowMaze/Presentation/Camera/
  GameplayCameraController.cs
  PinchZoomController.cs
  ZoomBarController.cs
```

Steps:

1. Route all board taps through `InputRouter`.
2. Support mouse input in editor.
3. Support touch input on mobile.
4. Tapping head or body selects the same snake.
5. Add pinch zoom for larger boards.
6. Add optional zoom bar if pinch is hard to discover.
7. Add one-time zoom tutorial.
8. Fit board so HUD does not cover important cells.

Gate:

- `20x20`, `30x30`, and `40x40` boards are playable.
- UI taps never trigger snake selection.
- Zoom/pan cannot lose the board.

## Phase P8: Special Gameplay Objects

Goal: add competitor-like mechanics one at a time.

Implementation order:

1. Static wall
2. Barrier
3. King snake
4. Way blocker
5. Black hole
6. Portal
7. Pipe
8. Direction changer
9. Warning line

For every object, follow the same sub-steps:

1. Add data fields to level schema.
2. Add validator rules.
3. Add pure model rule.
4. Add visual prefab/view.
5. Add blocked/success feedback.
6. Add one tutorial intro.
7. Add 5 original levels using that object.
8. Add model tests.

Gate:

- Each object works in model tests.
- Each object works in scene.
- Object tutorials appear once and stay dismissed.
- Special objects do not break solvability checks.

## Phase P9: Boosters Without Ads Or IAP

Goal: add gameplay boosters first, without monetization SDK complexity.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Gameplay/Boosters/
  BoosterType.cs
  BoosterDefinition.cs
  BoosterInventory.cs
  BoosterUnlockService.cs
  BoosterUsageService.cs
  HintBooster.cs
  PathPreviewBooster.cs
  ReviveHeartBooster.cs
  ReviveTimeBooster.cs
  RemoveObstacleBooster.cs

Assets/Scripts/ArrowMaze/Economy/
  CurrencyType.cs
  Wallet.cs
  WalletService.cs
  RewardGrantService.cs
```

Steps:

1. Add local wallet/save fields.
2. Unlock hint around level 6.
3. Unlock path/ruler around level 12.
4. Add hint:
   - finds one legal move,
   - highlights it,
   - never selects an impossible move.
5. Add path preview:
   - previews escape route,
   - clears preview after use.
6. Add revive heart:
   - restores hearts after fail.
7. Add revive time:
   - adds time after timeout.
8. Use debug/free grants first, not ads/IAP.

Gate:

- Boosters cannot be used before unlock.
- Boosters cannot corrupt board state.
- Save/load preserves booster counts.

## Phase P10: Modes And Content Packs

Goal: implement competitor-like mode structure in ArrowMaze.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Modes/
  GameMode.cs
  MainSagaMode.cs
  ChallengeMode.cs
  RescueMode.cs
  SeasonalMode.cs
  ModeProgressionService.cs
  ModeUnlockConfig.cs
```

Steps:

1. Add `LevelMode` to every level definition.
2. Add separate progress saves per mode.
3. Main saga is the default play button.
4. Challenge unlocks from main progress, default around level 50.
5. Challenge uses chapters of 10 levels.
6. Rescue has a separate map/panel and separate progress.
7. Seasonal supports 24-level event packs.
8. Modes are enabled/disabled by local config first.
9. Add mode panels:
   - `ChallengeChapterPanel`
   - `RescueModePanel`
   - `SeasonalModePanel`

Content targets for this phase:

- Main: `100` original levels minimum
- Challenge: `3` chapters, `10` levels each
- Rescue: `10` levels minimum
- Seasonal: `24` levels in one event pack

Gate:

- Player can switch modes.
- Each mode loads its own level pack.
- Each mode saves progress separately.
- Main progress unlocks extra modes correctly.

## Phase P11: Level Authoring, Generator, And Validation Tools

Goal: make it possible to create hundreds of original levels safely.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Tools/LevelEditor/
  LevelEditorWindow.cs
  LevelPaintTool.cs
  LevelObjectPalette.cs
  LevelExportTool.cs

Assets/Scripts/ArrowMaze/LevelContent/
  LevelSolvabilityChecker.cs
  DifficultyEstimator.cs
  DuplicateLayoutDetector.cs
```

Steps:

1. Build a level editor workflow.
2. Add export to ArrowMaze JSON.
3. Add validation before export.
4. Add solvability checking or author-solution replay.
5. Add difficulty estimator using:
   - board size,
   - snake count,
   - path length,
   - blockers,
   - portals,
   - black holes,
   - first legal move count,
   - estimated solution depth.
6. Add duplicate layout detection.
7. Build a generator-assisted workflow:
   - generate candidate,
   - validate,
   - solve,
   - score difficulty,
   - reject duplicates,
   - save only good levels.

Gate:

- 100 original levels validate.
- 50 original levels are playtested.
- No copied competitor layouts are present.

## Phase P12: Audio, Haptics, And Feedback Polish

Goal: make the game feel responsive and complete.

Build these scripts:

```text
Assets/Scripts/ArrowMaze/Services/Audio/
  AudioService.cs
  SoundCatalog.cs
  MusicController.cs

Assets/Scripts/ArrowMaze/Presentation/Effects/
  EffectPool.cs
  TouchEffectPlayer.cs
  ComboTextPlayer.cs
  ConfettiPlayer.cs
  CameraShakeService.cs
  HapticService.cs
```

Steps:

1. Add UI click sound.
2. Add snake tap sound.
3. Add moving sound.
4. Add blocked fail sound.
5. Add blocker flash and haptic.
6. Add escape success effect.
7. Add level complete celebration.
8. Add timeout warning tick.
9. Pool all repeated effects.

Gate:

- Major actions have immediate feedback.
- Effects do not create GC spikes.
- Win feedback is satisfying but not slow.

## Phase P13: Gameplay Release Candidate

Goal: finish the no-ads gameplay build.

Required content:

- Main saga: `100+` original levels
- Challenge: `30+` original levels
- Rescue: `10+` original levels
- Seasonal: `24` original levels

Required systems:

- Full board gameplay
- Timer
- Hearts
- Win/fail/replay
- Revive through gameplay tokens/coins
- Hint booster
- Path/ruler booster
- Special objects
- Mode selection
- Local save
- Settings
- Offline play

Gate:

- App can be played offline from launch to at least main level 50.
- No softlocks are found.
- No copied competitor assets, levels, IDs, or text exist.
- Low-end Android performance is stable.

## Phase P14: Production Content Scale

Goal: reach a serious launch content volume.

Steps:

1. Expand main saga from `100+` to `300+`.
2. Keep challenge at `3` chapters minimum.
3. Expand rescue from `10` to `30` if the mode is fun.
4. Keep one seasonal event pack at `24` levels.
5. Add difficulty tags:
   - normal,
   - hard,
   - super hard.
6. Add object introductions gradually.
7. Use playtest data to smooth difficulty spikes.

Gate:

- `300+` main levels validate.
- No duplicate copied layouts.
- Difficulty curve is reviewed.
- At least `100` levels are playtested end to end.

## Phase P15: Ads, IAP, Analytics, And Live-Ops Later

Goal: add production systems only after gameplay is stable.

Do not start this phase until P13 is complete.

Build later:

- `IAdService`
- `NullAdService`
- `AdCooldownService`
- `AdEligibilityService`
- `IPurchaseService`
- `NullPurchaseService`
- `WalletService`
- `EntitlementService`
- `IAnalyticsService`
- `IRemoteConfigService`
- `INotificationService`
- `ReviewPromptGate`

Rules:

- Forced ads disabled through levels `0-25`.
- Rewarded ads are optional only.
- No-ads disables forced ads and banners only.
- Reward callbacks must grant exactly once.
- Remote config must never be required to play local levels.
- App must work offline.

Gate:

- Fake services work in editor.
- Real SDK code is isolated behind adapters.
- No forced ads before configured threshold.
- No-ads behavior is exact.
- Missing network cannot break gameplay.

## Phase P16: Release Hardening

Goal: prepare for real Android release.

Steps:

1. Add build validation runner.
2. Audit `Packages/manifest.json`.
3. Remove unused sample/demo/debug packages if not needed.
4. Audit Android manifest permissions.
5. Check no competitor identifiers exist in the project.
6. Check no test ad IDs, product IDs, or service IDs ship in production.
7. Test low-end Android.
8. Test pause/resume during movement.
9. Test offline launch.
10. Test save migration.
11. Test AAB/split install.

Gate:

- Release build contains no debug menus or competitor identifiers.
- Cold start, memory, FPS, and crash-free smoke tests pass.
- Play Console declarations match actual SDK and manifest behavior.

## Exact Follow Order

Use this order if you want the safest path:

1. P0: audit current project and fix level file/name mismatches.
2. P1: create level data and validator.
3. P2: create pure board/snake model.
4. P3: connect current scene to new model.
5. P4: split visuals out of current `GameManager` and `Arrow`.
6. P5: add session flow, timer, hearts, progression.
7. P6: build gameplay UI.
8. P7: finish camera, zoom, input.
9. P8: add special objects one by one.
10. P9: add gameplay boosters without ads/IAP.
11. P10: add main/challenge/rescue/seasonal mode structure.
12. P11: build level editor, generator, validator.
13. P12: add audio/haptics/polish.
14. P13: produce no-ads gameplay release candidate.
15. P14: scale content toward 300+ main levels.
16. P15: add ads/IAP/analytics only after gameplay is stable.
17. P16: release hardening.

## Immediate Next 10 Tasks

Start here:

1. Run a level audit for every JSON in `Assets/Resources/Levels`.
2. Fix mismatched file names and internal `Name` fields.
3. Decide whether current `Level_31` to `Level_35` should become `Level_1` to `Level_5` or move to a prototype folder.
4. Create `Assets/Scripts/ArrowMaze/Common`.
5. Create `Assets/Scripts/ArrowMaze/LevelContent`.
6. Add `GridCoord`, `GridDirection`, `LevelDefinition`, and `SnakeDefinition`.
7. Add `LevelJsonSerializer` that reads the current JSON schema.
8. Add `LevelValidator`.
9. Add an editor/test command that validates all current levels.
10. Only after validation passes, start extracting `BoardModel` and `MoveValidator`.

## Stop Conditions

Stop and fix the phase before moving forward if:

- level JSON names do not match file names,
- level loading order is unclear,
- old and new move validators disagree,
- board occupancy desyncs after a move,
- failed moves change board state,
- UI taps trigger board input,
- mode progress overwrites main progress,
- boosters can create impossible board states,
- copied competitor assets or level layouts appear in the project.

