# Folder Structure

Project organization for the 3D RPG.

## Layout

```
res://
├── scenes/
│   ├── main/              # Main.tscn - entry point
│   ├── characters/        # Player.tscn, enemies, NPCs
│   ├── levels/            # World scenes
│   └── ui/                # HUD.tscn, menus
├── scripts/
│   ├── player/            # Player.cs, PlayerCamera.cs
│   ├── systems/           # GameManager.cs, SignalBus.cs
│   ├── resources/         # PlayerStats.cs
│   └── ui/                # HUD.cs
├── resources/
│   └── stats/             # Default stat configurations
└── assets/
    ├── models/
    ├── textures/
    └── audio/
```

## Changelog
- 26-01-17: Initial design
