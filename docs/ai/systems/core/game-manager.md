# Game Manager

Global game state management.

## Autoload
`GameManager.cs`

## Responsibilities
- Track game state (Playing, Paused, Menu)
- Handle pause/unpause
- Manage scene transitions
- Store reference to current player

## States

```csharp
enum GameState { MainMenu, Playing, Paused, Dialogue, Inventory }
```

## Changelog
- 26-01-17: Initial design
