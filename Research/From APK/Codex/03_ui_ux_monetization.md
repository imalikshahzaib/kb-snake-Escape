# UI, UX, Monetization, Audio, And Visual Research

## Main UI Structure

Evidence:

- `MainScene.unity`
- `HomePanel.prefab`
- `BottomNav.prefab`
- `StorePanel.prefab`
- serialized files in `studio\MonoBehaviour\MonoBehaviour`

Main scene contains:

- `Canvas`
- `Background`
- `Main Camera`
- `AudioManager`
- `PanelSystem`
- `EventSystem`
- `GameHolder`
- `HomeScreen`
- `ShopScreen`
- `GestureHandler`
- `WinterModeButton`
- `CurrentLevelView`

Bottom navigation:

- `BottomNavManager.json` has `3` buttons and `3` screens.
- Text evidence shows bottom tabs for `Shop`, `Home`, and `Challenge`.

Main player-facing panels:

- Home
- Game
- Pause
- Level complete
- Level failed
- Timeout
- Revive
- Shop/store
- Remove ads
- Starter pack
- Purchase booster
- Unlock booster
- Challenge mode
- Rescue mode
- Xmas/Winter mode
- Settings
- Language
- Review/rate
- Internet/offline popup
- New object intro
- Pinch zoom tutorial

## Home Screen

Recovered home prefab text includes:

- `Normal`
- `Hard`
- `Super Hard`
- `Level 12`
- `Remove Ads`
- `Starter Pack`
- `Special Deals`
- `No Ads Bundle`
- `Challenge`
- `Chapter 1`
- `Unlock at Lv.50`
- `Chapter 3\nUnlock At Lv.75`
- `Coming soon!`
- `Shop`
- `Home`

There are also seasonal/home edge items:

- Santa gift
- Winter
- Video bar
- Rescue

Some recovered prefab strings are in Japanese/Korean/Russian/French/Spanish despite the English table existing. Treat these as localization fallback artifacts from the extraction, not as final UX copy to copy.

## Gameplay HUD

Evidence from `GamePanel.prefab`:

- level label text such as `Level 12`
- chapter labels
- progress text like `31/90`
- timer text like `1:36`
- zoom percentage text like `10%`
- heart count text
- booster unlock labels like `Lv.12` and `Lv.6`
- top coin view
- combo text such as `Great!`

Expected HUD elements:

- top bar with coins and plus button,
- current level/chapter,
- timer,
- board progress / remaining snakes,
- hearts,
- pause button,
- hint booster,
- ruler/grid booster,
- zoom bar or pinch zoom support,
- temporary combo/feedback text.

## Endgame Flow

Level complete panel evidence:

- text format: `Level {0}\nComplete!`
- coin reward: `+40`
- rewarded double option: `x2 Coin`
- doubled coin text: `<sprite=0> 80`
- normal claim text: `<sprite=0> 40`
- next button: `Next`

Mode-specific complete panels exist:

- `LevelCompleteMode2Panel`
- `LevelCompleteMode3Panel`
- `LevelCompleteMode4Panel`

Mode 3 complete panel includes move text such as `Moves: 15`, implying one mode tracks moves as a success metric.

Failure panel evidence:

- fail/restart text,
- out-of-hearts and timeout reason text,
- restart/replay button,
- revive options:
  - `+1`
  - `+3`
  - `300 <sprite=0>`

Practical loop:

- Win gives 40 coins by default.
- Rewarded ad can double to 80.
- Failure can offer replay/revive using ads or coins.
- Continue/next is prominent after reward claim.

## Shop And Economy

Evidence:

- `HomePanel.prefab`
- `StorePanel.prefab`
- `RemoveAdsPanel.prefab`
- `StarterPackPanel.prefab`
- `PurchaseBoosterPanel.prefab`

Prefab text shows these offers/prices. These are recovered static prefab values and may differ from live store metadata.

Coin pack text:

| Offer | Recovered price text |
|---:|---:|
| 100 coins | `Free (3/5)` |
| 1,000 coins | `$ 2.99` |
| 5,000 coins | `$ 7.99` |
| 10,000 coins | `$ 14.99` |
| 25,000 coins | `$ 29.99` |
| 50,000 coins | `$ 52.99` |
| 100,000 coins | `$ 100.99` |

