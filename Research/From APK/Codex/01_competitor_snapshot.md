# Competitor Snapshot

## Source Layout

Analyzed folder:

`C:\Users\Muhammad Ali\Downloads\snakeescape`

Top-level dump structure:

- `ExportedProject` - Unity-exported project/assets/settings.
- `Jadx\ExprotedProject` - Android resources and JADX Java decompilation output.
- `studio\MonoBehaviour` - AssetStudio-style exported raw assets and serialized MonoBehaviour JSON.
- `AuxiliaryFiles` - helper files such as `path_id_map.json` and copied game assemblies.

## Unity Build Facts

Evidence:

- `ExportedProject\ProjectSettings\ProjectVersion.txt`
- `ExportedProject\ProjectSettings\ProjectSettings.asset`
- `ExportedProject\Assets\Resources\BuildInfo.txt`

Findings:

- Unity editor version: `6000.0.59f2`.
- Product name: `Snake Escape`.
- Company name in Unity settings: `SuperGame`.
- Unity bundle version: `1.8.0`.
- Build info: `version:1.8.0 build:18 time:2026-03-18 05:25:13 +0000`.
- Build info expanded version: `1.8.0.18.2026-03-18T13:25:13`.
- Project appears portrait-first despite autorotation settings existing in Unity config.
- Runtime Android activity uses `screenOrientation="portrait"`.

The Unity `Packages\manifest.json` only shows built-in Unity modules, but the exported `Assets\Plugins` folder contains many package DLLs. This means the APK contained compiled packages/plugins, even though the recovered package manifest is minimal.

## Android APK Facts

Evidence:

- `Jadx\ExprotedProject\resources\com.snake.escape.maze.slither.puzzle.apk\AndroidManifest.xml`

Findings:

- Package: `com.snake.escape.maze.slither.puzzle`.
- Version name: `1.8.0`.
- Android version code: `186`.
- Compile SDK: `36`.
- Target SDK: `36`.
- Min SDK: `23`.
- Install location: `preferExternal`.
- Main activity: `com.google.firebase.MessagingUnityPlayerActivity`.
- Application class: `com.safedk.android.SafeDKApplication`.
- Permissions include internet, vibration, network state, advertising ID, billing, notifications, Firebase messaging, wake lock, install referrer, and ad services permissions.

Do not reuse this package name, app IDs, Facebook IDs/tokens, Firebase app config, ad IDs, or signing/config identity.

## Scenes

Evidence:

- `ExportedProject\ProjectSettings\EditorBuildSettings.asset`
- `ExportedProject\Assets\Addressables\asset-game-scene`

Scenes found:

- `Assets/Addressables/asset-game-scene/InitScene.unity`
- `Assets/Addressables/asset-game-scene/MainScene.unity`
- `Assets/Addressables/asset-game-scene/LoadingScene.unity`

Build settings enable:

- `InitScene.unity`
- `MainScene.unity`

`LoadingScene.unity` exists in Addressables but is not listed in `EditorBuildSettings.asset`.

Main scene object names include:

- `Canvas`
- `Background`
- `Main Camera`
- `AudioManager`
- `PanelSystem`
- `EventSystem`
- `GameHolder`
- `HomeScreen`
- `ShopScreen`
- `WinterModeButton`
- `GestureHandler`
- `CurrentLevelView`

## Addressables Inventory

Evidence:

- `ExportedProject\Assets\Addressables`

Important groups:

| Group | Non-meta files | Size MB | Purpose |
|---|---:|---:|---|
| `asset-game-effect` | 7 | 0.69 | effects/glow/light |
| `asset-game-level` | 1462 | 5.05 | main saga levels |
| `asset-game-level-2` | 80 | 0.35 | mode 2 levels |
| `asset-game-level-3` | 101 | 1.34 | mode 3/rescue-style levels |
| `asset-game-level-4` | 24 | 0.25 | Xmas/Winter levels |
| `asset-game-loading` | 1 | 0.13 | loading transition prefab |
| `asset-game-prefab` | 21 | 1.59 | gameplay object prefabs |
| `asset-game-scene` | 3 | 1.62 | scenes |
| `asset-ui-challenge` | 9 | 1.84 | challenge UI |
| `asset-ui-common` | 37 | 1.45 | common UI |
| `asset-ui-font` | 16 | 11.8 | font assets |
| `asset-ui-gameplay` | 29 | 5.13 | gameplay/home/shop UI |
| `asset-ui-rescue` | 4 | 1.8 | rescue UI |
| `asset-ui-shop` | 1 | 0.0 | shop particle/effect |
| `asset-ui-videobar` | 2 | 0.34 | video bar UI |
| `asset-ui-xmas` | 10 | 2.16 | winter/Xmas UI |

## Gameplay Prefabs

Evidence:

- `ExportedProject\Assets\Addressables\asset-game-prefab`

Important prefabs:

- `Snake.prefab`
- `SnakeKing.prefab`
- `Body.prefab`
- `KingBody.prefab`
- `Barrier.prefab`
- `WayBlocker.prefab`
- `BlackHole.prefab`
- `Portal.prefab`
- `Pipe.prefab`
- `DirectionChanger.prefab`
- `Grid.prefab`
- `GestureHandler.prefab`
- `WarningLine.prefab`
- `ComboText.prefab`
- `SparkleYellow.prefab`
- `SparkleYellow_Default.prefab`
- touch effect prefab

## UI Prefabs

Evidence:

- `ExportedProject\Assets\Addressables\asset-ui-gameplay\prefab`
- `ExportedProject\Assets\Addressables\asset-ui-challenge`
- `ExportedProject\Assets\Addressables\asset-ui-rescue`
- `ExportedProject\Assets\Addressables\asset-ui-xmas`

