# Deeper Hidden Secrets And Solid Plan

Source analyzed: `C:\Users\Muhammad Ali\Downloads\snakeescape`

Output folder: `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Codex`

## Boundary

This is competitor research for clean-room learning. Do not copy their package name, ad IDs, Firebase keys, AppLovin SDK key, Facebook token, product IDs, exact levels, textures, Spine data, audio, CI scripts, or endpoints. Sensitive identifiers found in the APK are redacted here.

## Solid Research Plan Used For This Pass

1. Map every remaining hidden surface:
   - Android manifest, `res/xml`, `res/raw`, `assets/aa`, Unity `Resources`, MonoBehaviour dumps, Addressables bundles, native split, runtime metadata, and serialized UI prefabs.
2. Extract live-ops and economy keys:
   - `bansheegz_database.bytes`, `global-metadata.dat`, UI product IDs, reward strings, config flags, remote level names, and hotfix names.
3. Inspect privacy/tracking/ad infrastructure:
   - Android ad services config, AppLovin consent flow, UMP properties, AppsFlyer backup rules, manifest queries/permissions/components, and ad-network files.
4. Inspect debug/protection/runtime clues:
   - SRDebugger, Unity debug UI, ACTk/Tangle, runtime initialization list, scripting assemblies, native libraries, build files, and CI leftovers.
5. Convert findings into ArrowMaze actions:
   - What to copy only as a design idea, what to avoid completely, what systems to build cleanly, and what legal/compliance risks to plan for.

## Missed Findings From The First Report

### 1. Android Ad Services Are Opened Broadly

Evidence:

- `res/xml/ad_services_config.xml` allows all access for:
  - attribution,
  - custom audiences,
  - topics.
- Manifest also requests:
  - `ACCESS_ADSERVICES_ATTRIBUTION`,
  - `ACCESS_ADSERVICES_AD_ID`,
  - `ACCESS_ADSERVICES_CUSTOM_AUDIENCE`,
  - `ACCESS_ADSERVICES_TOPICS`,
  - Google advertising ID permission.

Meaning:

This is not just normal ad display. The app is configured for modern Android ad targeting/measurement APIs, including topics and custom-audience style signals.

ArrowMaze action:

- Do not enable these permissions by default. Add them only if a real ad provider requires them and your privacy policy/consent flow covers them.

### 2. AppsFlyer Purchase And Attribution Data Is Deliberately Excluded From Backups

Evidence:

- `appsflyer_backup_rules.xml` excludes:
  - `sharedpref/appsflyer-data`,
  - `sharedpref/appsflyer-purchase-data`,
  - `database/afpurchases.db`.
- `appsflyer_data_extraction_rules.xml` excludes the same data from cloud backup and device transfer.

Meaning:

Attribution and purchase tracking data is treated as sensitive/non-transferable. This is a signal that AppsFlyer purchase tracking is part of the monetization stack, not an unused plugin.

ArrowMaze action:

- If using attribution/purchase analytics, decide exactly what is backed up, what is excluded, and document it in privacy policy and QA.

### 3. AppLovin Consent Flow Is Embedded And Enabled

Evidence:

- `res/raw/applovin_settings.json` has:
  - `consent_flow_enabled: true`,
  - privacy policy URL,
  - AppLovin terms URL,
  - an AppLovin SDK key, redacted here as `NPP8...CXi_-`.
- `applovin_consent_flow_unified_cmp.json` defines GDPR decision states, CMP load/show states, terms/privacy alert states, and events named `tos_ok` and `gdpr_ok`.

Meaning:

The app carries a formal ad-consent flow. This is a compliance and monetization infrastructure layer, not just a UI preference.

ArrowMaze action:

- If you add multi-network ads, implement consent first. Do not ship ad networks before you know consent states, age gating, GDPR/CCPA handling, and store-disclosure answers.

### 4. Unity Services Is Set To Production

Evidence:

- `UnityServicesProjectConfiguration.json` includes:
  - `com.unity.services.core.environment-name = production`,
  - Unity Purchasing version `4.13.0`,
  - Unity Services Core version `1.14.0`,
  - initialized packages `com.unity.purchasing;com.unity.services.core`.

Meaning:

The APK is not a loose prototype. Its Unity services are configured as a production monetization build.

ArrowMaze action:

- Use separate dev/staging/production service projects. Never test purchases or ads directly on production config.

### 5. The APK Has A Large Component Surface

Evidence from manifest count:

- `21` permissions.
- `94` activities.
- `18` services.
- `14` providers.
- `17` receivers.
- `14` exported=true components.
- `59` metadata entries.

Meaning:

This puzzle app carries a large Android runtime surface because of ads, billing, notifications, Firebase, Facebook, attribution, WorkManager, debug/mediation tools, and Play services.

ArrowMaze action:

- Track manifest growth. Every SDK should justify its permissions, components, receivers, and Play Console data safety declarations.

### 6. Manifest Queries Reveal External Intent And App-Probing Behavior

Evidence:

The manifest `queries` section checks or references:

- Chrome/WebView/browser handlers,
- Google Play Store,
- Facebook and Facebook Lite,
- Instagram,
- Samsung maps agent,
- AppLovin AppHub service,
- AppsFlyer install-referrer provider,
- Android billing and billing test companion,
- attribution referral providers,
- Digital Turbine Ignite remote service,
- dial and view intents.

Meaning:

The app can discover/install-referrer, billing, browser, social, and ad-partner surfaces around the device. Some of this is SDK-driven, but it increases privacy and review complexity.

ArrowMaze action:

- Keep `queries` minimal. Do not inherit SDK query entries unless the feature is active.

### 7. App Open / Native Ad Revenue Is Explicitly Tracked

Evidence from metadata strings:

- `AD_REVENUE_SDK`
- `AD_OPEN_NATIVE_IMPRESSION`
- `AD_REWARD_GGCAMP`
- `cc_openad_native_revenue`
- `ad_format`, `ad_network`, `ad_platform`, `ad_unit`, `ad_unit_name`
- `LogAdRevenue`, `SendRevenue`, `LogPurchase`
- `OPEN_AD`, `NATIVE_AD`, `REWARDED_VIDEO`

Meaning:

They are not only showing ads. They are logging revenue-level ad events and separating formats such as open/native/rewarded.

ArrowMaze action:

- If you track ad revenue, normalize events early: `ad_impression`, `ad_revenue`, `placement`, `network`, `format`, `level`, `user_segment`. Keep the event model provider-neutral.

### 8. Multiple Analytics Vendors Are Planned In Code

Evidence from metadata strings:

- Firebase-style events:
  - `LEVEL_ACHIEVED`,
  - `firebase_app_instance_id`.
- Facebook-style events:
  - `fb_mobile_level_achieved`,
  - `fb_mobile_purchase`,
  - `fb_mobile_rate`,
  - `fb_mobile_tutorial_completion`,
  - `fb_mobile_initiated_checkout`.
- AppsFlyer-style events:
  - `af_level`,
  - `af_level_achieved`,
  - `af_purchase`,
  - `af_rate`,
  - `af_tutorial_completion`.
- Adjust-style names also appear:
  - `ADJUST_IAP_PURCHASE`,
  - `ADJUST_INTERSTITIAL_AD`,
  - `ADJUST_LEVEL_TRACKING`,
  - `ADJUST_REWARD_AD`.

Meaning:

The app appears built with an analytics abstraction that can feed several vendors. Even if one provider is inactive, the architecture supports multi-vendor tracking.

ArrowMaze action:

- Build one analytics abstraction, but start with one analytics backend. Avoid duplicate event reporting until you have a data QA process.

### 9. Hidden Game Modules Exist Beyond The Visible UI

Evidence from `global-metadata.dat`:

- `Game.Entity.DB_Config`
- `Game.Entity.DB_IAPProduct`
- `Game.Entity.DB_PushNotification`
- `Game.Entity.DB_Store`
- `Game.MainGame.Database`
- `Game.MainGame.Tracker`
- `Game.Modules.AdTracker`
- `Game.Modules.Advertisement`
- `Game.Modules.AppReview`
- `Game.Modules.Configs`
- `Game.Modules.Firebase`
- `Game.Modules.MultiConfig`
- `Game.Modules.Notification`
- `Game.Modules.Purchasing`
- `Game.Modules.RemoteLevel`
- `Game.Modules.RemoteLevel.PackData`
- `Game.Modules.SRDebugger`
- `Game.Modules.Vibration`

Meaning:

The real architecture is modular: database, config, tracker, ads, app review, Firebase, multi-config, notification, purchasing, remote level, and debug modules.

ArrowMaze action:

- Mirror the architecture concept, not the code:
  - `CoreGame`,
  - `LevelContent`,
  - `Economy`,
  - `Ads`,
  - `Purchasing`,
  - `Analytics`,
  - `RemoteConfig`,
  - `LiveOps`,
  - `Notifications`,
  - `DebugTools`.

