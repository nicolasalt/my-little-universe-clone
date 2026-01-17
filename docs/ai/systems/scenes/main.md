# Main Scene

Entry point and world container.

## Hierarchy

```
Main (Node3D)
├── WorldEnvironment
├── DirectionalLight3D
├── Ground (StaticBody3D)
│   ├── MeshInstance3D (PlaneMesh 50x50)
│   └── CollisionShape3D
├── Player (instance of Player.tscn)
└── HUD (instance of HUD.tscn)
```

## Changelog
- 26-01-17: Initial design
