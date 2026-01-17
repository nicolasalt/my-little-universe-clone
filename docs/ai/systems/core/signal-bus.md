# Signal Bus

Global event system for decoupled communication.

## Autoload
`SignalBus.cs`

## Global Signals

### Player Events
- `PlayerSpawned(Player player)`
- `PlayerDied()`
- `PlayerLeveledUp(int newLevel)`
- `BoosterCardOffered(List<BoosterCard> choices)`
- `BoosterCardSelected(BoosterCard card)`
- `BoosterCardLost(BoosterCard card)`

### Resource Events
- `ResourceGathered(ResourceType type, int amount)`
- `ResourcesChanged(Dictionary<ResourceType, int> resources)`
- `BackpackUpdated()`

### Territory Events
- `HexUnlocked(Vector2I hexCoord)`
- `HexUnlockStarted(Vector2I hexCoord, Dictionary<ResourceType, int> cost)`
- `AreaRevealed(Vector2I hexCoord)`

### Combat Events
- `EnemySpawned(Enemy enemy)`
- `EnemyDefeated(Enemy enemy, List<Drop> drops)`
- `BossDefeated(Boss boss)`
- `DamageDealt(Node target, int amount)`
- `DamageTaken(int amount)`

### Progression Events
- `ToolUpgraded(ToolType tool, int newLevel)`
- `ArmorUpgraded(ArmorType armor, int newLevel)`
- `PortalConstructed(int worldId)`
- `WorldChanged(int fromWorld, int toWorld)`
- `FacilityBuilt(FacilityType type)`

### Facility Events
- `ProcessingStarted(FacilityType facility, ResourceType input)`
- `ProcessingCompleted(FacilityType facility, ResourceType output, int amount)`

## Changelog
- 26-01-17: Updated for My Little Universe design pivot
- 26-01-17: Initial design
