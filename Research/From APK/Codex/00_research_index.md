# Snake Escape Competitor Research Index

Source analyzed: `C:\Users\Muhammad Ali\Downloads\snakeescape`

Target folder: `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Codex`

## Important Boundary

This research is for learning and product reference. Do not copy proprietary code, APK package identity, ad IDs, SDK tokens, textures, audio, Spine data, meshes, exact levels, or branding. Use this as a clean-room specification for an original ArrowMaze implementation with similar player-facing mechanics, pacing, and monetization structure.

## Files Created

- `01_competitor_snapshot.md`
  - Engine/build facts, Android manifest facts, Unity scenes, Addressables, plugin/DLL inventory, SDKs, and source evidence.
- `02_gameplay_level_design.md`
  - Core game loop, objects, board/level formats, level pack counts, plaintext level statistics, boosters, lives, timers, difficulty/mode structure.
- `03_ui_ux_monetization.md`
  - Home/gameplay/endgame/shop flows, unlocks, IAP price text found in prefabs, ad tuning, audio/haptics/visual style.
- `04_arrowmaze_cleanroom_blueprint.md`
  - Original implementation plan for ArrowMaze based on the research, including systems to build and clone-risk items to avoid.
- `05_hidden_secrets_dark_truths.md`
  - Hidden/high-risk findings from configs, manifest, live-ops database strings, ads, tracking, notification hooks, CI leftovers, and clean-room warnings.
- `06_deeper_hidden_secrets_solid_plan.md`
  - Second-pass missed findings and a concrete ArrowMaze plan covering Android ad services, AppsFlyer backup rules, AppLovin consent, Unity production services, manifest surface area, analytics events, remote-level/hotfix clues, failure/win monetization, debug/protection artifacts, and release hygiene.
- `07_gap_check_hidden_secrets_addendum.md`
  - Third-pass gap check covering missing Android backup behavior, Facebook auto tracking, Play Integrity/reCAPTCHA/PaiRIP licensing clues, split-APK dependency, ad WebView/playable-ad runtime, default input baggage, runtime auto-initialization bloat, localization/package mismatch, billing test hooks, and SafeDK application-level wrapping.
- `08_full_implementation_plan.md`
  - Production-ready clean-room implementation plan for ArrowMaze covering functional scope, system architecture, module/script structure, prototype migration, development phases, performance optimization, testing, validation, and release readiness gates.
- `09_project_implementation_phases_steps.md`
  - Followable project implementation checklist for applying the research inside the current ArrowMaze Unity project, including current project state, level-count targets, phase gates, module order, content scale targets, and immediate next tasks.
- `10_current_game_deep_audit_and_implementation_start.md`
  - Current ArrowMaze project audit and first implementation-start report, covering prototype scripts, current level-data issues, side-effect risks, current parity rating, and the new validation/domain foundation scripts added to the project.
- `current_level_validation_report.md`
  - Current JSON level preflight report showing 25 checked level files, repaired file/name ordering, no basic path/bounds failures, and current legal-first-move counts.
- `gameplay_model_validation_report.md`
  - Current pure gameplay model preflight report showing all 25 current levels pass first-move model assumptions and can move into `BoardModel`/`BoardOccupancy` integration.
- `11_parity_scorecard.md`
  - Live parity scorecard for tracking implementation progress toward competitor-like clean-room functionality, with current score, phase status, milestones, and next target.
- `12_current_project_vs_competitor_rating.md`
  - File-based current project comparison against the competitor research, including implemented parity score, visual/UX likeness score, category ratings, evidence, clean-room asset warning, and next actions.
- `13_movevalidator_safe_integration_report.md`
  - Safe integration report for routing real player taps through the new `MoveValidator` comparison layer while the old prototype movement remains the runtime authority.
- `14_movevalidator_runtime_authority_switch.md`
  - Runtime authority switch report showing that real taps now use `MoveValidator` and `SnakeEscapeService`, with old path-clear movement decisions removed from the active tap flow.
- `15_snake_movement_similarity_check.md`
  - Current comparison of ArrowMaze snake movement against the competitor movement target, including logic similarity, visual feel gaps, and next fixes.
- `16_movement_feedback_polish_implementation.md`
  - Implementation report for movement-feel improvements: blocked bump/return, escape feedback, camera shake, haptic hooks, and MoveValidator smoke tests.
- `17_cleanroom_level_generation_implementation.md`
  - Implementation report for original level generation, JSON export, solvability checking, difficulty estimation, Unity generation menus, and level-pack audit tooling.
