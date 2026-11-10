# Hidden Secrets And Dark Truths Report

Source analyzed: `C:\Users\Muhammad Ali\Downloads\snakeescape`

Output folder: `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Codex`

## Scope And Redaction

This report lists hidden, high-risk, and easy-to-miss findings from the extracted APK/project files. Sensitive public client identifiers found in the APK are intentionally redacted here. Do not reuse competitor package names, ad IDs, Firebase keys, Facebook tokens, product IDs, assets, code, or exact levels in ArrowMaze.

## Executive Summary

The competitor is not only a snake puzzle game. It is a Unity live-ops monetization shell around a large level library. The visible gameplay is simple, but the hidden structure is built around staged ads, rewarded-ad pain relief, remote configuration, analytics, attribution, notifications, IAP offers, event modes, and protected/encoded content.

The most important "dark truth" is that the first play session is probably designed to feel cleaner than the later game. Static ad config disables interstitials through early levels, then turns them on after level 25, increases pressure after level 100, and enables lose interstitials in the later band.

## Hidden Findings

### 1. The Clean Early Game Is A Deliberate Funnel

Evidence:

- `AdConfig_Default.asset` has interstitials disabled from levels `0-25`.
- Levels `26-100` allow interstitials with `180` second cooldown and `3` level cooldown.
- Levels `101+` allow interstitials with `120` second cooldown and `2` level cooldown.
- Lose interstitials are disabled before level `101`, then enabled from `101+`.
- Banner ads start at level `18`.
- App open ads start at level `6`.
- Ad preloading starts at level `2`.

Interpretation:

The game withholds the most disruptive ads until the player has already learned the loop. It then increases ad frequency after the player has more sunk progress.

Clean-room lesson for ArrowMaze:

- If you use ads, design a transparent ad curve. Do not copy their thresholds blindly. Use your own telemetry and keep early retention, user trust, and store compliance in mind.

### 2. Rewarded Ads Are Attached To Pain Points

Evidence:

- Rewarded ads are enabled in the default preset.
- Hint ad reward unlocks at level `6`, with `2` uses per level.
- Ruler/grid ad reward unlocks at level `12`, with `2` uses per level.
- Revive reward unlocks at level `3`, with `2` uses per level.
- The database strings include `ShowReward`, `BonusReward`, `EndGameReward`, `video_bar_enable`, `video_bar_reward`, and many `CoinToken` reward entries.

Interpretation:

Reward ads are not random extras. They are placed exactly where the player feels blocked: revive, hint, grid/ruler, endgame, daily video bar, and coin recovery.

Clean-room lesson for ArrowMaze:

- Build boosters as real gameplay tools, not just ad triggers. If rewarded ads exist, every reward must also have a fair non-ad earn path.

### 3. "No Ads" Is Not A Simple Global Switch

Evidence:

- IAP product strings include `Remove Ads`, `Remove Ads Bundle`, starter packs, Santa gift, and coin packs.
- The ad config has multiple presets. One preset pushes banner start far away and changes reward/interstitial behavior, but the app still ships the full ad SDK stack, ad activities, ad memory gates, app-open ad settings, and mediation code.
- The static config contains `skipAd:false`, plus separate flags for ad break, rewarded ads, open ads, banners, and interstitials.

Interpretation:

The app is not built as "ads on or off." It is built as many independent ad surfaces controlled by config, purchase state, and possibly remote values.

Clean-room lesson for ArrowMaze:

- If selling "No Ads", define exactly which ad types it removes: banner, interstitial, app-open, ad break, and optional rewarded ads. Avoid vague purchase behavior.

### 4. Main Level Content Is Protected Or Obfuscated

Evidence:

- Main saga level files are encoded-looking single-line text rather than readable JSON.
- Mode 2 level files are also encoded-looking.
- Mode 3 and Mode 4 levels are readable JSON-like plaintext, which proves the unreadable packs are intentionally treated differently.
- Extracted strings mention `GetDataDecompressed`, `EncryptedSaveService`, anti-cheat package `com.codestage.anticheat`, and `Tangle.dll`.

