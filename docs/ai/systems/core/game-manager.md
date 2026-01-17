# Game Manager

Global game state management for My Little Universe clone.

## Autoload
`GameManager.cs`

## Responsibilities
- Track game state (Playing, Paused, WorldMap)
- Handle pause/unpause
- Manage world transitions via portals
- Store reference to current player
- Track current world ID
- Manage hex unlock state

## States

```csharp
enum GameState { MainMenu, Playing, Paused, WorldMap, Upgrading, Trading }
```

## Properties

```csharp
int CurrentWorldId = 0;
Dictionary<int, WorldProgress> WorldProgressData;
```

## Changelog
- 26-01-17: Updated for My Little Universe design pivot
- 26-01-17: Initial design