Gameplay/home prefabs:

- `BottomNav.prefab`
- `HomePanel.prefab`
- `GamePanel.prefab`
- `LevelCompletePanel.prefab`
- `LevelFailedPanel.prefab`
- `PausePanel.prefab`
- `RevivePanel.prefab`
- `TimeOutPanel.prefab`
- `PurchaseBoosterPanel.prefab`
- `UnlockBoosterPanel.prefab`
- `StorePanel.prefab`
- `RemoveAdsPanel.prefab`
- `StarterPackPanel.prefab`
- `TutorialPanel.prefab`
- `PinchZoomTutorialPanel.prefab`
- `NewObjectIntroPanel.prefab`
- `SettingPanel.prefab`
- `LanguagePanel.prefab`
- `QuickMessagePanel.prefab`
- `ResourceCollectorPanel.prefab`

Challenge/rescue/Xmas prefabs exist separately, which shows each mode has its own endgame and progression panels.

## Custom Assemblies

Evidence:

- `ExportedProject\Assets\Plugins`
- `AuxiliaryFiles\GameAssemblies`

Game-specific assemblies:

- `Game.MainGame.dll`
- `Game.MainGame.Gameplay.dll`
- `Game.Entity.dll`
- `Game.Enum.dll`
- `Game.Support.dll`
- `Game.Time.dll`
- `GDK.Audio.dll`
- `GDK.Common.dll`
- `GDK.Initialize.dll`
- `GDK.LevelDesign.dll`
- `GDK.Loader.dll`
- `GDK.SaveLoadData.dll`
- `GDK.Services.dll`
- `GDK.Transitioner.dll`

Safe metadata/string extraction found these important gameplay/system names:

- Board/gameplay: `Board`, `BoardUtil`, `GameController`, `GamePanel`, `GameUIFlowHandler`, `GameLoader`.
- Snake/arrow: `Arrow`, `KingArrow`, `ArrowRenderer`, `ArrowData`, `ArrowNode`, `SnakeRenderer`, `SnakeBodyRenderer`, `SnakeHeadPart`, `SnakeTailPart`, `KingSnakeBodyRenderer`.
- Obstacles: `Barrier`, `BlackHole`, `Portal`, `WayBlocker`, `BlockerDestroyEvent`.
- Movement: `MoveAlongPath`, `MoveBackByPath`, `MoveByPath`, `GetArrowTracePath`, `IsAheadClear`, `HitByArrow`.
- Level loading: `LevelData`, `LevelDataIO`, `LoadLevelJson`, `LoadRemoteLevel`, `GetLevelData`, `LevelRemote`, `ListRemoteLevelStorage`.
- Events: `ArrowEscapeFailedEvent`, `ArrowGoAwayEvent`, `LevelCompleteEvent`, `LevelFailedEvent`, `GameOverEvent`, `GamePauseEvent`, `GenerateLevelDoneEvent`, `CameraShakeEvent`.
- Economy/ads: `AdManager`, `AdConfig`, `AdRewardConfig`, `IAPManager`, `GrantReward`, `CanShowAd`, `CanShowAdReward`, `HideBannerAds`.
- Remote/live ops: `FirebaseRemoteConfigManager`, `FirestoreManager`, `FirebaseAnalysticManager`, `FirebaseCloudMessageManager`, `AppsflyerManager`, `AdjustManager`.

## Third-Party SDKs And Libraries

Unity/managed DLLs show:

- Unity Addressables
- Unity Localization
- Unity Input System
- Unity Notifications Android
- Unity IAP / Purchasing
- Firebase Analytics, Auth, Crashlytics, Firestore, Messaging, Remote Config
- Facebook Unity SDK
- AppsFlyer
- Google Play Core / Review
- AppLovin MAX
- DOTween / DOTween Pro
- Spine Unity
- MoreMountains Tools
- Coffee UIEffect / UIParticle
- LitMotion
- UniTask, UniRx, R3
- Odin Inspector / Sirenix
- TextMeshPro and TMPEffects
- Nice Vibrations / HapticFeedback
- Anti-Cheat Toolkit (`ACTk.Runtime.dll`)
- BGDatabase

Android manifest evidence also shows mediation/network integrations:

- AppLovin
- Google AdMob / Google Mobile Ads
- Pangle / ByteDance
- Moloco
- InMobi
- Facebook Audience Network
- Vungle
- BIGO Ads
- ironSource / LevelPlay
- SafeDK

## Serialized MonoBehaviour Evidence

Evidence:

- `studio\MonoBehaviour\MonoBehaviour`

Important serialized classes/files include:

- `Arrow.json`
- `KingArrow.json`
- `Board.json`
- `GameController.json`
- `GamePanel.json`
- `Barrier.json`
- `BlackHole.json`
- `Portal.json`
- `WayBlocker.json`
- `SnakeRenderer.json`
- `SnakeBodyRenderer.json`
- `KingSnakeBodyRenderer.json`
- `HintButton.json`
- `RulerBoosterButton.json`
- `HeartView.json`
- `RevivePanel.json`
- `TimeOutPanel.json`
- `HomePanel.json`
- `BottomNavManager.json`
- `ChallengeModePanel.json`
- `RescueModePanel.json`
- `StorePanel.json`
- `LevelCompletePanel.json`
- `LevelCompleteMode2Panel.json`
- `LevelCompleteMode3Panel.json`
- `LevelCompleteMode4Panel.json`
- `LevelFailedPanel.json`
- `DataBooster.json`

These files are useful for understanding serialized fields and component references. They should not be copied into ArrowMaze.