- `18_static_wall_blocker_implementation.md`
  - Implementation report for the first special-object slice: static walls/barriers in data, model occupancy, `MoveValidator`, validation, scene feedback, generator support, and smoke tests.
- `19_wayblocker_countdown_implementation.md`
  - Implementation report for competitor-like count-locked `WayBlocker` rules, JSON import, model countdown, occupancy removal, runtime text/visual removal, generator support, and smoke tests.

## Highest-Value Findings

- Competitor is a Unity Android game exported from Unity `6000.0.59f2`.
- APK identity is `com.snake.escape.maze.slither.puzzle`, version name `1.8.0`, Android version code `186`, target SDK `36`, min SDK `23`.
- Build info says version `1.8.0`, build `18`, built on `2026-03-18 05:25:13 +0000`.
- Unity build scenes are `InitScene` and `MainScene`; `LoadingScene` also exists in Addressables.
- Main gameplay is a 2D grid snake/arrow escape puzzle:
  - tap/select snakes/arrows,
  - snake follows a stored path of grid nodes,
  - escape succeeds when path out is clear,
  - collision/blocked path triggers failure feedback,
  - objective is clearing board before timer/hearts run out.
- Level packs:
  - Main saga: `1451` numeric files (`lv0` to `lv1450`) plus `11` named/special files.
  - Mode 2: `80` numeric files (`lv1` to `lv80`).
  - Mode 3: `100` numeric files plus `lvportal`.
  - Xmas/Winter mode: `24` numeric files.
- Plaintext level packs use fields: `size`, `timeLimit`, `name`, `arrows`, `k`, `wayBlockers`, `blackHoles`, `portals`.
- Common level timer is `180` seconds.
- Special objects include normal snakes/arrows, king/special snakes, barriers, way blockers, black holes, portals, pipes/direction changers, grids, and warning lines.
- Boosters and lives are monetized:
  - Hint unlock level `6`.
  - Ruler/grid booster unlock level `12`.
  - Revive ad rewards unlock level `3`.
  - Booster prices found: `300`, `900`, `900` coins.
- Ads are delayed in early levels:
  - interstitials disabled through levels `0-25`,
  - levels `26-100`: `180s` cooldown and `3` level cooldown,
  - levels `101+`: `120s` cooldown and `2` level cooldown, including lose interstitials.
- Hidden configs show a broad live-ops and monetization shell:
  - Firebase Remote Config/Firestore/AB testing/hotfix strings,
  - many ad networks and attribution SDKs,
  - four daily notification time bands,
  - public client IDs and CI/build leftovers that must not be reused.
- Deeper second-pass findings:
  - Android ad services config allows attribution/custom audience/topics access.
  - AppsFlyer purchase/attribution data is excluded from backup/device transfer.
  - AppLovin consent flow is embedded and enabled.
  - Unity Services is set to production.
  - Runtime metadata exposes ad revenue, AppsFlyer/Facebook/Firebase/Adjust-style event names, remote-level/hotfix classes, and SRDebugger/runtime debug artifacts.
- Third-pass gap-check findings:
  - No camera, microphone, location, contacts, or storage permissions were found; the privacy risk is ad/analytics/attribution-heavy rather than hardware-spying-heavy.
  - Facebook automatic app event logging and advertiser ID collection are enabled in manifest metadata.
  - Android backup is enabled globally, while the explicit backup exclusions target AppsFlyer attribution/purchase data.
  - Play Integrity, Firebase Auth reCAPTCHA, PaiRIP license-check components, Play split metadata, and Play Store stamp data show a production Play-distribution/licensing layer.
  - A WebView/playable-ad runtime is shipped through MRAID/OMID/endcard scripts, ad activities, `audience_network.dex`, and `IPlayTempleView` reporting hooks.
  - `RuntimeInitializeOnLoads.json` has `134` startup initialization entries across `31` assemblies; `ScriptingAssemblies.json` lists `228` assemblies.
- Shop text found in prefabs includes coin packs, no-ads, no-ads bundle, starter pack, special deals, Santa gift, and video bar.
- The full implementation plan maps the current flat ArrowMaze prototype scripts into production modules under `Assets/Scripts/ArrowMaze`, including gameplay model, presentation, UI, economy, monetization, services, tooling, and tests.

## Recommended Use

Use reports `02` and `04` as the source of truth for building ArrowMaze parity. Keep competitor assets out of the repo unless you have explicit rights. Create original levels, art, UI copy, sounds, product IDs, ad unit IDs, and package identifiers.
