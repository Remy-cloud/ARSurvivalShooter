# AR Survival Shooter

A first-person mobile **Augmented Reality survival shooter** built with **Unity 6 (6000.4.7f1)** and **AR Foundation / ARKit** for iOS.

Scan the floor, tap to place the arena, and survive waves of **Melee** and **Shooter** enemies until the timer runs out.

**Author:** Roy Intwali

## Features (planned / in progress)
- [x] AR plane detection (horizontal) — AR Foundation + ARKit
- [ ] Custom plane visualizer showing the author's name
- [ ] Tap-to-place arena (single instance, plane detection stops after placement)
- [ ] First-person shooting with **object-pooled** projectiles
- [ ] Player health, score, damage feedback, game over
- [ ] Melee enemy & Shooter enemy (inheritance from `EnemyBase`)
- [ ] Enemy spawner (Factory pattern) on the AR plane
- [ ] Game states: Start → Play → End (State pattern)
- [ ] UI: Start menu, HUD (health / score / time), End screen
- [ ] Local leaderboard (last 5 sessions, persistent)
- [ ] Sound for all required gameplay events
- [ ] Difficulty levels (bonus)

## Requirements
- Unity 6000.4.7f1 with iOS Build Support
- macOS + Xcode
- ARKit-capable iPhone (tested on iPhone 16)

## Build (iOS)
1. Open the project in Unity Hub.
2. File → Build Profiles → iOS → Build.
3. Open the generated Xcode project, set your signing team, and run on device.

## Project structure
```
Assets/
  _Project/
    Scripts/   (Core, AR, Player, Enemies, Pooling, UI, Audio, Data)
    Prefabs/
    Materials/
    Audio/
    Scenes/
```