Interpretation:

The largest and most valuable level content is not left as plain JSON. The readable files are likely secondary/event/test packs or modes that were not protected the same way.

Clean-room lesson for ArrowMaze:

- Build your own level authoring and packing pipeline. Do not decode, copy, or import competitor levels. Use the observed schemas only as inspiration for your own schema.

### 5. Remote Config And Firestore Can Change The Game After Install

Evidence:

- Manifest registers Firebase Remote Config, Firestore, Firebase Messaging, Firebase Storage, Firebase ABT, Firebase Analytics, Crashlytics, Sessions, and Installations.
- Database strings include `Path_folder_firestore`, `Level_Max_Firestore`, `Level_PackageHotfix_Version_Local`, `Hotfix_Version_Firestore`, `Path_Hotfix`, `AB_Test_Path_Default`, and `Level_Package_Default`.
- `SandBoxIDConfig.asset` embeds Firebase project config for Android and iOS.

Interpretation:

The APK is not the full source of truth. Ads, level packages, hotfix versions, events, difficulty lists, and monetization flags can likely be changed remotely.

Clean-room lesson for ArrowMaze:

- Implement your own Remote Config only for controlled flags, difficulty tuning, and content rollout. Keep defaults sane when offline. Never depend on competitor endpoints.

### 6. The Tracking And Ad Network Footprint Is Heavy

Evidence:

- Manifest permissions include `INTERNET`, `ACCESS_NETWORK_STATE`, `AD_ID`, Android ad services attribution, ad services ad ID, custom audience, topics, install referrer, Wi-Fi state, notifications, billing, wake lock, and foreground service.
- Registered SDKs include AppLovin MAX, AdMob, Pangle/ByteDance, Moloco, BidMachine, InMobi, Facebook Audience Network, Vungle, BIGO, ironSource/LevelPlay, Mintegral resources, SafeDK, AppsFlyer, Firebase, and Facebook.
- Performance dependency list confirms many mediation adapters for Android and iOS, including Google, Google Ad Manager, BidMachine, BIGO, ByteDance, Facebook, Fyber, InMobi, ironSource, Mintegral, Moloco, Unity Ads, and Vungle.

Interpretation:

This is a broad ad mediation and attribution stack, not a lightweight puzzle app. It carries privacy, consent, policy, and QA burden.

Clean-room lesson for ArrowMaze:

- Start with fewer SDKs. Add networks only when you can support consent flows, privacy policy disclosures, ATT/GDPR/CCPA handling, crash monitoring, and adapter QA.

### 7. Public Client Configs Are Exposed In The APK

Evidence:

- `strings.xml` and `SandBoxIDConfig.asset` expose Firebase project number, app id, storage bucket, project id, and client API key.
- `FacebookSettings.asset` and manifest metadata expose the Facebook app id and client token.
- `SandBoxIDConfig.asset` exposes AdMob app id-style strings.
- Privacy and terms URLs are embedded.

Redacted examples:

- Firebase project id: `snake-escape-gp`
- Firebase Android API key: `AIza...rnG2k`
- Firebase iOS project id: `snake-escape-ios`
- Facebook app id: `367313...927748`
- Facebook client token: redacted
- AdMob app id prefix: `ca-app-pub-9084327244941949~...`

Interpretation:

Some client IDs are normally public in mobile apps, but they are still competitor infrastructure. Reusing them would be wrong and could break analytics, ads, billing, privacy reporting, and store review.

Clean-room lesson for ArrowMaze:

- Create fresh Firebase, Facebook, AdMob/AppLovin, AppsFlyer, and store product IDs. Never ship competitor identifiers.

### 8. CI, Build, And White-Label Leftovers Are Shipped

