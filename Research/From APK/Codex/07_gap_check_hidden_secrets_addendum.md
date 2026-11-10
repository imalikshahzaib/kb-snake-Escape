# Gap Check Addendum: Missing Hidden Secrets And Dark Truths

Source rechecked: `C:\Users\Muhammad Ali\Downloads\snakeescape`

Existing reports checked: `00` through `06` in `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Codex`

## Boundary

This is clean-room competitor research. Do not copy their package name, public client identifiers, SDK keys, product IDs, code, assets, audio, Spine files, exact level data, CI files, or endpoint identity. Sensitive identifiers seen in the APK are intentionally not reproduced here.

## Verdict

The previous reports already captured the main dark truths: staged ads, rewarded-ad pressure points, remote config, encoded level packs, broad SDK tracking, notification pressure, debug leftovers, CI leaks, and clone-risk warnings.

This recheck found additional missing details. They are mostly Android packaging, privacy, backup, licensing, WebView ad-runtime, localization, and release-hygiene details rather than new core gameplay systems.

## Newly Added Findings

### 1. The App Is Tracking-Heavy, But It Does Not Request Camera, Microphone, Location, Contacts, Or Storage

Evidence:

- Manifest permissions include internet, vibration, network state, advertising ID, Android ad services access, install referrer, billing, notifications, Firebase messaging, wake lock, Wi-Fi state, license check, and foreground service.
- A manifest scan found no camera, microphone, fine/coarse location, contacts, phone-state, or external-storage permissions.

Meaning:

The dark truth is not hidden microphone/camera access. The privacy burden comes from ads, attribution, ad services topics/custom audiences, Facebook/Firebase/AppLovin/AppsFlyer style tracking, notifications, billing, and install-referrer surfaces.

ArrowMaze action:

- Keep dangerous permissions out unless a feature truly needs them.
- Still treat ad/analytics permissions as high-risk privacy disclosures.

### 2. Facebook Automatic Event Logging And Advertiser ID Collection Are Enabled

Evidence:

- Manifest metadata includes Facebook SDK automatic app event logging enabled.
- Manifest metadata also enables Facebook advertiser ID collection.
- Facebook Unity login/dialog/app-link/deep-link activities and a Facebook content provider are registered.
- Facebook app/client identifiers are present in the APK but are not repeated here.

Meaning:

The Facebook integration is not just a login button or dormant SDK. It is configured to collect/forward app events and advertising identity signals unless runtime consent/config disables it.

ArrowMaze action:

- If Facebook SDK is added, decide explicitly whether automatic app events and advertiser ID collection are enabled.
- Make this match consent flow, privacy policy, and store data-safety declarations.

### 3. Android Backup Is Globally Enabled, But Only AppsFlyer Data Is Explicitly Excluded

Evidence:

- Manifest has `android:allowBackup="true"`.
- Manifest points to AppsFlyer-specific backup and data-extraction rules.
- Those rules exclude:
  - `sharedpref/appsflyer-data`
  - `sharedpref/appsflyer-purchase-data`
  - `database/afpurchases.db`
- No broad rule was found excluding all game save data or all purchase/economy preferences.

Meaning:

AppsFlyer attribution/purchase tracking is treated as special sensitive data, but normal app preferences and local save state may be eligible for Android cloud backup or device transfer unless separately protected at runtime.

ArrowMaze action:

- Decide what should transfer between devices:
  - settings and language can transfer,
  - local puzzle progress can transfer if intended,
  - purchase state should be restored from the store/server, not trusted only from backed-up PlayerPrefs.

### 4. Store/License Protection Is Deeper Than The Earlier Anti-Cheat Notes

Evidence:

- Manifest requests `com.android.vending.CHECK_LICENSE`.
- Manifest requests `com.google.android.providers.gsf.permission.READ_GSERVICES`.
- `integrity.properties` shows Play Integrity client `1.3.0`.
- `recaptcha.properties` shows reCAPTCHA client `18.6.1`.
- Firebase Auth registers exported Generic IDP and reCAPTCHA redirect activities using `genericidp://firebase.auth/` and `recaptcha://firebase.auth/`.
- Manifest includes `com.pairip.licensecheck.LicenseActivity` and `com.pairip.licensecheck.LicenseContentProvider`.

