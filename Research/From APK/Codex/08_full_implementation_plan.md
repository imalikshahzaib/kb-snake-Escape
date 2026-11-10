# ArrowMaze Full Implementation Plan

Source research folder: `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Codex`

Target project: `C:\Users\Muhammad Ali\ArrowMaze`

## Clean-Room Boundary

Build functional parity, not a copied product. ArrowMaze can reproduce the logic, feature categories, progression flow, and monetization behavior with original code, original levels, original UI copy, original assets, original package identity, and original service IDs.

Do not copy competitor assets, exact level files, exact UI layouts, audio, Spine files, package name, product IDs, Firebase/Facebook/ad IDs, CI files, endpoints, notification copy, or brand identity.

## Product Target

ArrowMaze should become a polished portrait mobile grid escape puzzle with the same functional pillars:

- tap/select a path-based arrow/snake,
- validate whether it can escape,
- animate along its stored path,
- fail with immediate feedback when blocked,
- clear the board before timer/hearts run out,
- introduce special objects gradually,
- support main, challenge, rescue/special, and seasonal/event modes,
- use boosters, coins, revives, reward doubling, shop bundles, optional rewarded ads, no-forced-ads purchase, analytics, remote config, notifications, and release hygiene.

The current ArrowMaze scripts are a useful prototype. The production build should migrate away from a single `GameManager`/`Arrow` heavy model into explicit modules.

## Functional Scope

### Core Gameplay

Required behavior:

- Board is generated from level data.
- Level data defines board size, time limit, arrows/snakes, special objects, colors, and mode metadata.
- Each snake/arrow is an ordered list of grid nodes.
- Player taps a snake/arrow or any body segment.
- Input is ignored while the board is animating or a modal is open.
- Game checks the escape route deterministically, using grid occupancy as the source of truth.
- If route is clear, snake moves along path and exits the board.
- If blocked, selected snake bumps/shakes, blocker flashes, haptic and sound play, and a heart may be lost if the level rules require it.
- Board progress updates after each escape.
- Level completes when all required snakes/objectives are cleared.
- Level fails on timeout, out of hearts, or mode-specific failure rule.

Core loop:

1. `Boot`
2. `LoadProgress`
3. `Home`
4. `SelectMode`
5. `LoadLevel`
6. `SpawnBoard`
7. `Play`
8. `TapSnake`
9. `CanMove`
10. `MoveOrFail`
11. `CompleteOrContinue`
12. `WinReward`, `FailRevive`, or `Replay`

### Gameplay Objects

Implement these object categories:

- `Snake`: normal escape unit.
- `KingSnake`: special/priority snake with distinct rules/visual treatment.
- `GridCell`: board coordinate and occupancy record.
- `StaticWall`: permanent blocker.
- `Barrier`: directional/length obstacle, optionally breakable.
- `WayBlocker`: timed or count-locked blocker with unlock/destruction feedback.
- `BlackHole`: sink/escape target that consumes valid entering snakes.
- `Portal`: paired endpoints with color/group id and teleport transition.
- `Pipe`: route continuation or entry/exit modifier.
- `DirectionChanger`: redirects movement according to configured direction.
- `WarningLine`: readable danger/blocked/escape hint overlay.
- `TouchEffect`, `ComboText`, `Sparkle`, `Confetti`: feedback-only presentation.

Rules must live in model/services first, then visuals follow. Do not make physics colliders the gameplay source of truth.

### Modes

Implement mode framework from day one, even if only main mode ships first:

- `MainSaga`
  - large linear progression,
  - difficulty tags: normal, hard, super hard,
  - object introductions,
  - primary unlock source.
- `Challenge`
  - chapter-based progression,
  - 10 levels per chapter,
  - chapter unlocks from main progression,
  - optional move count display.
- `Rescue`
  - separate mode shell and endgame panels,
  - can initially reuse core clear-board objective,
  - later can add a rescued target rule.
- `Seasonal`
  - event pack, about 24 levels per event,
  - isolated progression,
  - feature flag controlled.

Recommended default gates, kept configurable:

- Revive reward: level 3.
- Hint booster: level 6.
- Path/ruler/grid booster: level 12.
- Banner ads: level 18 or later.
- Special/event edge mode: level 25.
- Challenge mode: level 50.
- Later challenge chapter: level 75.
- No forced interstitial ads before level 26.

### UI And UX Flow

Screens/panels required:

- `BootLoadingScreen`
- `HomeScreen`
- `BottomNav`
- `ModeSelectPanel`
- `GameHud`
- `PausePanel`
- `SettingsPanel`
- `LanguagePanel`
- `LevelCompletePanel`
- `LevelFailedPanel`
- `TimeoutPanel`
- `RevivePanel`
- `ShopPanel`
- `RemoveAdsPanel`
- `StarterPackPanel`
- `BundleOfferPanel`
- `PurchaseBoosterPanel`
- `UnlockBoosterPanel`
- `DailyVideoPanel`
- `NewObjectIntroPanel`
- `TutorialPanel`
- `PinchZoomTutorialPanel`
- `ChallengeChapterPanel`
- `RescueModePanel`
- `SeasonalModePanel`
- `ReviewPromptPanel`
- `OfflinePopup`
- `QuickMessagePanel`

