# AR Survival Shooter

A first-person **Augmented Reality survival shooter** for iOS, built with Unity and AR Foundation.
Scan the floor, tap to place the arena around you, and survive **90 seconds** against spiders and drones.

**Platform:** iOS (ARKit) · tested on iPhone

## How to play
1. Choose **Easy** or **Hard**.
2. Point the phone at the floor until the plane (with my name on it) appears, then tap to place the arena.
3. Tap anywhere to shoot. The dot shows where the barrel is aiming.
4. Survive until the timer reaches 0:00. If your health hits 0, it's game over.

## Features
- Horizontal plane detection with a custom plane texture; the arena is placed once and anchored
- First-person gun with **object-pooled** bullets, recoil and muzzle flash
- **Melee enemy** (spider): walks up and bites; dies in 2 hits
- **Shooter enemy** (drone): stops at range and fires pooled bullets; dies in 4 hits
- Health, score, damage flash, hit feedback, 90 s countdown, win/lose end screen
- Start menu, HUD, leaderboard screen and end screen (score, enemies defeated, time survived)
- Local leaderboard of the **last 5 games**, saved between sessions
- 5 gameplay sounds plus background music through one AudioManager
- Easy and Hard difficulty modes

## Design patterns
| Pattern | Where |
|---|---|
| Object Pool | `ProjectilePool` + `Projectile` |
| Singleton | `GameManager`, `AudioManager` |
| State | `GameState` (Menu → Placing → Playing → GameOver) in `GameManager` |
| Factory | `EnemyFactory` creates melee / shooter enemies |
| Observer | C# events used by the UI and audio (`StateChanged`, `HealthChanged`, `AnyKilled`, `Shot`, …) |

Enemies use inheritance and polymorphism: `EnemyBase` (abstract) → `MeleeEnemy`, `ShooterEnemy`, and anything that can be hit implements `IDamageable`.

## Project structure
```
Assets/
  Scripts/
    AR/        Arena, TapToPlaceArena
    Audio/     AudioManager
    Combat/    IDamageable, Projectile, ProjectilePool
    Core/      GameManager, GameState, Difficulty, Leaderboard
    Enemies/   EnemyBase, MeleeEnemy, ShooterEnemy, EnemyFactory, EnemySpawner
    Player/    PlayerHealth, PlayerShooter
    UI/        GameUI, CrosshairUI, DamageFlashUI
  Editor/      setup tools (Tools → AR Shooter)
  Prefabs/  Materials/  Textures/  Audio/  Scenes/ARGame.unity
```

## Build and run (iOS)
Requirements: Unity **6000.4.7f1** with iOS Build Support, macOS with Xcode, an ARKit iPhone.
1. Open the project in Unity Hub.
2. **File → Build Profiles → iOS → Build** and pick a folder.
3. Open `Unity-iPhone.xcodeproj` in Xcode, choose your signing team under **Signing & Capabilities**, and press **Run** with the iPhone connected.
