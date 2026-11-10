# ArrowMaze Current Game Deep Audit And Implementation Start

Date: 2026-04-26

Target project: `C:\Users\Muhammad Ali\ArrowMaze`

Goal: rebuild ArrowMaze toward competitor-like functionality using clean-room implementation. This means behavior, systems, flow, and structure can be replicated, but competitor assets, levels, identifiers, copy, and service credentials must not be copied.

## Current Project Snapshot

### Unity Project

- Unity version: `6000.0.58f2`
- Main project scenes found:
  - `Assets/Scenes/GameScene.unity`
  - `Assets/Scenes/LevelEditor.unity`
  - `Assets/Scenes/SampleScene.unity`
- Gameplay scripts are currently prototype-style and mostly flat:
  - `Assets/Scripts/GameManager.cs`
  - `Assets/Scripts/Arrow.cs`
  - `Assets/Scripts/ArrowSegment.cs`
  - `Assets/Scripts/CameraController.cs`
  - `Assets/Scripts/LevelImporter.cs`
  - `Assets/Scripts/LevelMaker.cs`
  - `Assets/Scripts/SettingsUI.cs`
  - `Assets/Scripts/SoundManager.cs`
  - `Assets/Scripts/Data/LevelData.cs`

### Package State

Useful packages already present:

- Unity Input System
- URP
- UGUI
- Unity Test Framework

Packages not required for the first gameplay phases:

- Ads SDKs
- IAP SDKs
- Firebase
- Remote Config
- Addressables

Recommendation: do not add monetization/live-ops SDKs yet. Core gameplay is not modular enough, and adding SDKs now would increase debugging noise.

## Current Gameplay Implementation

### What Already Works As A Prototype

- JSON level loading through `Resources.LoadAll<TextAsset>("Levels")`.
- Basic level ordering by numeric `Level_` file name.
- Main menu with play button.
- Grid-centered world conversion.
- Arrow/snake prefab spawning.
- Arrow body segment spawning.
- Tap on arrow head/body through `OnMouseDown`.
- Directional movement.
- Collision detection against `GameManager.gridContent`.
- Failed collision feedback through red flash/return.
- Hearts/lives.
- Game over panel.
- Level complete panel.
- Next-level unlock through `PlayerPrefs` key `CurrentLevelIndex`.
- Camera pan/zoom.
- Simple settings panel.
- Level maker/editor scene support.
- Hint-like function that flashes a currently solvable arrow.
- Directional/erase power-up prototype functions.

### Main Gaps Against Competitor-Like Target

- No pure gameplay model yet.
- `GameManager` owns too many systems:
  - level loading,
  - board state,
  - occupancy,
  - UI state,
  - progression,
  - win/fail,
  - grid visuals,
  - boosters.
- `Arrow` owns too many systems:
  - input handling,
  - movement validation,
  - movement animation,
  - collision reaction,
  - body segment visuals,
  - erase behavior.
- Gameplay decisions still depend on scene objects and `MonoBehaviour` timing.
- No formal level validator.
- No formal board model.
- No formal move validator.
- No special object system.
- No timer system.
- No mode framework.
- No challenge/rescue/seasonal progression.
- No level authoring validation gate.
- No automated tests yet.

## Current Level Data Audit

Folder checked: `Assets/Resources/Levels`

Files found: `25`

Initial audit found 9 file/name mismatches. That has now been repaired:

- early levels from old `Level_31` to `Level_35` now live at `Level_1` to `Level_5`,
- late levels from old `Level_1` to `Level_4` now live at `Level_22` to `Level_25`,
- `.meta` files were moved with their matching JSON files.

Current shell preflight result after repair:

- file/name mismatches: `0`,
- basic path/bounds errors: `0`,
- every level has at least one legal first move.

## Side Effects To Avoid

### Level Order Side Effects

Do not blindly rename files without deciding intended progression. The current folder looks like early levels and late levels were mixed.