### 10. Remote Level / Hotfix Infrastructure Is More Explicit Than It Looked

Evidence from database/metadata strings:

- `Game.Modules.RemoteLevel`
- `Game.Modules.RemoteLevel.PackData`
- `Path_folder_firestore`
- `Level_Max_Firestore`
- `Level_PackageHotfix_Version_Local`
- `Hotfix_Version_Firestore`
- `Path_Hotfix`
- `LEVEL_PACK_VERSION`
- `AB_Test_Path_Default`
- `Level_Package_Default`

Meaning:

The app likely supports content/config hotfixing through Firestore or Firebase-backed paths. APK levels are not the only content source.

ArrowMaze action:

- Build an original remote-level contract:
  - local bundled levels,
  - remote catalog version,
  - content hash,
  - rollback version,
  - signed/validated level pack,
  - offline fallback.

### 11. There Are Admin/Auth Clues

Evidence from metadata strings:

- `FirebaseAuthManager`
- `LoginAdmin`
- `SignInAnonymously`
- `IsLoggedIn`
- `email`

Meaning:

The app includes Firebase Auth-related code and names that suggest anonymous login and possibly admin/internal login paths. I did not confirm a reachable UI for admin login, so treat it as a code-level clue, not proof of an exposed admin screen.

ArrowMaze action:

- Keep admin tools out of the player build. If any admin login exists, make it server-side, role-gated, and absent from production UI.

### 12. Notification Infrastructure Has Both Local And Cloud Paths

Evidence:

- `Notification Data.asset` defines four daily notification time bands.
- Manifest includes Firebase Messaging services and Unity notification receivers.
- Manifest has `custom_notification_android_activity` pointing notifications to the Unity player activity.
- `UnityNotificationRestartReceiver` listens for boot completed.
- `com.unity.androidnotifications.exact_scheduling` is set to `0`.
- Metadata includes `Game.Entity.DB_PushNotification` and `Game.Modules.Notification.Android`.

Meaning:

They can use local scheduled notifications and Firebase cloud messaging. Boot receiver support means local notification schedules can survive device restart.

ArrowMaze action:

- Use notifications only after explicit opt-in. Add a test checklist for reboot, timezone changes, denied permission, Android 13+ permission flow, and unsubscribe behavior.

### 13. The "No Ads" Claim Is Explicitly Limited In UI Text

Evidence:

`RemoveAdsPanel.prefab` contains:

- `Remove Ads`
- `Remove obligatory ads`
- `Remove bottom banner ads`
- `Keep optional ads for rewards`
- product id `snakeescape_remove_ad`
- price text `$ 2.99`

Meaning:

The no-ads purchase is specifically framed as removing forced ads and bottom banner ads, while keeping rewarded ads. This was not only inferred from config; the UI says it.

ArrowMaze action:

- If you sell a no-ads product, use clear text like this but with original copy. The purchase should match the claim exactly.

### 14. Failure Screens Are Strongly Monetized

Evidence:

- `LevelFailedPanel` has:
  - `PurchaseRevive`,
  - `Refuse`,
  - `300 <sprite=0>`,
  - `+3`.
- `RevivePanel` has:
  - `WatchAdsAndContinue`,
  - `PurchaseRevive`,
  - `+1`,
  - `+3`,
  - `300 <sprite=0>`.
- `TimeOutPanel` has:
  - `WatchAdsAndContinue`,
  - `WatchAdsToRevive`,
  - `PurchaseRevive`,
  - `+15s`,
  - `+45s`,
  - `300 <sprite=0>`.

Meaning:

Failure is converted into three choices: watch ad, spend coins, or refuse. Time failure has a smaller ad/free extension and a larger coin extension.

ArrowMaze action:

- Decide your failure economy deliberately. Do not make every failure a monetization trap. Provide replay as a clear, non-monetized option.

### 15. Completion Screens Are Also Monetized

Evidence:

- `LevelCompletePanel` contains:
  - `OnDoubleRewardButtonClicked`,
  - `x2 Coin`,
  - `+40`,
  - `<sprite=0> 80`,
  - `<sprite=0> 40`,
  - `NextLevel`.
- Mode 4 completion panel contains `x2 Reward` and `x1`.

Meaning:

Win screens are used to upsell ad-watched reward doubling. Both loss and win states are ad opportunities.

ArrowMaze action:

- Keep reward-doubling optional and avoid blocking the next level behind reward prompts.

### 16. Store Has Two No-Ad Products And A Broad Price Ladder

Evidence from `StorePanel.prefab` and `HomePanel.prefab`:

- No Ads Bundle:
  - product `snakeescape_remove_ad_bundle`,
  - `$ 4.99`,
  - includes `3000`, `x3`, `x2`, `x3`.
- No Ads:
  - product `snakeescape_remove_ad`,
  - `$ 2.99`.
- Starter Pack:
  - product `snakeescape_starterpack`,
  - `$ 3.99`,
  - includes `1000`, `x1`, `x1`.
- Free claim:
  - `100`,
  - `Free (3/5)`.
- Coin packs:
  - `1000` for `$ 2.99`,
  - `5000` for `$ 7.99`,
  - `10000` for `$ 14.99`,
  - `25000` for `$ 29.99`,
  - `50000` for `$ 52.99`,
  - `100000` for `$ 100.99`.

Meaning:

The economy is built to anchor a high maximum spend while offering small starter/no-ad purchases. The $100.99 pack is a high anchor for a casual puzzle game.

ArrowMaze action:

- Use fewer SKUs at launch. Recommended starting SKUs:
  - Remove forced ads,
  - Starter pack,
  - Small/medium coin pack.
  Add larger packs only after retention and spend data justify them.

### 17. Freebie Limits Are Configured

Evidence:

- Database strings include:
  - `ShopFree_PurchaseMethod`,
  - `ShopFree_CoolDown`,
  - `ShopFree_MaxClaimPerDay`,
  - `ShopFreeReward`.
- UI text shows `Free (3/5)`.

Meaning:

The free shop reward is a controlled daily/limited loop, not a simple giveaway.

ArrowMaze action:

- If you add free claims, implement:
  - daily max,
  - cooldown,
  - clear reset time,
  - no exploit-prone client-only timer.

### 18. Difficulty And Review Timing Are Config Flags

Evidence:

- Database strings include:
  - `ReviewLevelConfig`,
  - `ReviewLevelCoolDownConfig`,
  - `level_difficulty_list`,
  - `level_diff_enable`,
  - `critical_region_level`,
  - `lv_start_show_level_name`,
  - `show_level_name`.
- UI includes `Hard` and `Superhard` states in `HomePanel.prefab`.

Meaning:

Difficulty labels and review prompts are controlled by config. The game can choose when to tell players a level is hard and when to ask for a review.

ArrowMaze action:

- Tag levels with difficulty during content creation. Use review prompts only after positive sessions, not after ad exposure or loss.

### 19. Addressables Catalog Updates Are Not Disabled

Evidence:

- `assets/aa/settings.json` says:
  - `m_DisableCatalogUpdateOnStart: false`,
  - Addressables version `2.7.6`,
  - max concurrent web requests `3`,
  - local catalog path `{RuntimePath}/catalog.bin`.
- The catalog hash is `e79f...d282` redacted here.

Meaning:

Addressables is configured in a way that can support catalog update checks, even though the static settings point to a local catalog. Combined with the app's own Firestore/hotfix strings, content can be more dynamic than the APK asset list suggests.

ArrowMaze action:

- Use Addressables for clean content packs, but design a real versioning policy. Never silently break older saves with remote content.

### 20. The APK Carries A Full Native IL2CPP And SDK Footprint

Evidence:

- Native split includes:
  - `libil2cpp.so` around `136.9 MB`,
  - `libunity.so` around `22.8 MB`,
  - Firebase C++ native libraries,
  - Crashlytics native libraries,
  - AppLovin native crash reporter,
  - Lofelt haptics,
  - Pangle/native ad-related libraries.
- `global-metadata.dat` is around `20.1 MB`.

Meaning:

The build is large because of Unity, IL2CPP, Firebase, ads, haptics, and many packages. This affects install size, memory, startup time, and low-end device performance.

ArrowMaze action:

- Track build size from day one. Keep optional SDKs modular, and test cold-start time on low-end Android devices.

### 21. Sample/Debug Packages Are Present In Release Assemblies

Evidence from `ScriptingAssemblies.json`:

- `ACTk.Examples.Genuine.Runtime.dll`
- `ACTk.Examples.Runtime.dll`
- `Unity.PlayerPrefsEditor.Samples.SampleScene.dll`
- `NaughtyAttributes.Test.dll`
- `SingularityGroup.HotReload.Runtime.Public.dll`
- `StompyRobot.SRDebugger.dll`
- `Unity.Recorder.dll`
- `Unity.TestFramework` dependency appears in performance info.