Gameplay HUD must show:

- level/mode label,
- timer,
- remaining snakes/progress,
- hearts,
- coins,
- pause,
- hint booster,
- path/ruler/grid booster,
- zoom control or pinch zoom,
- transient combo/success/fail text.

UX behavior:

- Home is the first useful screen, not a marketing page.
- Bottom tabs keep Home, Shop, and Challenge reachable.
- New mechanics show a concise intro once.
- Booster unlock panels appear at first eligible level.
- Win panel offers normal claim and optional x2 reward.
- Failure panel offers replay, revive with ad, revive with coins, and exit.
- No-ads copy must clearly say it removes forced ads and banners while keeping optional reward ads.
- Review prompts appear only after positive sessions, never after a loss or forced ad.

### Economy

Implement a token-based economy:

- `Coin`
- `HintToken`
- `PathPreviewToken`
- `ReviveToken`
- `TimeToken`
- `RemoveForcedAdsEntitlement`
- optional future event tokens

Default economy tuning:

- Level win reward: 40 coins.
- Rewarded double claim: 80 total coins.
- Hint coin price: about 300.
- Path/ruler/grid price: about 900.
- +3 hearts or large revive option: about 300.
- Free small coin claim with daily cap and cooldown.

Launch SKU set should be smaller than the competitor:

- Remove forced ads.
- Starter pack.
- Small coin pack.
- Medium coin pack.
- Optional no-ads bundle after base purchase restore works.

All product IDs must be ArrowMaze-owned and environment-specific.

### Ads

Build ads behind interfaces so gameplay never depends on a specific SDK.

Ad surfaces:

- Banner
- Interstitial
- App open
- Rewarded hint
- Rewarded path/ruler
- Rewarded revive hearts
- Rewarded revive time
- Rewarded double coins
- Daily video reward

Default forced-ad curve:

- Levels 0-25: no forced interstitials.
- Levels 26-100: interstitial cooldown 180 seconds and 3 completed levels.
- Levels 101+: interstitial cooldown 120 seconds and 2 completed levels.
- Lose interstitials disabled until late progression.
- App-open ads no earlier than level 6, with cooldown.
- Preloading can start after early onboarding, but must respect memory gates.

No-ads purchase:

- disables banners,
- disables forced interstitials,
- disables forced app-open ads if used,
- does not remove optional rewarded ads.

Compliance:

- Consent must be initialized before ad SDKs.
- Keep Android ad services permissions out until a provider requires them.
- Keep manifest queries minimal.
- Treat playable ads as untrusted third-party WebView content.

### Analytics And Live Ops

Use one internal analytics API and one backend at launch.

Required event names:

- `app_start`
- `session_start`
- `home_open`
- `mode_open`
- `level_start`
- `level_complete`
- `level_fail`
- `snake_tap`
- `snake_blocked`
- `snake_escape`
- `booster_unlock`
- `booster_used`
- `booster_purchase`
- `coins_earned`
- `coins_spent`
- `ad_request`
- `ad_impression`
- `ad_reward_granted`
- `ad_failed`
- `iap_offer_view`
- `iap_purchase_start`
- `iap_purchase_success`
- `iap_purchase_fail`
- `review_prompt_show`
- `notification_schedule`
- `notification_open`

Remote config keys:

- feature flags per mode,
- booster unlock levels,
- ad cooldowns,
- ad placement enabled flags,
- daily reward values,
- review prompt gates,
- notification flags,
- level pack version,
- emergency disable flags.

Remote level packs should come after local content is stable:

- local fallback pack,
- remote catalog version,
- content hash,
- schema version,
- min app version,
- rollback version,
- validation before activation.

### Notifications

Use opt-in notifications only:

- daily reward ready,
- event ending,
- new level pack available.

Avoid medical, disease, pressure, and misleading difficulty claims. Test denied permission, Android 13+ permission flow, reboot, timezone change, and opt-out.

## System Architecture

### Layering

Use this dependency direction:

```text
App/Bootstrap
  -> Services interfaces
  -> Feature modules
  -> Domain model

Presentation/UI
  -> Gameplay controllers
  -> Domain model

Domain model
  -> no Unity scene objects
  -> no ad/IAP/Firebase dependencies
```

Keep pure gameplay logic testable without loading a Unity scene.

### Runtime State Machines

`AppState`:

- `Booting`
- `LoadingLocalConfig`
- `InitializingServices`
- `LoadingProgress`
- `Home`
- `Gameplay`
- `Shop`
- `Backgrounded`

`LevelState`:

- `None`
- `Loading`
- `Spawning`
- `Intro`
- `Playing`
- `ResolvingMove`
- `Paused`
- `Complete`
- `Failed`
- `Reviving`
- `Exiting`

`AdState`:

- `Unavailable`
- `Loading`
- `Ready`
- `Showing`
- `Completed`
- `Failed`
- `Cooldown`

### Data Flow

Level load:

```text
LevelCatalog -> LevelProvider -> LevelJsonSerializer -> LevelValidator
-> LevelDefinition -> LevelSession -> BoardModel -> BoardView
```

