# HUD System

Heads-up display for MLU-style gameplay.

## Scene
`scenes/ui/HUD.tscn`

## Root Node
CanvasLayer

## Elements

### Top Bar
- Health bar (ProgressBar) - Shows current/max health
- XP bar (ProgressBar) - Shows progress to next level
- Level indicator (Label)

### Resource Display
- Resource counters (GridContainer)
- Shows relevant resources for current area
- Coin counter always visible

### Tool Display
- Current tool icon
- Tool level indicator
- Charge progress bar (when charging ability)

### Booster Card Display
- Active booster card icons (max 10 slots)
- Card tooltip on hover

### Minimap
- Current area overview
- Hex grid with fog of war
- Resource/enemy indicators
- Navigation arrows to objectives

### Interaction Prompts
- Unlock hex cost display
- Facility interaction prompts
- Portal construction progress

## Changelog
- 26-01-17: Updated for My Little Universe design
- 26-01-17: Initial design