Meaning:

The release build carries sample/test/debug/hot-reload related assemblies. They may be harmless or stripped at runtime, but they are still a production hygiene issue.

ArrowMaze action:

- Create a release-build exclusion checklist:
  - no sample assemblies,
  - no test packages,
  - no recorder,
  - no hot reload,
  - no debug menu,
  - no internal editor helpers.

### 22. SRDebugger Is Disabled But Auto-Initialized

Evidence:

- `Resources/srdebugger/Settings.asset` has `_isEnabled: 0`.
- It also has `_requireEntryCode: 1`.
- `RuntimeInitializeOnLoads.json` includes:
  - `SRDebugger.AutoInitialize.OnLoadBeforeScene`,
  - `SRDebugger.AutoInitialize.OnLoad`.
- Addressables link preserves `Game.Modules.SRDebugger`.

Meaning:

The debug tool is disabled by settings but still present and initialized by runtime code. It is not visible proof of a vulnerability, but it is production baggage.

ArrowMaze action:

- Strip debug frameworks from release builds. A disabled debug panel is still something to audit.

### 23. Anti-Cheat / Protection Is Present But Not A Security Solution By Itself

Evidence:

- `ACTk.Runtime.dll`
- `Tangle.dll`
- metadata strings:
  - `EncryptedSaveService`,
  - `add_CheatDetected`,
  - `add_CheatChecked`,
  - `add_OnAlterationDetected`,
  - `add_OnPossibleForeignSavesDetected`,
  - `Obscured` style strings.

Meaning:

They likely protect saves, keys, or client data against casual tampering. It does not make client economy secure by itself.

ArrowMaze action:

- For offline puzzle games, protect local saves lightly but do not trust client state for purchases, leaderboards, or server rewards.

### 24. Firebase Messaging, Auth, Storage, Firestore, Remote Config, And App Check Are All Registered

Evidence:

Manifest registers Firebase components for:

- Auth,
- Analytics connector,
- Firestore,
- Messaging,
- Storage,
- App Check,
- Remote Config,
- Crashlytics,
- Sessions,
- Installations,
- ABT,
- Datatransport.

Meaning:

Firebase is not only crash reporting. It is a full backend/live-ops stack.

ArrowMaze action:

- Add Firebase modules only as needed. Crashlytics and Analytics may be enough at launch; Remote Config and Firestore should come with rules, versioning, and quota planning.

### 25. There Is A Play Review / Rate Pipeline

Evidence:

- `review.properties` says Play Review client `2.0.0`.
- Package list includes Google Play Review.
- Metadata strings include:
  - `Game.Modules.AppReview`,
  - `af_rate`,
  - `fb_mobile_rate`,
  - `get_rate`.
- UI has `prefab-ui-popup-review.prefab` and `RateStar.prefab`.

Meaning:

Review prompts are part of the growth funnel and analytics.

ArrowMaze action:

- Implement review prompts as a gated service:
  - after wins,
  - after several sessions,
  - cooldown,
  - no prompt after loss,
  - no prompt immediately after an ad.

### 26. The Game Has Old/New Input Or Gesture Systems

Evidence:

- `GestureHandler.prefab` has children `Old` and `New`.
- Addressables link preserves:
  - `GestureHandler`,
  - `GestureSystem`,
  - `OldGestureHandler`,
  - `TracerMode`.
- Database strings include `GestureMode`.

Meaning:

They likely A/B tested or migrated gesture/tap behavior. The current player-facing control may hide alternate input code.

ArrowMaze action:

- Keep gesture handling behind a strategy interface so you can tune tap/drag behavior without rewriting gameplay.

### 27. Medical/Brain Claims Appear In Notification Copy

Evidence:

- Notification copy includes "Push back Alzheimer!" and "15 minutes a day to keep your brain sharp!"

Meaning:

This is a risky marketing/retention angle. It may help engagement but can create compliance and platform-review issues if not substantiated.

ArrowMaze action:

- Avoid medical or disease-related claims. Use neutral copy like "daily puzzle ready" or "new challenge available."

### 28. Their Ad Network Config Has More Networks Than Static Active Flags Show

Evidence:

- `AdNetworkConfig.asset` has four network config entries, only one statically active in the dumped asset.
- Manifest/resources include many SDKs anyway.
- AppLovin MAX mediation adapters list many networks across Android/iOS.
- Database strings include MAX/AdMob/IronSource/Appsflyer/Adjust/Facebook-style event names.

