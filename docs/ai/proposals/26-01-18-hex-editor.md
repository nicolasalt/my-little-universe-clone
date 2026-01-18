---
status: in-progress
modifies: []
creates: [systems/hex-editor.md]
priority: high
author: claude
created: 2026-01-18
---

# Hex Editor Tool

Dev-time level design tool for editing hex grid maps.

## Overview

In-game debug mode that allows placing resources, configuring unlock costs, shaping the grid, and
setting initial hex states. Saves to Godot Resource files.

## Activation

- Press `F1` to toggle editor mode on/off
- Editor overlay appears on top of the running game
- Game simulation pauses while editor is active

## Camera Modes

| Mode                  | Activation  | Controls                           |
| --------------------- | ----------- | ---------------------------------- |
| Game camera           | Default     | Normal third-person controls       |
| Top-down orthographic | Press `Tab` | WASD/arrows to pan, scroll to zoom |

## UI Layout

```
┌─────────────────────────────────────────────────────┐
│ [Save] [Load] [New Hex] [Delete Hex]    │ Top bar   │
├─────────────────────────────────────────────────────┤
│                                         │          │
│                                         │ Selected │
│         3D Viewport                     │ Hex      │
│         (click hex to select)           │ Panel    │
│         (click in hex to place resource)│          │
│                                         │ • Coords │
│                                         │ • State  │
│                                         │ • Costs  │
│                                         │ • Spawns │
├─────────────────────────────────────────────────────┤
│ [Tree] [Rock] [Future...]               │ Palette  │
└─────────────────────────────────────────────────────┘
```

### Top Bar

- **Save**: Write current map to `hex_map.tres`
- **Load**: Load existing map file
- **New Hex**: Enter hex-add mode (click empty space to create hex)
- **Delete Hex**: Enter hex-delete mode (click hex to remove)

### Selected Hex Panel

Appears when a hex is selected. Contains:

**Coordinates** (read-only)

- Display: `(2, -1)` in odd-q offset format

**Initial State** (dropdown)

- `Locked` — default, must be unlocked by player (visible when adjacent to unlocked)
- `Unlocked` — starts unlocked at game start
- `StartHidden` — explicitly hidden even if adjacent to unlocked; used for secrets or late-game areas

> **Note**: Current runtime behavior already hides locked hexes until adjacent to an unlocked hex. `StartHidden` is an *additional* flag for designer control over hexes that should remain hidden longer than normal adjacency rules would allow.

**Unlock Costs** (spinboxes)

- Wood: `[___]`
- Stone: `[___]`
- (Future resource types added here)

**Spawns List**

- Shows all resources placed in this hex
- Each entry: `[Icon] TreeName (0.5, -1.2) [X]`
- Click `[X]` to delete spawn
- Click entry to select it for repositioning

### Resource Palette

Bottom toolbar with available resource types:

- Buttons for each spawnable resource (Tree, Rock, etc.)
- Click to select, then click in hex to place
- Selected resource highlighted

## Interactions

### Selecting

- **Click hex** → Select hex, show properties in panel
- **Click resource** → Select resource for repositioning
- **Click empty space** → Deselect all
- **Escape** → Deselect / cancel current mode

### Placing Resources

1. Select resource type from palette
2. Click inside a hex to place at that position
3. Resource appears with preview while hovering
4. Position stored as local offset from hex center

### Repositioning Resources

- **Drag** selected resource to move within hex
- Constrained to hex boundaries
- Snap to grid optional (hold `Shift` for free placement)

### Deleting

- Select resource, press `Delete` key
- Or use delete mode for hexes

### Adding/Removing Hexes

- **New Hex mode**: Click empty grid position to create hex with default values
- **Delete Hex mode**: Click existing hex to remove (confirms if has spawns)

## Data Structures

### HexMapData (NEW Resource)

Root resource containing the entire map. New class to create.

```csharp
[GlobalClass]
public partial class HexMapData : Resource
{
    [Export] public Godot.Collections.Array<HexSaveData> Hexes { get; set; } = new();
}
```

### HexSaveData (NEW Resource)

Serialization-friendly hex configuration. Separate from runtime `HexTile` because:
- Uses explicit cost fields instead of Dictionary (Godot serializes these properly)
- Stores initial state, not runtime state (no `Unlocking` state)
- References spawn data directly