Safe rule:

- first decide true level order,
- then rename files and internal `Name` fields together,
- then validate,
- then playtest levels 1-10.

### Runtime Refactor Side Effects

Do not immediately replace `GameManager` or `Arrow`. They are still the working prototype reference.

Safe rule:

- build pure model beside current code,
- compare old and new move results,
- only then route runtime through new systems.

### Visual Side Effects

Do not change sprite/prefab setup while movement logic is being extracted.

Safe rule:

- gameplay model first,
- visual split second,
- atlas/sprite polish later.

### Monetization Side Effects

Do not add ads/IAP now.

Safe rule:

- no ads, no IAP, no analytics SDKs until gameplay loop, modes, and validation are stable.

## Current Competitor Parity Rating

This rating is for the current implemented project, not the plan.

Current implementation after this pass: `40/100`, pending Unity import/compile and scene playtest.

Reason:

- Basic tap-to-escape prototype exists.
- Basic JSON level flow exists.
- Basic lives/win/fail exists.
- Level order/name mismatches are repaired.
- Pure level/domain and board/snake model foundations now exist.
- But the competitor-like system still requires runtime integration, automated Unity tests, mode framework, special objects, boosters, content scale, UI flow, polish, and later live-ops.

Plan readiness remains much higher, but implemented parity is still early.

## Implementation Started In This Pass

Added the first safe foundation for Phase P0/P1:

```text
Assets/Scripts/ArrowMaze/Common/
  GridCoord.cs
  GridDirection.cs
  Result.cs

Assets/Scripts/ArrowMaze/LevelContent/
  LevelMode.cs
  LevelDifficulty.cs
  LevelDefinition.cs
  SnakeDefinition.cs
  BoardObjectDefinition.cs
  LevelJsonSerializer.cs
  LevelValidationResult.cs
  LevelValidator.cs
  LevelCatalog.cs

Assets/Scripts/ArrowMaze/Editor/
  LevelValidationMenu.cs
```

Added the first safe foundation for Phase P2:

```text
Assets/Scripts/ArrowMaze/Gameplay/Board/
  BoardBounds.cs
  BoardCell.cs
  BoardOccupancy.cs
  BoardModel.cs
  BoardFactory.cs

Assets/Scripts/ArrowMaze/Gameplay/Snakes/
  SnakeModel.cs
  SnakeMoveCommand.cs
  SnakeMoveResult.cs
  MovePlanner.cs
  MoveValidator.cs
  SnakeEscapeService.cs
  CollisionResolver.cs
```

Added another Unity menu:

```text
ArrowMaze/Validate Gameplay Model
```

Added a low-risk runtime bridge:

- `GameManager` now exposes `validateGameplayModelOnLoad`.
- On level load, it builds a pure `BoardModel` from the same `LevelData`.
- It logs snake count and legal first move count.
- It does not replace old movement yet, so existing prototype behavior should remain intact.

New Unity menu:

```text
ArrowMaze/Validate Current Levels
```

What it does:

- reads all JSON files in `Assets/Resources/Levels`,
- parses current ArrowMaze JSON schema,
- validates file name vs internal level name,
- validates board size,
- validates snake path cells,
- validates path adjacency,
- validates overlaps,
- validates declared head position,
- validates at least one legal first move,
- writes a report to:

```text
Research/From APK/Codex/current_level_validation_report.md
```

## What To Implement Next

Next implementation slice should be:

1. Open Unity and let it import/compile the new scripts.
2. Run `ArrowMaze/Validate Current Levels`.
3. Run `ArrowMaze/Validate Gameplay Model`.
4. Confirm `GameScene` still plays levels 1-5 in correct order.
5. Add tests proving old movement and new movement agree.
6. Begin routing `GameManager.IsPathClear`/`IsPathBlocked` through `MoveValidator` behind a debug comparison toggle.

Do not add special objects before the basic board model and move validator are stable.
