# Player Scene

Player character scene structure.

## Hierarchy

```
Player (CharacterBody3D)
├── CollisionShape3D (CapsuleShape3D)
├── MeshInstance3D (CapsuleMesh - placeholder)
├── CameraArm (Node3D) - pivot point
│   └── Camera3D
└── InteractionRayCast (RayCast3D)
```

## Changelog
- 26-01-17: Initial design