Tap flow:

```text
InputRouter -> SnakeSelectionService -> MoveValidator
-> MoveCommand -> MovementAnimator -> BoardOccupancy update
-> ObjectiveTracker -> UI/Analytics/Economy
```

Rewarded ad flow:

```text
UI Button -> RewardRequest -> AdService.ShowRewarded
-> RewardGrantService -> Economy/Booster/Revive
-> Analytics -> UI refresh
```

Purchase flow:

```text
Shop UI -> PurchaseService -> Store Provider
-> ReceiptValidator/PurchaseRestore -> EntitlementService
-> EconomyWallet -> SaveService -> UI refresh
```

## Project Structure: Modules And Scripts Only

Recommended script tree:

```text
Assets/Scripts/ArrowMaze/
  App/
    AppBootstrap.cs
    AppStateMachine.cs
    AppConfig.cs
    ServiceRegistry.cs
    SceneFlowController.cs
    BuildInfoProvider.cs

  Common/
    GridCoord.cs
    GridDirection.cs
    Result.cs
    Optional.cs
    TimeProvider.cs
    DisposableGroup.cs
    EnumExtensions.cs

  Events/
    GameEventBus.cs
    IGameEvent.cs
    LevelEvents.cs
    EconomyEvents.cs
    AdEvents.cs
    UiEvents.cs

  LevelContent/
    LevelDefinition.cs
    LevelMode.cs
    LevelDifficulty.cs
    LevelObjectiveDefinition.cs
    SnakeDefinition.cs
    BoardObjectDefinition.cs
    PortalDefinition.cs
    WayBlockerDefinition.cs
    BarrierDefinition.cs
    LevelPackDefinition.cs
    LevelCatalog.cs
    ILevelProvider.cs
    ResourcesLevelProvider.cs
    AddressablesLevelProvider.cs
    RemoteLevelProvider.cs
    LevelJsonSerializer.cs
    LegacyLevelImporter.cs
    LevelSchemaVersion.cs
    LevelValidationResult.cs
    LevelValidator.cs
    LevelSolvabilityChecker.cs
    DifficultyEstimator.cs

  Gameplay/
    Session/
      LevelSessionController.cs
      LevelStateMachine.cs
      LevelTimer.cs
      HeartManager.cs
      ObjectiveTracker.cs
      LevelProgressionService.cs
      FailReason.cs
      LevelResult.cs

    Board/
      BoardModel.cs
      BoardCell.cs
      BoardOccupancy.cs
      BoardBounds.cs
      BoardFactory.cs
      BoardQueryService.cs
      BoardMutationService.cs

    Snakes/
      SnakeModel.cs
      SnakeSegmentModel.cs
      SnakeFactory.cs
      SnakePath.cs
      SnakePathSampler.cs
      SnakeSelectionService.cs
      SnakeEscapeService.cs
      SnakeMoveCommand.cs
      SnakeMoveResult.cs
      MoveValidator.cs
      MovePlanner.cs
      CollisionResolver.cs
      KingSnakeRule.cs

    Objects/
      BoardObjectModel.cs
      StaticWallRule.cs
      BarrierRule.cs
      WayBlockerRule.cs
      BlackHoleRule.cs
      PortalRule.cs
      PipeRule.cs
      DirectionChangerRule.cs
      WarningLineRule.cs
      ObjectRuleRegistry.cs

    Boosters/
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

  Presentation/
    Board/
      BoardView.cs
      GridView.cs
      GridCellView.cs
      BoardObjectViewFactory.cs
      BoardFitService.cs

    Snakes/
      SnakeView.cs
      SnakeBodyView.cs
      SnakeSegmentView.cs
      SnakeMovementAnimator.cs
      SnakeSpawnAnimator.cs
      SnakeFailAnimator.cs
      SnakeColorPalette.cs
      SnakeViewPool.cs

    Objects/
      BarrierView.cs
      WayBlockerView.cs
      BlackHoleView.cs
      PortalView.cs
      PipeView.cs
      DirectionChangerView.cs
      WarningLineView.cs

    Effects/
      EffectPool.cs
      TouchEffectPlayer.cs
      ComboTextPlayer.cs
      ConfettiPlayer.cs
      CameraShakeService.cs
      HapticService.cs

    Camera/
      GameplayCameraController.cs
      PinchZoomController.cs
      ZoomBarController.cs

    Input/
      InputRouter.cs
      TouchInputReader.cs
      MouseEditorInputReader.cs
      GestureMode.cs
      GestureStrategy.cs
      TapGestureStrategy.cs
      DragGestureStrategy.cs

  UI/
    Core/
      PanelSystem.cs
      PanelId.cs
      PanelBase.cs
      ModalStack.cs
      SafeAreaFitter.cs
      UiTransitionPlayer.cs
      ButtonSoundBinder.cs

    Screens/
      BootLoadingScreen.cs
      HomeScreen.cs
      BottomNavController.cs
      GameHud.cs
      PausePanel.cs
      SettingsPanel.cs
      LanguagePanel.cs
      LevelCompletePanel.cs
      LevelFailedPanel.cs
      TimeoutPanel.cs
      RevivePanel.cs
      ShopPanel.cs
      RemoveAdsPanel.cs
      StarterPackPanel.cs
      BundleOfferPanel.cs
      PurchaseBoosterPanel.cs
      UnlockBoosterPanel.cs
      DailyVideoPanel.cs
      NewObjectIntroPanel.cs
      TutorialPanel.cs
      ChallengeChapterPanel.cs
      RescueModePanel.cs
      SeasonalModePanel.cs
      ReviewPromptPanel.cs
      OfflinePopup.cs

    ViewModels/
      HomeViewModel.cs
      GameHudViewModel.cs
      ShopViewModel.cs
      LevelResultViewModel.cs
      BoosterViewModel.cs
      ModeCardViewModel.cs

  Economy/
    CurrencyType.cs
    Wallet.cs
    WalletService.cs
    RewardType.cs
    RewardGrantService.cs
    DailyFreeClaimService.cs
    EntitlementService.cs
    ProductCatalog.cs
    OfferDefinition.cs
    ShopOfferService.cs
    EconomyBalanceConfig.cs

  Monetization/
    Ads/
      IAdService.cs
      NullAdService.cs
      AdPlacement.cs
      AdCooldownService.cs
      AdEligibilityService.cs
      RewardedAdRequest.cs
      InterstitialPolicy.cs
      BannerPolicy.cs
      AppOpenAdPolicy.cs
      NoAdsPolicy.cs
      AdMemoryGate.cs
      AdAnalyticsReporter.cs

    IAP/
      IPurchaseService.cs
      NullPurchaseService.cs
      PurchaseProductId.cs
      PurchaseRequest.cs
      PurchaseResult.cs
      PurchaseRestoreService.cs
      ReceiptValidationService.cs
      StoreEnvironmentConfig.cs

    Consent/
      ConsentService.cs
      ConsentState.cs
      PrivacyPolicyLauncher.cs
      DataSafetyConfig.cs

  Services/
    Save/
      ISaveService.cs
      PlayerPrefsSaveService.cs
      SaveData.cs
      SaveMigrator.cs
      SaveIntegrityService.cs
      BackupPolicyNotes.cs

    Analytics/
      IAnalyticsService.cs
      NullAnalyticsService.cs
      AnalyticsEvent.cs
      AnalyticsEventBuilder.cs
      LevelAnalytics.cs
      EconomyAnalytics.cs
      AdAnalytics.cs

    RemoteConfig/
      IRemoteConfigService.cs
      LocalRemoteConfigService.cs
      RemoteConfigKeys.cs
      RemoteConfigDefaults.cs

    Notifications/
      INotificationService.cs
      LocalNotificationService.cs
      NotificationSchedule.cs
      NotificationPermissionService.cs

    Review/
      IReviewService.cs
      ReviewPromptGate.cs
      NullReviewService.cs

    Audio/
      AudioService.cs
      SoundId.cs
      SoundCatalog.cs
      MusicController.cs

    Localization/
      LocaleService.cs
      TextKey.cs
      TextTable.cs

  Tools/
    LevelEditor/
      LevelEditorWindow.cs
      LevelEditorSceneController.cs
      LevelPaintTool.cs
      LevelObjectPalette.cs
      LevelExportTool.cs

    BuildValidation/
      BuildValidationRunner.cs
      ManifestAudit.cs
      PackageAudit.cs
      ReleaseHygieneAudit.cs
      AddressablesAudit.cs

    Tests/
      TestLevelFactory.cs
      GameplayTestDriver.cs
      FakeAdService.cs
      FakePurchaseService.cs
      FakeAnalyticsService.cs
```

