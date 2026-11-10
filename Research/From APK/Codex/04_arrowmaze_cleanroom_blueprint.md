# ArrowMaze Clean-Room Blueprint

Goal: build a player-facing experience with similar mechanics and quality signals while keeping ArrowMaze original.

## Product Target

Build ArrowMaze as a grid escape puzzle where the player clears a board by selecting arrow/snakes/paths in the correct order. Match the competitor's functional pillars:

- fast tap-to-move puzzle loop,
- animated path movement,
- blocked-path feedback,
- timer/hearts pressure,
- boosters,
- level progression,
- difficulty modes,
- special object unlocks,
- optional rewarded ads,
- no-ads purchase,
- polished mobile UI.

Do not copy competitor code, art, sound, level files, product identity, or exact layouts.

## Core Systems To Build

### 1. Board Model

Create a data-driven board model:

```text
LevelData
- id
- mode
- boardWidth
- boardHeight
- timeLimitSeconds
- snakes[]
- blockers[]
- blackHoles[]
- portals[]
- barriers[]
- tutorialFlags[]
```

```text
SnakeData
- id
- colorId
- isSpecial
- nodes[]
- escapeDirection or inferred head direction
- rewardWeight / priority optional
```

```text
Node
- x
- y
```

Rules:

- Board cells know which snake segment/object occupies them.
- A snake can move only if its next route is clear or valid special object rules allow escape.
- Movement updates occupancy frame-by-frame or step-by-step.
- Clearing all snakes ends the level.

### 2. Movement And Collision

Implement:

- `CanMove(snake)` check.
- `GetTracePath(snake)` path generation from level nodes.
- `MoveAlongPath(snake)` animation.
- `MoveBackOrShake(snake)` fail response.
- `OnSnakeEscaped`.
- `OnSnakeBlocked`.
- `OnBoardCleared`.

Design notes:

- Snake body should animate sequentially along grid nodes.
- On blocked tap, show shake/color flash/haptic/sound.
- Use deterministic movement. Avoid physics as the source of truth.
- Keep path solver separate from visual animation.

### 3. Level Authoring

Use original levels only.

Recommended JSON schema:

```json
{
  "id": "main_001",
  "mode": "main",
  "size": {"x": 12, "y": 14},
  "timeLimit": 180,
  "snakes": [
    {
      "id": "s1",
      "nodes": [{"x": 1, "y": 2}, {"x": 2, "y": 2}],
      "color": 1,
      "special": false
    }
  ],
  "blockers": [],
  "blackHoles": [],
  "portals": []
}
```

Validation requirements:

- all nodes in range unless intentionally outside-board escape markers are supported,
- no invalid overlaps,
- all portal pairs complete,
- blockers/holes not on illegal cells,
- level has at least one valid opening move,
- level is solvable by internal solver or recorded author solution,
- target duration and move count are in desired range.

### 4. Modes And Progression

Build modes inspired by the competitor's structure, but use original names and theming:

- Main mode:
  - large progression map/list,
  - difficulty variants: easy/medium/hard or ArrowMaze-specific names.
- Challenge mode:
  - chapter-based progression,
  - 10 levels per chapter,
  - unlock later chapters by main level progress.
- Rescue/special mode:
  - separate objective or special piece rules,
  - move count displayed.
- Seasonal mode:
  - limited pack, around 24 levels,
  - unique original visual theme.

Suggested unlock gates:

- Hint: level 6.
- Path/ruler/grid booster: level 12.
- Special mode 1: level 25.
- Challenge: level 50.
- Later challenge chapter: level 75.

These gates are genre/product pacing references, not code or assets.

### 5. Special Objects

Implement in phases:

Phase 1:

- Normal snakes.
- Special/king snake.
- Static blockers/walls.
- Timer and hearts.

Phase 2:

- Timed way blocker:
  - position,
  - lock count/time,
  - visual countdown,
  - destruction/open effect.
- Black hole:
  - position,
  - consumes/escapes entering snake,
  - introduction panel.

Phase 3:

- Portals:
  - pair positions,
  - color ID,
  - linked movement transition.
- Barrier:
  - direction,
  - length,
  - breakable state.
- Direction changer/pipe:
  - redirects movement according to rule.

### 6. Boosters

Build boosters as command objects, not hardcoded UI branches:

```text
Booster
- type
- unlockLevel
- coinPrice
- rewardedAdLimitPerLevel
- Apply(levelState)
```

Recommended booster set:

- Hint:
  - unlock level 6,
  - price around 300 coins,
  - highlights one safe move.
- Path/ruler:
  - unlock level 12,
  - price around 900 coins,
  - previews escape route or blocking chain.
