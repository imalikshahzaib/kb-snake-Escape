# Movement Feedback Polish Implementation

Date: 2026-04-26

Source plan: `15_snake_movement_similarity_check.md`

## Implemented

### Stronger Blocked Feedback

Files:

- `Assets/Scripts/Arrow.cs`
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/CameraController.cs`

Behavior now:

- blocked snake bumps forward toward the blocker,
- blocked snake briefly holds at impact,
- blocked snake eases back to its original path position,
- selected snake flashes red during the fail,
- blocker still flashes red,
- collision sound plays on every blocked tap,
- heart loss remains limited by the current `hasFailed` rule,
- input remains locked until the return animation completes,
- camera shake plays for blocked movement,
- optional mobile haptic hook plays on supported mobile builds.

### Escape Polish

Behavior now:

- valid snake gets a small scale punch when movement starts,
- escape triggers a small feedback particle through the existing particle prefab,
- escape triggers lighter camera shake,
- optional short mobile haptic hook runs on supported mobile builds,
- forced/directional escape also gets the escape feedback hook.

### Camera Shake

Added to `CameraController`:

- `Shake(float duration, float strength)`

This is integrated into the existing camera smoothing so movement feedback does not require a new scene object.

### Haptic Hook

Added to `GameManager`:

- `enableHaptics`
- Android one-shot vibration path for supported Android versions,
- iOS fallback through `Handheld.Vibrate`,
- mobile-only guard so editor/desktop play is unaffected.

### MoveValidator Smoke Tests

Added Unity editor menu:

```text
ArrowMaze/Run MoveValidator Smoke Tests
```

File:

- `Assets/Scripts/ArrowMaze/Editor/MoveValidatorTestMenu.cs`

It validates:

- clear route can escape,
- blocked route reports `Blocked`,
- blocker identity is detected,
- escaped snake is removed from occupancy.

## Unity Setup Needed

No new prefab is required.

Optional inspector tuning:

- On `GameManager`:
  - `Enable Camera Shake`
  - `Enable Haptics`
  - `Blocked Shake Duration`
  - `Blocked Shake Strength`
  - `Escape Shake Duration`
  - `Escape Shake Strength`
- On arrow prefab:
  - `Blocked Bump Cells`
  - `Blocked Bump Duration`
  - `Blocked Hold Duration`
  - `Escape Scale Punch`
  - `Escape Scale Punch Duration`

Defaults are already set in code.

## Unity Test Checklist

1. Open Unity and wait for compile.
2. Run `ArrowMaze/Run MoveValidator Smoke Tests`.
3. Run `ArrowMaze/Validate Gameplay Model`.
4. Play Levels 1-5.
5. Tap blocked arrows:
   - selected snake should bump forward,
   - selected snake should turn red,
   - blocker should flash,
   - camera should shake lightly,
   - input should stay locked until return completes.
6. Tap valid arrows:
   - snake should punch slightly,
   - move smoothly,
   - trigger escape feedback near exit,
   - clear from board without desync.
7. Complete a level and confirm win flow still appears.

## Score Impact

Pending Unity validation:

- movement rules: still about `75/100`
- movement feel: raised from about `45/100` to about `55/100`
- overall movement similarity: raised from about `55/100` to about `62/100`

After Unity confirms this feels good on device:

- movement feel can be rated about `60/100`
- overall movement similarity can be rated `65-70/100`

## Still Missing

The movement is still not fully competitor-like until these are added:

- real pooled hit/escape effects instead of reusing click particle,
- dedicated short audio clips for tap, snake move, fail, escape,
- tuned mobile haptic plugin/service,
- static walls/barriers,
- portals,
- black holes,
- way blockers,
- large-board zoom/camera validation.