### Prototype Migration Map

Current prototype classes should be split like this:

- `GameManager`
  - `LevelSessionController`
  - `BoardFactory`
  - `BoardOccupancy`
  - `ObjectiveTracker`
  - `LevelProgressionService`
  - `PanelSystem` integration
- `Arrow`
  - `SnakeModel`
  - `SnakeView`
  - `SnakeMovementAnimator`
  - `SnakeSelectionService`
  - `MoveValidator`
- `ArrowSegment`
  - `SnakeSegmentView`
  - `InputRouter` hit target
- `LevelImporter`
  - `LegacyLevelImporter`
  - `LevelJsonSerializer`
  - `LevelValidator`
- `LevelMaker`
  - `LevelEditorWindow`
  - `LevelExportTool`
- `SoundManager`
  - `AudioService`
  - `SoundCatalog`
- `SettingsUI`
  - `SettingsPanel`
  - `LocaleService`
  - audio/haptic toggles in `SaveData`

Keep the old scripts working until the new module passes parity tests, then remove or adapt them.

## Development Phases

### Gameplay-Only Phase Plan

Use these phases if the target is only competitor-like gameplay functionality. This excludes ads, IAP, attribution, notifications, remote config, app-open ads, banners, store SDKs, Facebook, Firebase, and other live-ops systems.

#### Phase G0: Prototype Lock And Cleanup

Goal: preserve the current ArrowMaze prototype behavior before replacing it with modular gameplay systems.

