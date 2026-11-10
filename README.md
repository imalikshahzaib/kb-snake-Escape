# Arrow Maze - High Performance ECS Template

This template is built using a lightweight **ECS (Entity Component System)** architecture, designed for high performance and scalability. It is heavily inspired by Entitas and ready for a full Entitas integration if desired.

## Core Features
1. **ECS Pathfinding & Movement**: Grid-based entities (Arrows, Snakes, Obstacles) with collision detection.
2. **Data-Driven Levels**: Levels are stored as `.json` TextAssets in `Assets/Resources/Levels`.
3. **Custom Level Editor**: Access via `Tools > Arrow Maze > Level Editor` to design levels in-engine.
4. **Tween Bridge**: Decoupled animation logic. Ready for **DOTween** (just import DOTween and update `TweenBridge.cs`).
5. **Meta Systems**: Sequential level unlocking, currency tracking, and service placeholders (Ads/IAP).
6. **Themes**: ScriptableObject-based theme system for Neon, Basic, Aztec, etc.

## Setup Instructions
1. **Import DOTween**: This template uses a `TweenBridge` fallback, but for premium animations, import DOTween from the Asset Store.
2. **Setup Prefabs**:
   - Create 3 Prefabs: `Arrow`, `Snake`, `Obstacle`.
   - Attach `EntityView.cs` to each.
   - For `Snake`, add a child GameObject as the body segment and reference it in `EntityView`.
3. **Configure GameManager**:
   - Create an empty GameObject in your scene named `GameManager`.
   - Attach `GameManager.cs` and assign your prefabs.
4. **Themes**:
   - Create a `ThemeData` asset in `Assets/Resources/Themes` via `Right Click > Create > Arrow Maze > Theme`.

## Level Editor
- Use the in-game level editor scene to set grid size and place entities.
- Click **SAVE LEVEL** or **SAVE NEXT LEVEL** to save your layout as JSON in `Assets/Resources/Levels`.

## ECS Architecture
The game logic resides in `Assets/Scripts/Systems/MovementSystem.cs`. No Unity-specific logic is used here, ensuring high performance. Visuals are synced in `GameManager.cs`.