Meaning:

The client contains several anti-abuse and store-entitlement layers in addition to ACTk/Tangle/local save protection. This still does not make client-side economy secure, but it shows the production build is tied to Play distribution and Firebase/Auth anti-abuse plumbing.

ArrowMaze action:

- Use Play Integrity or equivalent only for your own package/signing identity.
- Never try to reuse competitor package identity, Firebase redirects, license providers, or store setup.

### 5. The Release Is Split-APK / ABI Dependent

Evidence:

- Manifest uses `requiredSplitTypes="base__abi"`.
- Manifest metadata says Google Play splits are required.
- Extracted resources include a base APK folder and an `arm64-v8a` config APK folder.
- Native libraries live in the `config.arm64_v8a.apk` split, including `libil2cpp.so`, `libunity.so`, Firebase C++ libs, Crashlytics libs, AppLovin native crash reporter, Lofelt haptics, and ad-network native libs.
- Manifest metadata includes a Play Store stamp source.

Meaning:

This is not a simple one-file Android package. Runtime depends on Play-style split delivery or an equivalent bundled install. A base-only sideload is not representative.

ArrowMaze action:

- Decide early whether ArrowMaze ships as AAB with ABI splits.
- Test installs through the same packaging path you will release with, not only local editor builds.

### 6. The Game Is Portrait, But Ad Surfaces Are Orientation-Flexible

Evidence:

- Main Unity activity is portrait and uses `com.google.firebase.MessagingUnityPlayerActivity`.
- Main Unity activity has `hardwareAccelerated="false"`.
- Application-level hardware acceleration is enabled.
- Manifest scan found 20 hardware-accelerated activities and 3 hardware-accelerated-false activities.
- Several ad activities allow `fullSensor`, `sensor`, `fullUser`, or landscape orientation.
- Unity metadata enables fullscreen launch, safe-area rendering outside normal bounds, notch support, and portrait/landscape notch config.

Meaning:

The playable game is portrait, but the ad/WebView stack can present fullscreen, landscape, sensor-driven, and hardware-accelerated surfaces. Ad SDKs can therefore create layout and lifecycle problems that the core game never sees.

ArrowMaze action:

- If ads are added, QA every placement through rotate, pause/resume, notch devices, split-screen/freeform, and low-end hardware.

### 7. A WebView / Playable-Ad Runtime Is Shipped Inside The APK

Evidence:

- Assets include MRAID bridge files, OMID/OMSDK scripts, endcard templates, and ad-viewer scripts.
- Manifest registers MRAID/playable/fullscreen ad activities from ByteDance/Pangle, Moloco, BidMachine, BIGO, Mintegral, Unity Ads, and other mediation stacks.
- `assets/audience_network.dex` is shipped as an additional ad SDK dex asset.
- `assets/builddatas.json` contains report hooks for `IPlayTempleView` methods such as alert show, alert dismiss, and play.

Meaning:

The ad system can render interactive HTML/WebView/playable ads and measure viewability/events. This is a separate runtime inside the game, not just static image/video ads.

ArrowMaze action:

- Treat playable ads as untrusted third-party web content.
- Keep WebView/ad SDK versions updated, test back-button/close behavior, and avoid exposing game internals to ad callbacks.

### 8. A Default Cross-Platform Input Action Asset Is Shipped

Evidence:

- `InputSystem_Actions.asset` contains a `Player` action map with `Move`, `Look`, `Attack`, `Interact`, `Crouch`, `Jump`, `Previous`, `Next`, and `Sprint`.
- Bindings include keyboard/mouse, touch, gamepad, joystick, and XR controller paths.
- Control schemes include keyboard/mouse, gamepad, touch, joystick, and XR.

Meaning:

This looks like Unity template/default input baggage or a reused action asset, not proof that the puzzle game truly supports XR/gamepad gameplay. It increases build metadata noise and can confuse future reverse analysis.