Build:

- Keep current `GameManager`, `Arrow`, `ArrowSegment`, `LevelImporter`, `CameraController`, and `SoundManager` working as the reference prototype.
- Create `Assets/Scripts/ArrowMaze` module folders.
- Create a small original test pack:
  - 5 simple levels,
  - 5 blocked-path levels,
  - 5 larger levels,
  - 5 timer/heart pressure levels.
- Define the final ArrowMaze level schema.

Done when:

- Current game can still open, play, win, and fail.
- A baseline test pack exists.
- No competitor levels/assets are imported.

#### Phase G1: Pure Board And Level Data

Goal: make gameplay data-driven and testable without scene objects.

Build:

- `LevelDefinition`
- `SnakeDefinition`
- `BoardObjectDefinition`
- `GridCoord`
- `GridDirection`
- `BoardModel`
- `BoardCell`
- `BoardOccupancy`
- `LevelJsonSerializer`
- `LevelValidator`

Rules:

- Board size supports at least `25x30`, `30x30`, `35x35`, and `40x40`.
- Snake paths are ordered grid nodes.
- Occupancy knows every snake segment and every object cell.
- Level validator rejects invalid overlaps, out-of-bounds cells, missing paths, invalid directions, and incomplete portal pairs.

Done when:

- Levels load into a pure model.
- Validator catches bad level data.
- Board occupancy is correct before any rendering exists.

#### Phase G2: Snake Movement Core

Goal: reproduce the competitor's tap-to-escape logic.

Build:

- `SnakeModel`
- `SnakePath`
- `SnakePathSampler`
- `MoveValidator`
- `MovePlanner`
- `SnakeMoveCommand`
- `SnakeMoveResult`
- `CollisionResolver`
- `SnakeEscapeService`

Rules:

- Player can select a snake.
- The game checks ahead of the snake using board occupancy.
- If clear, the snake exits.
- If blocked, the move fails without changing board state.
- Escaped snakes are removed from occupancy.
- Movement rules are deterministic and do not depend on physics collisions.

Done when:

- Model tests prove clear path, blocked path, outside-board escape, and occupancy removal.
- The model can complete a simple level without Unity visuals.

#### Phase G3: Visual Board And Snake Rendering

Goal: connect the pure model to the scene.

Build:

- `BoardView`
- `GridView`
- `GridCellView`
- `SnakeView`
- `SnakeSegmentView`
- `SnakeViewPool`
- `SnakeSpawnAnimator`
- `SnakeMovementAnimator`
- `SnakeFailAnimator`
- `SnakeColorPalette`

Behavior:

- Level spawns a visible grid.
- Every snake path creates head/body/tail visuals.
- Player can tap head or body segment.
- Successful move animates smoothly along the path and off-board.
- Failed move bumps/shakes, flashes, and returns.
- Blocker snake/object flashes when hit.

Done when:

- 20 original levels are playable visually.
- No input is accepted during movement animation.
- No board/visual desync after repeated moves.

#### Phase G4: Gameplay Session Flow

Goal: add win/fail structure around the board.

Build:

- `LevelSessionController`
- `LevelStateMachine`
- `LevelTimer`
- `HeartManager`
- `ObjectiveTracker`
- `FailReason`
- `LevelResult`
- `LevelProgressionService`

Behavior:

- Level starts with timer.
- Level starts with 3 hearts.
- Bad taps can reduce hearts if configured.
- Timeout fails level.
- Out of hearts fails level.
- Clearing all required snakes completes level.
- Progress counter updates, such as `31/90`.

Done when:

- Win, timeout fail, and out-of-hearts fail all work.
- Pause/resume does not corrupt timer or movement state.
- Progress saves next level locally.

#### Phase G5: Gameplay UI Only

Goal: add the UI required for gameplay flow without shop, ads, IAP, or live-ops.

Build:

- `HomeScreen`
- `ModeSelectPanel`
- `GameHud`
- `PausePanel`
- `SettingsPanel`
- `LevelCompletePanel`
- `LevelFailedPanel`
- `TimeoutPanel`
- `RevivePanel` as coin/free gameplay only
- `TutorialPanel`
- `PinchZoomTutorialPanel`
- `NewObjectIntroPanel`
- `QuickMessagePanel`

HUD must show:

- current level/mode,
- timer,
- progress,
- hearts,
- pause button,
- hint button,
- path/ruler button,
- zoom control or pinch zoom prompt.

Done when:

- Player can go Home -> Play -> Complete -> Next.
- Player can fail -> replay.
- Player can pause -> resume/restart/home.
- UI blocks gameplay input when modal panels are open.

#### Phase G6: Camera, Zoom, Input Feel

Goal: match the competitor's practical mobile feel.

Build:

- `InputRouter`
- `TouchInputReader`
- `MouseEditorInputReader`
- `TapGestureStrategy`
- `DragGestureStrategy` if needed
- `GameplayCameraController`
- `PinchZoomController`
- `ZoomBarController`
- `BoardFitService`

Behavior:

- Tap selects snake reliably.
- Body segment tap counts as snake tap.
- Pinch zoom works on large boards.
- Camera fits small and large boards.
- Zoom tutorial appears once.
- UI taps never trigger board taps.

