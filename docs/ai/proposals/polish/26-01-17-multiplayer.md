---
title: Co-op Multiplayer
status: draft
modifies: null
priority: P4
author: AI
created: 26-01-17
---

# Co-op Multiplayer

Local split-screen co-op for up to 4 players.

## Summary

Implement local multiplayer allowing 2-4 players to play together with shared resources and independent booster cards.

## Design

### Multiplayer Manager

```csharp
public partial class MultiplayerManager : Node
{
    public const int MAX_PLAYERS = 4;

    public int PlayerCount { get; private set; } = 1;
    public List<Player> ActivePlayers { get; private set; }
    public SharedBackpack SharedResources { get; private set; }

    [Signal] public delegate void PlayerJoinedEventHandler(int playerId);
    [Signal] public delegate void PlayerLeftEventHandler(int playerId);

    public void AddPlayer();
    public void RemovePlayer(int playerId);
    public void SetupSplitScreen();
}
```

### Shared Resources

```csharp
public partial class SharedBackpack : Backpack
{
    // Same as single-player backpack
    // All players contribute and spend from same pool
}
```

### Player Input Mapping

```csharp
public class PlayerInputMap
{
    public static Dictionary<int, string> GetMappings(int playerId)
    {
        // Each player gets unique action names
        return new Dictionary<int, string>
        {
            { "move_up", $"p{playerId}_move_up" },
            { "move_down", $"p{playerId}_move_down" },
            { "move_left", $"p{playerId}_move_left" },
            { "move_right", $"p{playerId}_move_right" },
            { "attack", $"p{playerId}_attack" },
            { "ability", $"p{playerId}_ability" }
        };
    }
}

// Input configuration
// Player 1: WASD + Space/LMB
// Player 2: Arrow keys + Numpad 0
// Player 3: Controller 1
// Player 4: Controller 2
```

### Split Screen Viewports

```csharp
public partial class SplitScreenManager : Node
{
    [Export] public SubViewportContainer[] ViewportContainers;
    [Export] public SubViewport[] Viewports;
    [Export] public Camera3D[] Cameras;

    public void SetupLayout(int playerCount)
    {
        switch (playerCount)
        {
            case 1:
                // Full screen
                SetFullScreen(0);
                break;
            case 2:
                // Horizontal split
                SetHorizontalSplit();
                break;
            case 3:
            case 4:
                // Quad split
                SetQuadSplit(playerCount);
                break;
        }
    }

    private void SetHorizontalSplit()
    {
        ViewportContainers[0].SetAnchorsPreset(LayoutPreset.TopWide);
        ViewportContainers[0].Size = new Vector2(1920, 540);

        ViewportContainers[1].SetAnchorsPreset(LayoutPreset.BottomWide);
        ViewportContainers[1].Size = new Vector2(1920, 540);
        ViewportContainers[1].Position = new Vector2(0, 540);
    }
}
```

### Independent Booster Cards

```csharp
public partial class Player : CharacterBody3D
{
    public int PlayerId { get; set; }
    public BoosterCardManager BoosterCards { get; private set; }

    // Each player has their own booster card loadout
    // Each player loses their own card on death
}
```

### Shared Systems

```csharp
// What's shared:
- Backpack (resources)
- World progress (hexes, portals)
- Tool/armor upgrades
- Dungeon completion

// What's independent:
- Booster cards
- Health
- Position
- Death penalty
```

### Co-op Hazards

```csharp
public partial class FriendlyFireSystem : Node
{
    [Export] public bool FireStarterFriendlyFire = true;
    [Export] public bool VenomousFriendlyFire = true;

    public bool CanDamage(Player attacker, Node target)
    {
        if (target is Player otherPlayer)
        {
            // Only certain abilities cause friendly fire
            if (attacker.HasFireStarter || attacker.HasVenomous)
                return true;

            return false;
        }
        return true;
    }
}
```

### Player Join/Leave

```csharp
public void OnPlayerJoinPressed()
{
    if (PlayerCount >= MAX_PLAYERS) return;

    var newPlayer = SpawnPlayer(PlayerCount);
    ActivePlayers.Add(newPlayer);
    PlayerCount++;

    SetupSplitScreen();
    EmitSignal(SignalName.PlayerJoined, newPlayer.PlayerId);
}

public void OnPlayerLeavePressed(int playerId)
{
    if (PlayerCount <= 1) return;

    var player = ActivePlayers.Find(p => p.PlayerId == playerId);
    if (player != null)
    {
        ActivePlayers.Remove(player);
        player.QueueFree();
        PlayerCount--;

        SetupSplitScreen();
        EmitSignal(SignalName.PlayerLeft, playerId);
    }
}
```

### Remote Play Support

For online play via Steam Remote Play Together:
- Only host needs to own game
- Guests stream video and send inputs
- Works with existing local co-op implementation

## Implementation Checklist

- [ ] Create MultiplayerManager
- [ ] Implement split screen viewports
- [ ] Set up per-player input mappings
- [ ] Create player join/leave flow
- [ ] Implement shared backpack
- [ ] Keep booster cards independent
- [ ] Add player indicators (P1, P2, etc.)
- [ ] Handle camera following per player
- [ ] Implement friendly fire system
- [ ] Test 2, 3, and 4 player layouts
- [ ] Add controller support
