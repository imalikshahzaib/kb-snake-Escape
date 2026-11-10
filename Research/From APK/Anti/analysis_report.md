# Competitor Analysis: Snake Escape (com.snake.escape.maze.slither.puzzle)

## 1. Technical Stack
- **Engine**: Unity 6 (Version: 6000.0.59f2) - (based on Addressables and modern package structures).
- **Animation**: 
  - **Spine (Esoteric Software)**: Used for the snakes' skeletal animation (skeletons found for `snake_face`, `obj_gate`, etc.).
  - **DOTween & LitMotion**: Used for tweening UI elements and possibly gameplay movements.
- **Data Management**: 
  - **BGDatabase (BansheeGZ)**: A heavy-duty database solution for Unity. Likely stores level metadata, shop items, and game configurations.
  - **Addressables**: Used for loading levels, prefabs, and UI assets dynamically to keep the initial APK size small.
- **Logic & Scripting**:
  - **UniTask**: High-performance async/await for Unity.
  - **R3**: A modern reactive extensions library (evolution of UniRx) used for event-driven gameplay.
  - **Sirenix (Odin Inspector)**: Used for advanced editor tooling and data serialization.
- **Utilities**:
  - **MoreMountains.Tools**: Likely used for haptics, feedback, or general utility functions.
  - **NiceVibrations**: Dedicated plugin for high-quality haptic feedback.
  - **SRDebugger**: On-device console for debugging (found `SROptions.cs`).

## 2. Gameplay Architecture (Prefabs)
Analysis of `Snake.prefab` and other gameplay objects reveals:
- **Snake Structure**:
  - **Main Controller**: Attached to the root, handles state and logic.
  - **Renderer**: Separated from logic, likely handles Spine animations and sprite swapping.
  - **Colliders**: Uses `EdgeCollider2D` for the snake body and `CircleCollider2D` for the tail/head triggers.
  - **Tracer**: A child object named "Tracer" likely handles pathfinding or input visualization.
- **Grid System**: 
  - A `Grid.prefab` exists, suggesting a logic-based grid movement rather than pure physics.
- **Interactive Objects**:
  - `Portal.prefab` and `BlackHole.prefab`: Teleportation and obstacle mechanics.
  - `DirectionChanger.prefab`: Forces the snake to turn.
  - `WayBlocker.prefab` and `ToggleWall.prefab`: Dynamic obstacles.
  - `Pipe.prefab`: Likely a restricted path for the snake.

## 3. Level System
- **Location**: `Assets/Addressables/asset-game-level/`
- **Count**: 1400+ levels found (`lv0.json` to `lv1450.json`).
- **Format**: The JSON files are **Base64 encoded** or **Encrypted**. 
  - Example: `lv10.json` contains a large block of Base64 text.
  - This suggests the game logic decodes these strings at runtime into level data structures.
- **Progression**: Levels are split into multiple Addressable groups (`asset-game-level`, `asset-game-level-2`, etc.), allowing the game to download new levels on the fly.

## 4. UI Framework
The UI is modular and follows a standard mobile puzzle game flow:
- **HomePanel**: Main menu with shop and settings access.
- **GamePanel**: The HUD during gameplay (likely shows moves, level number).
- **LevelCompletePanel / LevelFailedPanel**: Standard win/loss screens with reward buttons.
- **Booster System**: Prefabs like `UnlockBoosterPanel` and `PurchaseBoosterPanel` indicate a monetization strategy involving power-ups.
- **Tutorials**: `PinchZoomTutorialPanel` and `TutorialPanel` suggest the game supports camera zooming and has an interactive walkthrough.

## 5. Monetization & Analytics
- **Ads**: Integrated with **AppLovin (MAX SDK)** and **Facebook (Audience Network)**.
- **Analytics**: Uses **Firebase Analytics** and **AppsFlyer**.
- **IAP**: Standard Unity Purchasing with a "Starter Pack" and "Remove Ads" options.

## 6. Recommendations for ArrowMaze
To replicate the "premium" feel of the competitor:
1. **Adopt a Grid-Logic System**: Don't rely on Unity Physics for movement. Use the grid for authority and tweens (DOTween/LitMotion) for visual smoothing.
2. **Use Addressables**: Even if not needed now, structuring levels as Addressables makes the project scalable.
3. **Enhance Feedback**: Use a library like `NiceVibrations` or `MoreMountains.Tools` for "juicy" tactile feedback.
4. **Animation Depth**: If sprites look too static, consider skeletal animation (Spine or Unity's 2D Animation package) for the snake/arrow heads to give them personality.
5. **Data-Driven Levels**: Encode your level JSONs if you want to prevent easy modification by players, but keep a clean C# class structure for the data itself.

---
*Research compiled by Antigravity AI on 2026-05-03.*