Evidence:

- `BuildInfo.txt` exposes version `1.8.0`, build `18`, build time `2026-03-18 05:25:13 +0000`, Unity `6000.0.59f2`, branch-like build path, commit hash, and internal builder/user text.
- `config.txt` exposes `BUILD_METHOD_NAME=SandBox.Tools.JenkinsBuild.Build`, `SLACK_DEFAULT_CHANNEL=ci`, `DATA_PATH=ID/ID-{0}.txt`, empty keystore fields, `TeamName=DungeonLabs`, and `GameID=WoodNuts`.
- `Jenkinsfile.txt` is included and references Unity credentials, Firebase tokens, Google credentials, Fastlane credentials, publishing stages, and artifact archive paths by credential id/name. It does not expose the actual secret values in the extracted text I reviewed.
- `BillingMode.json` says `GooglePlay`.
- `PerformanceTestRunInfo.json` includes the package dependency list and build target details.

Interpretation:

The shipped resources contain internal pipeline fingerprints and possibly template leftovers. `GameID=WoodNuts` is especially notable because it does not match Snake Escape, suggesting reused infrastructure, white-label project history, or generic build templates.

Clean-room lesson for ArrowMaze:

- Keep CI files, credential names, sandbox configs, build metadata, and internal team names out of production resources unless they are explicitly needed.

### 9. Debug And Mediation Tools Are Present In Production Files

Evidence:

- `Resources/srdebugger/Settings.asset` exists. It is disabled (`_isEnabled: 0`) and has entry-code protection enabled, but the UI prefabs are still shipped.
- Manifest includes many AppLovin MAX debugger, waterfall, test mode, CMP, TCF, and creative debugger activities.
- Unity debug UI prefabs and performance test files exist in the exported assets.

Interpretation:

The production APK carries debug/admin tooling artifacts. They may be disabled, but they increase package size and attack surface.

Clean-room lesson for ArrowMaze:

- Strip debug panels, mediation debugger UI, performance test files, and internal test prefabs from release builds unless a provider requires them and they are gated safely.

### 10. Notifications Are Retention Pressure, Not Utility

Evidence:

- `Notification Data.asset` schedules four daily time bands: morning around `8`, noon around `12.5`, evening around `19`, and night around `22.5`.
- Notification copy uses claims like "Only 10% can pass this level", daily gift prompts, brain-training language, break-time prompts, and "one last game" night prompts.
- Manifest requests `POST_NOTIFICATIONS`, `WAKE_LOCK`, Firebase Messaging, and Unity notification services.

Interpretation:

Notifications are designed to pull users back multiple times per day using curiosity, scarcity, daily gift framing, and light pressure.

Clean-room lesson for ArrowMaze:

- Use opt-in notifications for useful reminders only: daily reward available, event ending, or level pack release. Keep claims honest and avoid medical/brain-health implications unless legally supportable.

### 11. The Economy Is Tuned Around Coins, Boosters, And Bundles

Evidence:

- Product strings include six coin packs: `1000`, `5000`, `10000`, `25000`, `50000`, and `100000`.
- Product strings include `Remove Ads`, `Remove Ads Bundle`, `Starter Pack`, `Starter Pack Bundle`, and `Santa Gift Box`.
- Token types include `CoinToken`, `RemoveAdToken`, `FreeToken`, `AdToken`, `ShuffleToken`, `UndoToken`, `MagnetToken`, `SlotToken`, `Vip1Token`, `Vip2Token`, `Vip3Token`, `ReviveToken`, `LiveToken`, `LivePackToken`, `HintToken`, and `RulerToken`.
- Shop strings and prefabs expose no-ads panels, starter pack panels, Santa/Xmas bundle UI, daily video bar UI, and free-claim style rewards.

Interpretation:

Even if only some boosters are visible in the current build, the economy database contains hooks for a broader monetization system.

