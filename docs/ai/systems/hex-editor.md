# Hex Editor System

Dev-time level design tool for editing hex grid maps.

## Overview

The Hex Editor is an in-game debug tool activated with F1 that allows placing resources, configuring
unlock costs, shaping the grid, and setting initial hex states. All changes are saved to Godot
Resource files (.tres).

## Activation

- Press `F1` to toggle editor mode on/off
- Game simulation pauses while editor is active
- Press `Tab` to switch between game camera and top-down orthographic view

## Controls

| Key/Action  | Function                                        |
| ----------- | ----------------------------------------------- |
| F1          | Toggle editor on/off                            |
| Tab         | Switch camera mode (game/top-down)              |
| WASD/Arrows | Pan camera (top-down mode)                      |
| Mouse Wheel | Zoom (top-down mode)                            |
| Left Click  | Select hex, place resource, or reposition spawn |
| Delete      | Remove selected spawn                           |
| Escape      | Cancel current mode / deselect                  |

## UI Elements

### Top Bar

- **Save**: Write map to `res://data/maps/hex_map.tres`
- **Load**: Reload map from file
- **New Hex**: Enter hex creation mode (click to add hexes)
- **Delete Hex**: Remove currently selected hex

### Selected Hex Panel

Appears when a hex is selected:

- **Coordinates**: Read-only display of hex position
- **Initial State**: Dropdown (Locked/Unlocked)
- **Start Hidden**: Checkbox for hiding hex even when adjacent to unlocked
- **Unlock Costs**: Wood and Stone spinboxes
- **Spawns List**: Click to select spawn for repositioning or deletion

### Resource Palette

- **None**: Clear resource placement mode
- **Tree/Rock**: Select resource type to place

## Workflows

### Placing Resources

1. Select resource type from palette (Tree/Rock)
2. Semi-transparent preview follows cursor
3. Click in hex to place resource
4. Resource added to hex's spawn list

### Repositioning Resources

1. Select a hex
2. Click a spawn in the Spawns List
3. Click new position within the same hex
4. Spawn moves to new location

### Deleting Resources

1. Select a hex
2. Click a spawn in the Spawns List
3. Press Delete key

### Creating New Hexes

1. Click "New Hex" button
2. Click anywhere to create hex at that grid position
3. Press Escape to cancel

## Data Files

### HexMapData (`res://data/maps/hex_map.tres`)

Root resource containing all hex configurations.

### HexSaveData

Per-hex configuration:

- Coordinates (Vector2I)
- InitialState (Locked/Unlocked)
- StartHidden (bool)
- UnlockCostWood/Stone (int)
- Spawns (Array of ResourceSpawnPoint)

## Code Location

- Main controller: `scripts/editor/HexEditor.cs`
- Scene: `scenes/editor/HexEditor.tscn`
- Data classes: `scripts/hex/HexMapData.cs`, `scripts/hex/HexSaveData.cs`

## Build Configuration

Editor code is conditionally compiled with `#if TOOLS || DEBUG` to exclude from release builds. Data
classes (HexMapData, HexSaveData) are always included since they're needed for loading maps.

## Changelog

### 2026-01-28

- Fix: Editor property changes (state, hidden, costs) now sync to runtime immediately
- Fix: Cost labels no longer disappear when editing unlock costs
- Fix: Hex creation/deletion operates on live grid (no save/reload needed)
- Added `AddTile`/`RemoveTile` public methods to HexGridManager for runtime manipulation

### 2026-01-18

- Initial implementation
- All phases complete: data layer, editor core, UI, interactions, polish
- Features: hex selection, resource placement, spawn repositioning, hover preview, save/load, new
  hex mode