Done when:

- Large `40x40` board is playable.
- Camera never hides the active board under HUD.
- Input works in editor and on mobile touch.

#### Phase G7: Core Special Objects

Goal: add competitor-like board mechanics one by one.

Build in this exact order:

1. `StaticWallRule`
2. `BarrierRule`
3. `KingSnakeRule`
4. `WayBlockerRule`
5. `BlackHoleRule`
6. `PortalRule`
7. `PipeRule`
8. `DirectionChangerRule`
9. `WarningLineRule`

For each object:

- Add data definition.
- Add model rule.
- Add level validation.
- Add visual prefab/view.
- Add object intro tutorial.
- Add 5 original test levels.

Done when:

- Every object has model tests.
- Every object appears in playable levels.
- Object intro appears once and then stays dismissed.

#### Phase G8: Gameplay Boosters Without Ads/IAP

Goal: add boosters as gameplay tools only.

Build:

- `BoosterDefinition`
- `BoosterInventory`
- `BoosterUnlockService`
- `BoosterUsageService`
- `HintBooster`
- `PathPreviewBooster`
- `ReviveHeartBooster`
- `ReviveTimeBooster`
- `RemoveObstacleBooster` if needed

Default behavior:

- Hint unlocks around level 6.
- Path/ruler unlocks around level 12.
- Hint highlights one valid move.
- Path/ruler previews escape direction or route.
- Revive heart restores hearts after fail.
- Revive time adds time after timeout.
- Booster counts are earned through gameplay or test/debug grant, not ads/IAP.

Done when:

- Boosters cannot be used before unlock.
- Boosters cannot corrupt board state.
- Hint never selects an impossible move.
- Path preview clears correctly after use.

#### Phase G9: Gameplay Modes

Goal: implement the competitor-like mode structure without monetization systems.

Build:

- `MainSagaMode`
- `ChallengeMode`
- `RescueMode`
- `SeasonalMode`
- `ModeProgressionService`
- `ChallengeChapterPanel`
- `RescueModePanel`
- `SeasonalModePanel`

Behavior:

- Main saga is the default progression.
- Challenge unlocks by main level progress.
- Challenge uses chapters and level nodes.
- Rescue has separate progression and end panels.
- Seasonal mode has separate 24-level pack support.
- Modes can be enabled/disabled by local config only.

Done when:

- Player can switch modes.
- Each mode loads its own original level pack.
- Each mode saves separate progress.
- Each mode has complete/fail flow.

#### Phase G10: Audio, Haptics, And Feedback Polish

Goal: make the game feel responsive and competitor-like using original assets.

Build:

- `AudioService`
- `SoundCatalog`
- `HapticService`
- `EffectPool`
- `TouchEffectPlayer`
- `ComboTextPlayer`
- `ConfettiPlayer`
- `CameraShakeService`

Feedback events:

- UI click,
- snake tap,
- snake moving,
- blocked tap,
- blocker hit,
- snake escaped,
- level complete,
- level failed,
- timeout tick,
- revive used,
- object destroyed/unlocked.

Done when:

- Every major action has readable feedback.
- Fail feedback is immediate.
- Win feedback is satisfying but not slow.
- Effects are pooled.

#### Phase G11: Level Authoring And Validation Tools

Goal: build the real moat: original content production.

Build:

- `LevelEditorWindow`
- `LevelEditorSceneController`
- `LevelPaintTool`
- `LevelObjectPalette`
- `LevelExportTool`
- `LevelSolvabilityChecker`
- `DifficultyEstimator`
- `DuplicateLayoutDetector`

Validation:

- valid bounds,
- valid overlaps,
- valid path continuity,
- valid object placement,
- valid portal pairs,
- at least one legal first move,
- complete objective,
- difficulty score,
- optional author solution replay.

Done when:

- Designers can create levels without touching code.
- 100 original levels validate.
- 50 original levels are playtested.

#### Phase G12: Gameplay Release Candidate

Goal: finish the no-ads gameplay product slice.

Build:

- 100+ original main levels.
- 30+ challenge levels.
- 10+ rescue levels.
- 24 seasonal levels.
- Full tutorial/object intro sequence.
- Full gameplay UI.
- Full local save.
- Full gameplay settings.
- Full offline play.

Done when:

- App can be played offline from launch to at least level 50.
- No gameplay softlocks are found.
- No copied competitor assets, levels, IDs, or text are present.
- Performance is stable on low-end Android.
- This gameplay-only version can be used as the base before ads/IAP/live-ops are considered.

### Phase 0: Project Prep And Baseline

Deliverables:

- Freeze current prototype behavior with 5-10 smoke-test levels.
- Create module folders and namespaces.
- Add asmdefs later only after folder boundaries stabilize.
- Define level schema version `1`.
- Add `NullAdService`, `NullPurchaseService`, `NullAnalyticsService`, and local-only config.
- Create a manual smoke-test checklist.

Gate:

- Existing level can still start, play, complete, and fail.
- No competitor assets/levels/IDs added.

### Phase 1: Pure Core Gameplay Model

