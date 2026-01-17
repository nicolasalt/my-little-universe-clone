# Player Controller

Movement, input handling, and player state.

## Scene
`scenes/characters/Player.tscn`

## Root Node
CharacterBody3D

## Components
- Player.cs - Movement, input handling, state
- Camera3D (child) - Third-person follow camera
- CollisionShape3D - Capsule collider
- MeshInstance3D - Placeholder capsule mesh

## Movement Specs

| Property | Value |
|----------|-------|
| Walk speed | 5 m/s |
| Sprint speed | 8 m/s |
| Jump velocity | 4.5 m/s |
| Gravity | Project default (9.8) |
| Mouse sensitivity | 0.3 |

## Changelog
- 26-01-17: Initial design