- Remove obstacle / free grid:
  - unlock after special objects appear,
  - price around 900 coins.
- Revive time:
  - rewarded ad from level 3,
  - adds time after timeout.
- Revive heart:
  - rewarded ad from level 3,
  - restores hearts.

### 7. Economy

Recommended economy loop:

- Win base reward: 40 coins.
- Rewarded ad double: 80 coins.
- Revive bundle: +3 hearts for 300 coins.
- Free small coin claim with daily cap.
- Coin packs, no-ads, starter bundle, and seasonal bundle.

Use your own product IDs and live store metadata.

Do not hardcode competitor prices as final business values. Treat them as market reference only.

### 8. Ads

Recommended ad pacing:

- No forced interstitials in first 25 levels.
- Begin banners around level 18 or later.
- Begin app-open ads no earlier than level 6.
- Interstitials:
  - levels 26-100: every 3 levels or 180 seconds.
  - level 101+: every 2 levels or 120 seconds.
- Lose interstitials only after the player has enough progression.
- Reward ads:
  - voluntary only,
  - tied to hint, path/ruler, revive, double coins.

No-ads purchase behavior:

- removes forced interstitials,
- removes banners,
- leaves rewarded ads available.

### 9. UI Screens

Build these screens:

- Loading/Init.
- Home.
- Mode selection.
- Main gameplay HUD.
- Pause/settings.
- Level complete.
- Level failed.
- Timeout.
- Revive.
- Shop.
- Remove ads.
- Bundle/starter pack.
- Booster purchase.
- Booster unlock.
- New object intro.
- Challenge chapter panel.
- Seasonal mode panel.
- Internet/offline popup.
- Review/rate prompt.
- Language/settings.

Gameplay HUD should include:

- level/mode label,
- timer,
- remaining/escaped count,
- hearts,
- coin view,
- pause,
- hint booster,
- path/ruler booster,
- zoom controls or pinch zoom,
- transient feedback text.

### 10. Audio And Haptics

Create original audio for:

- UI click,
- snake tap,
- snake movement,
- failed tap,
- obstacle break,
- escape/swish,
- coin collect,
- timeout ticking,
- win popup,
- lose popup,
- revive popup,
- confetti/success.

Haptic presets:

- light tap: 0.04s
- medium success: 0.06s
- stronger fail/break: 0.08s
- short popup emphasis: 0.05s

### 11. Visual Asset Checklist

Create original assets for:

- snake head/body/tail/tail-joint for each palette color,
- special snake variant,
- board tiles,
- barriers/walls,
- black hole,
- portals,
- pipe/direction changer,
- warning lines,
- booster icons,
- heart icons,
- coin icons,
- shop pack icons,
- home mode cards,
- challenge/rescue/seasonal panels,
- win/lose/revive panels,
- confetti, sparkle, glow, touch effect.

The competitor has high asset volume. ArrowMaze should prioritize a smaller but coherent original set first, then expand.

## Implementation Order

1. Build board data model and level loader.
2. Build snake rendering from node paths.
3. Build movement validation and animated path movement.
4. Build clear/fail/win/loss states.
5. Build 20 original test levels.
6. Add timer, hearts, and progress counter.
7. Add home/game/endgame UI.
8. Add hint booster.
9. Add path/ruler booster.
10. Add revive and coin economy.
11. Add blockers, black holes, portals one by one.
12. Add challenge and seasonal mode shells.
13. Add ads/IAP behind interfaces.
14. Add analytics events.
15. Build level authoring/validation tools.
16. Expand original level packs.

## Quality Gates

Before claiming parity:

- 50+ original levels playable without softlocks.
- Every level can be completed from cold start.
- Failed tap feedback is immediate and readable.
- No overlapping UI on 1080x2160 and common aspect ratios.
- Timer/hearts/revive states persist correctly across pause/resume.
- Rewarded ad callbacks cannot grant duplicate rewards.
- No-ads removes forced ads only.
- Boosters handle edge cases and cannot corrupt board state.
- Special object tutorials appear once and can be skipped.
- Level completion and failure analytics are reliable.
- App works offline for local levels.

## Clone-Risk Checklist

Avoid:

- copied level JSON paths,
- copied texture/audio/spine/mesh files,
- copied package name or app title,
- copied exact store product IDs,
- copied ad/Facebook/Firebase/Appsflyer IDs,
- copied UI screenshots/layouts pixel-for-pixel,
- copied localized copy,
- copied class names in your public API if avoidable.

Allowed clean-room inspiration:

- broad game mechanic of path-based grid escape,
- concept of boosters,
- concept of hearts/timer/revives,
- ad pacing principles,
- mode/chapter progression pattern,
- original implementation of similar object categories.