```csharp
[GlobalClass]
public partial class HexSaveData : Resource
{
    [Export] public Vector2I Coordinates { get; set; }
    [Export] public HexInitialState InitialState { get; set; } = HexInitialState.Locked;
    [Export] public int UnlockCostWood { get; set; }
    [Export] public int UnlockCostStone { get; set; }
    [Export] public bool StartHidden { get; set; }  // Override normal visibility rules
    [Export] public Godot.Collections.Array<ResourceSpawnPoint> Spawns { get; set; } = new();

    /// <summary>
    /// Convert to runtime HexTile with populated UnlockCost dictionary.
    /// </summary>
    public HexTile ToRuntimeTile()
    {
        var tile = new HexTile(Coordinates);
        tile.State = InitialState == HexInitialState.Unlocked ? HexState.Unlocked : HexState.Locked;
        if (UnlockCostWood > 0) tile.UnlockCost[ResourceType.Wood] = UnlockCostWood;
        if (UnlockCostStone > 0) tile.UnlockCost[ResourceType.Stone] = UnlockCostStone;
        return tile;
    }
}

public enum HexInitialState { Locked, Unlocked }
```

### ResourceSpawnPoint (EXISTING)

Reuse existing class at `scripts/resources/ResourceSpawnPoint.cs`. No changes needed.

```csharp
// Already exists with these fields:
[Export] public PackedScene ResourceScene;
[Export] public Vector2 LocalOffset;  // X = world X, Y = world Z
[Export] public float RotationY;      // Degrees
```

## File Location

Maps saved to: `res://data/maps/hex_map.tres`

## Integration with Existing Systems

### HexGridManager Changes

Add data-driven loading alongside existing procedural generation:

```csharp
public void LoadFromData(HexMapData data)
{
    ClearGrid();
    foreach (var hexData in data.Hexes)
    {
        var tile = hexData.ToRuntimeTile();
        CreateTileFromData(hexData.Coordinates, tile, hexData.StartHidden);
        // Store spawn data reference for ResourceSpawnManager
        _spawnData[hexData.Coordinates] = hexData.Spawns;
    }
}
```

**Loading priority:**
1. Check for `res://data/maps/hex_map.tres`
2. If exists → `LoadFromData()`
3. If not → `GenerateGrid()` (current procedural behavior)

### ResourceSpawnManager Changes

- Keep `InitializeSpawnDefinitions()` as fallback for procedural mode
- Add `LoadSpawnData(Dictionary<Vector2I, Array<ResourceSpawnPoint>> data)` for editor-defined spawns
- Spawning behavior unchanged: resources spawn when hex unlocks

### HexVisual Changes

- Add `StartHidden` property check in `Initialize()`
- If `StartHidden == true`, hex remains invisible even when adjacent to unlocked hex
- Reveal trigger: TBD (could be quest completion, item use, etc.)

## Build Configuration

Editor code should be conditionally compiled:

```csharp
#if TOOLS || DEBUG
public partial class HexEditor : Control { ... }
#endif
```

This prevents editor UI from shipping in release builds. The `HexMapData` and `HexSaveData` classes are always included since they're needed for loading maps.

## Keyboard Shortcuts

| Key      | Action                   |
| -------- | ------------------------ |
| `F1`     | Toggle editor mode       |
| `Tab`    | Switch camera mode       |
| `Delete` | Delete selected resource |
| `Escape` | Deselect / cancel mode   |
| `Ctrl+S` | Quick save               |
| `Ctrl+Z` | Undo (stretch goal)      |

## Implementation Checklist

### Phase 1: Data Layer
- [x] Create `HexMapData` Resource class
- [x] Create `HexSaveData` Resource class with `ToRuntimeTile()` conversion
- [x] Update `HexGridManager` with `LoadFromData()` method
- [x] Update `HexGridManager._Ready()` to check for map file first
- [x] Update `ResourceSpawnManager` to accept spawn data from HexGridManager

### Phase 2: Editor Core
- [x] Create `HexEditor` main controller (conditionally compiled)
- [x] Implement editor toggle (F1) with game pause via `GetTree().Paused`
- [x] Implement hex selection via raycasting
- [x] Implement top-down orthographic camera mode (Tab toggle)

### Phase 3: Editor UI
- [x] Build top bar UI (Save, Load, New Hex, Delete Hex buttons)
- [x] Build selected hex panel (coords, state dropdown, cost spinboxes, spawns list)
- [x] Build resource palette (Tree, Rock buttons)

### Phase 4: Editor Interactions
- [ ] Implement resource placement via raycasting
- [ ] Implement resource drag repositioning within hex bounds
- [x] Implement save to `.tres` file
- [x] Implement load from `.tres` file
- [ ] Add hex creation mode (click empty space)
- [x] Add hex deletion mode with confirmation

### Phase 5: Polish
- [ ] Add visual feedback for selected hex/resource
- [ ] Add hover preview when placing resources
- [ ] Add StartHidden support in HexVisual

## Future Enhancements

- **Undo/redo system** — Use command pattern: each edit creates an `IEditorCommand` with `Execute()` and `Undo()` methods, stored in a stack
- Copy/paste hexes
- Multi-select hexes
- Brush tools (paint multiple hexes)
- Import/export to JSON for external tools
- Prefab hex templates