Deliverables:

- `LevelDefinition`, `SnakeDefinition`, object definitions.
- `BoardModel`, `BoardCell`, `BoardOccupancy`.
- `SnakeModel`, `SnakePath`, `MoveValidator`, `MovePlanner`.
- Deterministic `CanMove`, `Move`, `Blocked`, `Escaped` results.
- Unit tests for movement, collision, outside-board escape, occupancy updates.

Gate:

- 50 model-only tests pass without loading Unity scenes.
- Board occupancy never desyncs after move/fail/escape.

### Phase 2: Presentation And Input Parity

Deliverables:

- `BoardView`, `SnakeView`, pooled segment rendering.
- Tap on head/body selects the correct snake.
- Animated move along path with acceleration.
- Failed tap bump, flash, haptic, sound, and blocker highlight.
- Camera fit, pinch zoom, optional zoom bar.

Gate:

- Player can complete 20 original levels on device.
- No input accepted during animation.
- No GC spikes from spawning after warmup.

### Phase 3: Level Pipeline

Deliverables:

- JSON serializer/importer using ArrowMaze-owned schema.
- Legacy importer kept only for current prototype test data.
- Level validator:
  - bounds,
  - overlap,
  - portal pair validity,
  - invalid object placement,
  - at least one opening move,
  - objective completeness.
- Solvability checker or author-solution replay.
- Level editor/export workflow.
- Packs:
  - `main`,
  - `challenge`,
  - `rescue`,
  - `seasonal`.

Gate:

- 100 original levels validate.
- 50 original levels are playable end to end.
- No competitor plaintext or encoded level is imported.

### Phase 4: Full Game Flow UI

Deliverables:

- Home screen with mode cards and bottom nav.
- Game HUD with timer, hearts, progress, coins, boosters, pause.
- Level complete, failed, timeout, revive, settings, language, new-object intro.
- Mode panels for challenge/rescue/seasonal.
- Panel stack and modal blocking.
- Safe area support and common aspect ratio checks.

Gate:

- Full loop from cold launch to level 10 completion works.
- UI does not overlap on common portrait resolutions.
- All modal panels block gameplay input correctly.

### Phase 5: Special Objects

Deliver in this order:

1. Static walls/barriers.
2. King snake.
3. Way blockers.
4. Black holes.
5. Portals.
6. Pipes/direction changers.
7. Warning lines and object intro tutorials.

Gate:

- Each object has model tests and at least 5 authored levels.
- Tutorial/object intro appears once and persists.
- Special objects do not break solvability validation.

### Phase 6: Boosters And Economy

Deliverables:

- Wallet and save data.
- Win reward and x2 reward path.
- Hint booster.
- Path/ruler/grid booster.
- Revive heart.
- Revive time.
- Purchase booster panel with coin and reward paths.
- Free daily claim with cooldown and daily cap.

Gate:

- Boosters cannot corrupt board state.
- Duplicate reward callbacks cannot grant twice.
- Economy save/load works across app restart.

### Phase 7: Ads, IAP, Consent

Deliverables:

- Interface-first ad service.
- Interstitial, banner, app-open, rewarded placement policies.
- No-ads entitlement.
- Purchase service with restore.
- Consent state before ads.
- Ad memory gate and cooldown gate.
- Provider matrix document.

Gate:

- All monetization works with fake services in editor.
- Real SDK integration is isolated to provider adapters.
- No forced ads before configured threshold.
- No-ads entitlement removes exactly the promised surfaces.

### Phase 8: Analytics, Remote Config, Review, Notifications

Deliverables:

- Analytics event schema and validation.
- Remote config defaults and local fallback.
- Feature flags for modes and ad placements.
- Review prompt gate.
- Opt-in local notifications.
- Remote level pack interface, initially disabled.

Gate:

- App is playable offline.
- Missing remote config cannot break levels.
- Review prompt only appears after positive criteria.
- Notification permission denial does not break the app.

### Phase 9: Content Scale And Live-Ops Readiness

Deliverables:

- 300+ main levels.
- 3 challenge chapters.
- 1 rescue/special pack.
- 1 seasonal pack.
- Difficulty tags and level-name display policy.
- AB-ready config keys.
- Hotfix/rollback rules for content packs.

Gate:

- Content validation runs in CI/manual build step.
- No duplicated copied layouts.
- Difficulty curve reviewed by playtest data.

### Phase 10: Release Hardening

Deliverables:

- Build validation runner.
- Manifest permission audit.
- Package/dependency audit.
- Remove sample/debug/test/hot-reload packages from release.
- Separate dev/staging/prod service configs.
- Purchase restore test.
- Android backup policy.
- AAB/split install test.
- Low-end device performance test.

Gate:

- Release build contains no debug menus, test assemblies, CI files, competitor identifiers, or unused SDKs.
- Play Console declarations match actual manifest and SDK behavior.
- Cold start, memory, FPS, crash-free smoke tests pass.

## Performance And Optimization Plan

### Gameplay Performance

