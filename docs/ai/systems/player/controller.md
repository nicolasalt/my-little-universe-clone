# Player Controller

Movement, auto-gathering, and player state for MLU-style gameplay.

## Scene
`scenes/characters/Player.tscn`

## Root Node
CharacterBody3D

## Components
- Player.cs - Movement, auto-gather, tool switching, ability casting
- Camera3D (child) - Isometric/third-person follow camera
- CollisionShape3D - Capsule collider
- GatherArea (Area3D) - Detects nearby harvestable resources
- AttackArea (Area3D) - Melee attack hitbox
- MeshInstance3D - Orange stick figure character

## Movement Specs

| Property | Value |
|----------|-------|
| Walk speed | 5 m/s |
| Sprint speed | 8 m/s |
| Swim speed | 3 m/s (base) |
| Gravity | Project default (9.8) |

## Auto-Gather Behavior
- Standing near harvestable resources triggers automatic gathering
- GatherArea radius determines gather range
- Gather speed affected by tool level and Gatherer booster
- Player faces resource while gathering

## Tool Switching
- Automatic based on nearest resource type (Manual mode available)
- Pickaxe for rocks/ores
- Axe for trees/wood
- Sword for combat

## Charged Ability
- Hold attack to charge
- Release to trigger area-of-effect ability
- Each tool has unique ability effect

## Changelog
- 26-01-17: Updated for My Little Universe design pivot
- 26-01-17: Initial design