Other offer text:

- No Ads: `$ 2.99`
- No Ads Bundle: `$ 4.99`, includes `3000` coins and booster quantities shown as `x3`, `x2`, `x3`.
- Starter Pack: `$ 3.99`, includes `1000` coins and two items shown as `x1`, `x1`.
- Santa's Gift appears as a seasonal/IAP offer.
- Special Deals section exists.

Remove ads panel text indicates:

- removes obligatory ads,
- removes bottom banner ads,
- keeps optional reward ads.

Implementation implication:

- No-ads purchase should remove forced interstitial/banner/open ads only.
- Reward ads should stay available for voluntary rewards.
- Store should group coin packs, no-ads, bundles, and limited/seasonal offers.

## Ad Configuration

Evidence:

- `ExportedProject\Assets\Resources\AdConfig_Default.asset`
- `AdNetworkConfig.asset`
- Android manifest SDK entries

Default ad preset:

- Banner ad starts at level `18`.
- Interstitial levels `0-25`:
  - `timeCoolDown: 99999999`
  - `levelCoolDown: 99999999`
  - lose interstitial disabled
  - regular interstitial disabled
- Interstitial levels `26-100`:
  - `timeCoolDown: 180`
  - `levelCoolDown: 3`
  - lose interstitial disabled
  - regular interstitial enabled
- Interstitial levels `101+`:
  - `timeCoolDown: 120`
  - `levelCoolDown: 2`
  - lose interstitial enabled
  - regular interstitial enabled
- Reward ads enabled.

Reward ad config:

| Reward type | Level show | Limit per level |
|---|---:|---:|
| Hint | 6 | 2 |
| Ruler | 12 | 2 |
| ReviveTime | 3 | 2 |
| ReviveLive | 3 | 2 |

Open ad config:

- `openAdLevelStart: 6`
- `openAdCoolDown: 60`
- `openAdWaitTime: 6`

Preload config:

- `preloadAdLevelStart: 2`
- `preloadAd_FirstDelay: 50`
- `preloadAdDelay: 4`

Ad break config:

- `adBreakEnable: 0`
- `adBreakReward: CoinToken,10`
- `adBreakRewardDescription: +10<sprite=0>`
- `adCoolDown: 10`

Memory gate config:

- Low-memory protocol enabled.
- Low-memory warning count: `4`.
- Ad placement memory requirements are around `1024 MB`, with preload requirements from `1500 MB` to `2048 MB`.

Ad network config:

- Four network entries exist.
- Only one entry is active in the recovered `AdNetworkConfig.asset` (`active: 1`, `type: 2`, `placements: 14`).
- Manifest still includes many mediation adapters and SDKs.

Implementation implication for ArrowMaze:

- Do not show forced ads in the first 25 levels.
- Start banners after a defined early-game threshold.
- Make rewarded ads feel optional and tied to useful relief: hint, ruler/path, revive time, revive hearts, double coins.
- Respect no-ads purchase for forced ads.
- Use your own ad network setup and product IDs.

## Analytics And Live Ops

Evidence from assemblies and manifest:

- Firebase Analytics
- Firebase Crashlytics
- Firebase Remote Config
- Firestore
- Firebase Messaging
- AppsFlyer
- Adjust strings in support assembly
- Google Play Review
- Facebook SDK
- Unity Notifications Android

Important implementation pattern:

- Level data can be local Addressables plus remote override/AB test paths.
- Strings found include `LoadRemoteLevel`, `AddLevelRemote`, `LoadABTestLevel`, `GetLevelRemote`, `levelMaxFirestore`, `FirebaseRemoteConfigManager`, and `FirestoreManager`.

Useful ArrowMaze metrics to track:

- level start, complete, fail,
- fail reason: blocked, timeout, out of hearts,
- snake tap fail,
- booster used,
- rewarded ad shown/completed/skipped,
- revive accepted,
- IAP shown/purchased/failed,
- ad revenue/impression,
- level difficulty balance.

## Audio

Evidence:

- `ExportedProject\Assets\Resources\SoundData.asset`
- `ExportedProject\Assets\AudioClip`
- `studio\MonoBehaviour\AudioClip`

Core mapped sound fields:

- `clickSfx`
- `moveSfx`
- `tapSfx`
- `stoneBroke`
- `swish`

Audio asset names:

- `SE_Click`
- `SE_Snake_Move`
- `SE_Snake_Tap`
- `StoneBreak`
- `Swish1`
- `TickTock`
- `timeout`
- `SE_revive_popup`
- `SE_lose_popup`
- `SE_win_popup1`
- `SE_win_popup2`
- coin sounds
- UI success sounds
- confetti explosion
- positive voice/feedback labels: amazing, awesome, excellent, great

Implementation implication:

- Separate short UI click, snake tap, snake movement, obstacle break, swish/escape, timeout ticking, win popup, lose popup, coin collect, confetti, and revive popup sounds.
- Use original sounds only.

## Haptics

Evidence:

- `VibrationConfig.asset`
- haptic DLLs: CandyCoded Haptic Feedback, Lofelt/NiceVibrations.

Presets:

| Preset | Power | Frequency | Duration |
|---:|---:|---:|---:|
| 1 | 0.10 | 0.01 | 0.04 |
| 2 | 0.15 | 0.01 | 0.06 |
| 3 | 0.20 | 0.01 | 0.08 |
| 4 | 0.20 | 0.01 | 0.05 |

Implementation implication:

- Use very short tactile feedback.
- Differentiate light tap, fail, success, obstacle break, and win/lose events.

## Visual Style

Evidence:

- `Texture2D`, `Sprite`, `TextAsset` names.
- Spine data for logos/snake/gates.
- Addressables UI groups.

Visual categories:

- snake body/head/tail sprite sets per color,
- board and grid elements,
- obstacles: barrier, wall, black hole, portal gate, pipe, redirection,
- glow/light/sparkle/confetti effects,
- bright rounded mobile UI panels,
- large illustrated seasonal backgrounds,
- tabs/buttons/top bars,
- coin and booster icons,
- mode panels for challenge/rescue/winter.

Sprite naming shows large categories:

- `snake`: 257 sprite assets.
- `home`: 78.
- `popup`: 45.
- `challenge`: 31.
- `play`: 28.
- `winter`: 22.
- `shop`: 17.
- `rescue`: 16.
- `obj`: 16.

Texture names indicate:

- `loading_BG`
- `challenge_BG`
- `home_winter_BG`
- `winter_main_BG`
- `HardLevel`
- `CryingSnake`
- `IAP_VideoBar`
- `NoAds`
- `Bundle`
- `AdIcon`
- `obj_blackhole`
- `obj_gate`
- `obj_pipe`
- `obj_portalgate_*`
- `play_iconbooster_grid`
- `play_iconbooster_time`
- `play_heart*`
- `lose_panel`
- `popup_buybooster_*`

Implementation implication:

- The competitor relies heavily on polished asset volume and mode-specific skins.
- For ArrowMaze, build an original visual language but keep the same functional asset categories:
  - snake segment variants,
  - object icons,
  - booster icons,
  - mode cards,
  - win/lose panels,
  - shop packs,
  - animated feedback.

## UX Details To Recreate With Original Content

High-value UX patterns:

- First screen should be usable home/game mode screen, not a landing page.
- Home has play difficulty options plus edge promotions/modes.
- Bottom tabs keep `Home`, `Shop`, and `Challenge` reachable.
- Gameplay HUD keeps time, level, coins, hearts, boosters, and zoom controls visible.
- Early levels teach zoom and objects with lightweight panels.
- New object intro panel appears when a mechanic unlocks.
- Win panel offers normal claim and x2 reward claim.
- Failure panel offers revive, replay, or home.
- Booster purchase panel offers coin purchase and ad/free path.
- Remove-ads messaging explicitly says reward ads remain optional.

Clone-risk items to avoid:

- exact text layout and art,
- exact level paths,
- exact sprite names/assets,
- exact product names like Snake Escape,
- APK package IDs and SDK identifiers,
- extracted audio,
- extracted Spine animations.