- Use grid occupancy dictionaries/arrays, not collider queries, for gameplay decisions.
- Pool snake segments, board cells, effects, combo text, and object views.
- Avoid allocating lists every tap; reuse buffers in `MovePlanner`.
- Keep path sampling deterministic and cache per snake until board changes.
- Animate visuals only; model updates should be discrete and cheap.
- Limit per-frame `Update` methods. Prefer central tickers for timers and animations.
- Use coroutines carefully; avoid one coroutine per segment for long-lived behavior.

### Rendering

- Use sprite atlases for snakes, objects, UI icons, and effects.
- Keep board cell count scalable for 25x30, 30x30, 35x35, and 40x40 levels.
- Batch static board visuals.
- Cap particle counts.
- Use object pools for repeated win/fail effects.
- Test with low-end Android devices before adding heavy UI effects.

### Memory

- Load only current level pack and common UI.
- Unload seasonal/challenge assets when leaving mode.
- Defer ad SDK preload until after onboarding threshold.
- Add low-memory guard before showing heavy ad placements.
- Track texture sizes and audio compression.
- Keep startup packages lean; avoid unused XR, recorder, hot reload, debug, and sample assemblies in release.

### Startup

- Boot only required services:
  - save,
  - local config,
  - audio,
  - level catalog.
- Initialize ads/IAP/remote config after first screen unless needed earlier.
- Do not block Home on remote config, analytics, ads, or notifications.

### Build Size

- Prefer Addressables/content packs once content grows.
- Exclude unused packages and sample content.
- Audit `Packages/manifest.json` before release.
- Consider AAB with ABI splits for Android release.
- Keep debug symbols/build metadata controlled.

## Testing And Validation

### Automated EditMode Tests

Core tests:

- level JSON parse/serialize roundtrip,
- schema version migration,
- board bounds validation,
- overlapping snake/object validation,
- portal pair validation,
- static blocker collision,
- snake escape route clear,
- blocked snake fail result,
- occupancy clear after escape,
- king snake rules,
- black hole behavior,
- way blocker unlock behavior,
- booster state changes,
- wallet earn/spend,
- save migration.

### Automated PlayMode Tests

Scene/runtime tests:

- cold boot to Home,
- load level by id,
- spawn board,
- tap snake and complete level,
- tap blocked snake and show fail feedback,
- pause/resume during level,
- timeout path,
- out-of-hearts path,
- revive with fake ad,
- complete with x2 fake ad,
- no-ads entitlement hides banner/interstitial,
- modal panels block gameplay input.

### Level Validation Tests

Run for every authored level:

- all cells in bounds,
- no illegal overlaps,
- all paths connected,
- snake directions valid,
- object references valid,
- at least one legal first move,
- level has a win condition,
- estimated difficulty matches assigned tag,
- author solution replays if available,
- timer target is reasonable,
- no duplicate layout hash.

### Monetization Tests

Use fake services first:

- rewarded ad success grants exactly once,
- rewarded ad fail grants nothing,
- interstitial cooldown respected,
- no forced ads before threshold,
- no-ads disables forced ads and banners,
- reward ads remain available after no-ads,
- purchase success grants entitlement/currency,
- purchase failure leaves state unchanged,
- restore purchase rebuilds entitlements.

### Device QA

Minimum manual device matrix:

- low-end Android,
- mid Android,
- high refresh Android,
- notched device,
- tablet or large aspect ratio,
- offline mode,
- slow network,
- airplane mode after boot,
- app pause/resume during move,
- app pause/resume during ad,
- rotate during ad,
- notification permission denied,
- notification permission granted,
- purchase restore after reinstall.

### Release Validation

Before every release:

- Manifest permission list reviewed.
- No competitor identifiers in project text search.
- No debug/test/sample/hot-reload packages in release build.
- No CI secrets or internal credential names in Resources/StreamingAssets.
- Play data safety answers match SDK behavior.
- Privacy policy matches ads/analytics/notifications.
- App works with remote config unavailable.
- Local levels playable without network.
- Crashlytics/analytics events verified in staging.
- Build size and cold-start metrics recorded.

## Production Readiness Gates

Do not call the system complete until:

- 100+ original levels are validated.
- 50+ original levels are playtested end to end.
- Every core object has tests and tutorial coverage.
- Full win/fail/revive/replay/shop loop is implemented.
- Ads/IAP are behind interfaces and tested with fake services.
- Reward callbacks are idempotent.
- Save data survives app restart and migration.
- No-ads behavior is exact and documented.
- Offline play works for local levels.
- Release build passes manifest/package hygiene audit.
- No copied competitor assets, levels, IDs, or code exist in the repo.

## Immediate Next Steps For This Project

1. Keep the existing flat scripts as prototype reference only.
2. Create `Assets/Scripts/ArrowMaze` module tree.
3. Extract pure level/domain classes from `GameManager`, `Arrow`, and `LevelImporter`.
4. Add model tests for board occupancy and movement validation.
5. Move scene-facing behavior into `Presentation` and `UI` after model tests pass.
6. Build a validator for current ArrowMaze JSON levels.
7. Create 20 original parity test levels covering normal movement, blocked movement, timer, hearts, and win flow.
8. Only after gameplay parity is stable, add economy, ads, IAP, analytics, and live-ops adapters.
