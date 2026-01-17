---
title: Portal Construction
status: review
modifies: systems/worlds/portals.md
priority: P1
author: AI
created: 26-01-17
---

# Portal Construction

Build portals to travel between worlds.

## Summary

Implement the portal system that allows players to construct portals to unlock new worlds and travel between previously completed worlds.

## Design

### Portal

```csharp
public partial class Portal : Area3D
{
    [Export] public int DestinationWorldId;
    [Export] public Dictionary<ResourceType, int> ConstructionCost;

    private Dictionary<ResourceType, int> _paidAmount = new();
    private bool _isComplete;
    private bool _isActive;

    [Signal] public delegate void ConstructionProgressEventHandler(float percent);
    [Signal] public delegate void PortalCompletedEventHandler(int destinationWorld);

    public float GetProgress();
    public void DeliverResources(Dictionary<ResourceType, int> resources);
    public void Activate();
}
```

### Portal Construction Site

```csharp
public partial class PortalConstructionSite : Node3D
{
    [Export] public Portal PortalPrefab;
    [Export] public Dictionary<ResourceType, int> TotalCost;

    private Node3D _constructionVisual;
    private Control _costUI;

    public void OnPlayerApproach();
    public void ShowProgress();
    public void OnResourcesDelivered(Dictionary<ResourceType, int> resources);
    public void CompleteConstruction();
}
```

### World-Specific Portal Costs

```csharp
public static class PortalCosts
{
    public static Dictionary<ResourceType, int> GetCost(int fromWorld) => fromWorld switch
    {
        0 => new() {  // Hades -> Gaia
            { ResourceType.Rock, 50 },
            { ResourceType.SourceCore, 1 }
        },
        1 => new() {  // Gaia -> Trollheim
            { ResourceType.Wood, 100 },
            { ResourceType.Steel, 50 },
            { ResourceType.SourceCore, 2 }
        },
        2 => new() {  // Trollheim -> Dimidium
            { ResourceType.Coal, 100 },
            { ResourceType.Leather, 50 },
            { ResourceType.SourceCore, 3 }
        },
        // ... etc
        _ => new()
    };
}
```

### Travel System

```csharp
public partial class WorldManager : Node
{
    public int CurrentWorldId { get; private set; }
    public List<int> UnlockedWorlds { get; private set; } = new() { 1 };

    public void TravelToWorld(int worldId);
    public bool CanTravelTo(int worldId);
    public void UnlockWorld(int worldId);
}
```

### World Map UI

```csharp
public partial class WorldMapUI : Control
{
    [Export] public WorldMapNode[] WorldNodes;

    public void OnWorldSelected(int worldId);
    public void UpdateWorldStates();
}

public partial class WorldMapNode : Control
{
    public int WorldId;
    public bool IsUnlocked;
    public bool IsCurrent;
    public Texture2D WorldIcon;
    public string WorldName;
}
```

### Portal Transition

```csharp
private async void TransitionToWorld(int worldId)
{
    // Fade out
    await screenFade.FadeOut();

    // Load world scene
    var worldScene = ResourceLoader.Load<PackedScene>($"res://scenes/worlds/World{worldId}.tscn");

    // Unload current world
    currentWorld.QueueFree();

    // Instantiate new world
    currentWorld = worldScene.Instantiate<World>();
    AddChild(currentWorld);

    // Position player at spawn
    player.GlobalPosition = currentWorld.SpawnPoint.GlobalPosition;

    // Fade in
    await screenFade.FadeIn();

    CurrentWorldId = worldId;
    EmitSignal(SignalName.WorldChanged);
}
```

## Visual Design

- Incomplete portal: Scaffolding with resource deposits
- Complete portal: Glowing ring/gateway
- Active portal: Particle effects, swirling energy
- Travel animation: Whoosh into portal, fade transition

## Implementation Checklist

- [ ] Create Portal scene
- [ ] Create PortalConstructionSite
- [ ] Implement resource delivery to portal
- [ ] Add construction progress visualization
- [ ] Create portal activation effect
- [ ] Implement WorldManager
- [ ] Create World Map UI
- [ ] Implement scene transition
- [ ] Add travel effects
- [ ] Save/load portal states
- [ ] Define portal costs for each world