Meaning:

Static active flags are not the whole story. Remote config or mediation waterfalls can route traffic even if one local config looks active.

ArrowMaze action:

- Keep a provider matrix: SDK present, manifest active, remote enabled, Play Console disclosed, consent covered, tested.

### 29. Build And Runtime Files Reveal Internal Pipeline Hygiene Gaps

Evidence:

- `Jenkinsfile.txt` is shipped.
- `config.txt` includes:
  - Jenkins build method,
  - Slack channel `ci`,
  - `TeamName=DungeonLabs`,
  - `GameID=WoodNuts`,
  - empty keystore fields.
- `BuildInfo.txt` includes build branch/user/version/hash/time.
- `PerformanceTestRunInfo.json` includes a long package/dependency list.
- `builddatas.json` includes report keys for `IPlayTempleView`.

Meaning:

The production package leaks internal build metadata and template leftovers. These are not necessarily security secrets, but they expose operational history and tooling.

ArrowMaze action:

- Strip CI files and internal configs from production. Keep build info only if you intentionally display or log it.

### 30. "Exactly Same" Is The Wrong Target

Evidence:

The competitor's moat is a combination of:

- 1400+ main levels,
- encoded/protected level packs,
- live remote level/hotfix hooks,
- staged ad pressure,
- reward ads at win/loss points,
- high SKU price ladder,
- push notifications,
- review prompts,
- analytics/attribution,
- ad revenue logging,
- broad mediation,
- anti-cheat/save protection,
- event modes,
- localization bundles.

Meaning:

Copying only the gameplay will not reproduce the business system. Copying their exact assets, levels, IDs, or package identity creates legal and store-review risk.

ArrowMaze action:

- Build equivalent systems cleanly:
  - original grid puzzle logic,
  - original level editor/generator,
  - original assets,
  - your own package/SDK IDs,
  - your own levels,
  - simpler monetization first,
  - live-ops only after the base game is stable.

## Priority Build Plan For ArrowMaze

### Phase 1: Core Game Parity

- Implement clean grid-board model.
- Implement snakes/arrows with path validation.
- Implement blockers, black holes, portals, direction changers, pipes, gates, king snake, warning lines.
- Implement fail reasons: blocked path, time out, lives out.
- Build level schema with version number and validation.
- Build level editor/importer with difficulty tags.

### Phase 2: Content Pipeline

- Create original main saga levels.
- Create mode/event packs separately:
  - challenge,
  - rescue,
  - winter/seasonal.
- Add automated checks:
  - solvability,
  - time target,
  - snake count,
  - object count,
  - difficulty band,
  - duplicate layout detection.

### Phase 3: Economy And UI

- Add coins, hints, ruler/grid booster, revive/time extension.
- Add free daily claim with cooldown and max claims.
- Add transparent no-ads product text.
- Add small SKU set only:
  - no forced ads,
  - starter pack,
  - two coin packs.
- Avoid high anchor packs until you have real player data.

### Phase 4: Ads And Consent

- Start with one mediation provider.
- Define placements:
  - interstitial after win/replay,
  - optional rewarded double reward,
  - optional revive/time extension,
  - optional daily video bar.
- Build consent before ads:
  - GDPR,
  - CCPA,
  - Android ad services,
  - store data safety.
- Make no-ads behavior explicit and testable.

### Phase 5: Analytics And Live Ops

- Define one event schema:
  - level_start,
  - level_complete,
  - level_fail,
  - booster_used,
  - ad_impression,
  - ad_reward,
  - purchase_start,
  - purchase_success,
  - review_prompt,
  - notification_open.
- Add Remote Config only for:
  - feature toggles,
  - ad cooldowns,
  - event mode gates,
  - review prompt gates.
- Add remote levels only after local content is stable.

### Phase 6: Release Hygiene

- Strip debug/sample/test/hot-reload assemblies.
- Remove CI/Jenkins files from Resources.
- Redact build metadata.
- Use separate dev/staging/prod configs.
- Verify manifest permissions and Play Console declarations.
- Test install size, cold start, memory, offline play, notification permission denial, and purchase restore.

## Final Clean-Room Rule

Use this competitor research to understand the system shape. Do not import their content. ArrowMaze should compete by recreating the player-facing category with original implementation, safer monetization, smaller SDK footprint, and a better-controlled release pipeline.