ArrowMaze action:

- Keep ArrowMaze input assets narrow:
  - touch/tap/drag first,
  - keyboard only for editor/debug if needed,
  - no XR/gamepad bindings unless actually supported.

### 9. Runtime Auto-Initialization Is Heavier Than The Main Game Suggests

Evidence:

- `RuntimeInitializeOnLoads.json` has 134 runtime initialization entries across 31 assemblies.
- 82 entries come from `MoreMountains.Tools`.
- Other auto-initialized systems include SRDebugger, Input System, Unity Services Core, Purchasing, Visual Scripting, Cinemachine, Addressables ResourceManager, LitMotion, UniTask, R3, UIEffect/UIParticle, Burst, and render-pipeline systems.
- `ScriptingAssemblies.json` lists 228 assemblies.

Meaning:

The APK carries and initializes many systems unrelated to a small grid puzzle. This can affect cold start, package size, memory use, crash surface, and QA burden.

ArrowMaze action:

- Keep startup lean.
- Add SDKs and visual frameworks only when a real shipped feature uses them.
- Track cold-start time and memory after each SDK/package addition.

### 10. Localization Support Is Broader In Packaging Than In Actual Content

Evidence:

- `splits0.xml` lists 75 language split entries.
- Addressables localization bundles include 21 localization bundle files.
- String tables are visible for English, French, German, Japanese, Korean, Portuguese, Russian, and Spanish.
- Asset localization bundles are visible for Japanese, Korean, Russian, and shared assets.

Meaning:

The Android package advertises a broad language split map, but the game content visible in the dump is closer to an 8-language string-table setup with fewer localized asset packs. Packaging-level language entries are not the same as true UX localization coverage.

ArrowMaze action:

- Define the exact launch locales you will truly support.
- Do not let dependency-generated split metadata imply languages that your UI, store listing, screenshots, and support process cannot handle.

### 11. Billing Is Production-Oriented But Also Carries Test/Phenotype Hooks

Evidence:

- `billing.properties` shows Billing Client `7.1.1`.
- Manifest queries Google Play billing and the Play billing test companion override service.
- `com_android_billingclient_phenotype.xml` registers BillingClient phenotype configuration.
- The earlier reports already found production IAP product strings and product panels.

Meaning:

Billing is not a simple purchase button. The build includes Google Billing runtime, test companion discovery, feature/config registration, product panels, purchase analytics, and no-ads/bundle/coin-pack economy hooks.

ArrowMaze action:

- Separate billing test and production tracks.
- Keep local purchase UI decoupled from product IDs so staging products cannot leak into release.

### 12. SafeDK Is The Application Class, Not Just A Passive Library

Evidence:

- Manifest application class is `com.safedk.android.SafeDKApplication`.
- SafeDK appears alongside many ad/attribution SDKs in the manifest/resource surface.

Meaning:

SafeDK wraps or monitors SDK behavior from application startup. This is part of ad/privacy/compliance instrumentation, not only a helper library.

ArrowMaze action:

- Do not add compliance-monitoring SDKs casually. They affect startup, manifest merge behavior, privacy disclosures, and troubleshooting.

## What Was Not Newly Found

- I did not find a new hidden core gameplay mode beyond the already documented main saga, mode 2, mode 3/challenge, rescue, and winter/xmas/event hooks.
- I did not find evidence of dangerous hardware permissions such as camera, microphone, location, contacts, or storage.
- I did not find actual private CI credential values in the text reviewed, only credential names/config references and internal build metadata already documented.
- I did not find a reason to copy any competitor asset, level, SDK key, identifier, or product ID. The clean-room rule still stands.

## Bottom Line

The missing dark truths are mostly operational: backup behavior, Facebook auto tracking, Play distribution/licensing protection, split APK dependency, playable-ad WebView runtime, overly broad input/runtime packages, and localization/package mismatch.

For ArrowMaze, the correct lesson is not to become "exactly same." The useful target is same-quality player-facing puzzle clarity, original levels, clean UI flow, controlled monetization, smaller SDK footprint, and stricter release hygiene.