Clean-room lesson for ArrowMaze:

- Design a simpler first economy. Ship only tokens and boosters you actively use, then expand after telemetry proves the need.

### 12. Multiple Event Modes Are Already Planned Or Gated

Evidence:

- Addressables include main, mode2, mode3, and mode4/winter/xmas style level groups.
- Database strings include `rescue_mode_enable`, `rescue_level_unlock`, `challenge_mode_enable`, `challenge_level_unlock`, `xmas_theme_enable`, `winter_escape_mode_enable`, `winter_escape_level_unlock`, and `xmas_pack_enable`.
- Prior level analysis found locks around Mode 2, Mode 3, and Mode 4 progression bands.

Interpretation:

The app is structured to keep users engaged with seasonal/event modes and remotely gated content, not only a single saga.

Clean-room lesson for ArrowMaze:

- Build one strong main mode first, then add event modes through a clean content pack system.

### 13. Review, Difficulty, And Level Naming Are Config-Driven

Evidence:

- Database strings include `ReviewLevelConfig`, `ReviewLevelCoolDownConfig`, `level_difficulty_list`, `level_diff_enable`, `show_level_namelv_start_show_level_name`, and `GestureMode`.
- UI includes a review popup prefab.

Interpretation:

The game can tune when it asks for reviews and how it labels/difficulty-tags levels. Review timing is probably part of progression design, not a simple button.

Clean-room lesson for ArrowMaze:

- Ask for reviews only after real positive moments, such as a successful level streak, not after failure or forced ads.

### 14. The Real Moat Is Content Volume Plus Live Ops

Evidence:

- Main saga has `1451` numeric level files plus named/special files.
- Additional modes contain `80`, `101`, and `24` files.
- The app ships remote config, Firestore, AB testing, hotfix path strings, daily video bar, notifications, event mode toggles, and a broad ad/IAP stack.

Interpretation:

The game is not hard only because of its tap-to-escape mechanic. The harder part to copy is the content production pipeline, difficulty pacing, event cadence, economy tuning, ad mediation, and update pipeline.

Clean-room lesson for ArrowMaze:

- Build a level generator/editor, validation scripts, difficulty tags, and remote rollout controls. Do not try to hand-copy their level library.

## High-Risk Items To Avoid Copying

- APK package name: `com.snake.escape.maze.slither.puzzle`
- App name and branding: `Snake Escape`
- Firebase project IDs, app IDs, API keys, storage buckets, and sender IDs
- Facebook app ID/client token
- AdMob/AppLovin/ad network app IDs and ad unit IDs
- Store product IDs such as `snakeescape_*`
- Exact level files, encoded or plaintext
- Textures, Spine files, sounds, prefabs, UI layouts, notification copy, and shop art
- CI scripts, credential names, internal team names, and pipeline config

## What ArrowMaze Can Safely Learn

- Keep the core loop simple: tap/select an arrow/snake, validate path, animate escape, clear board.
- Use a staged difficulty curve with special mechanics introduced gradually.
- Use original level schemas inspired by fields like board size, timer, snakes, blockers, portals, and hazards.
- Build a clean content pipeline for main saga and optional event packs.
- Add boosters with clear unlock levels and fair earn paths.
- Use remote config for your own flags, but keep defaults playable offline.
- Keep monetization transparent. Avoid selling "No Ads" unless every removed surface is clearly defined.
- Reduce SDK footprint compared with the competitor unless there is a real business reason.

## Bottom Line

The competitor's hidden advantage is not one secret mechanic. It is the combination of a large protected level library, staged ad pressure, reward ads at failure points, live remote tuning, push-notification retention, event packs, IAP bundles, broad mediation, and analytics/attribution infrastructure.

For ArrowMaze, the useful strategy is clean-room parity at the product-design level: original mechanics implementation, original levels, original assets, original IDs, simpler SDK stack, and a more transparent monetization policy.
