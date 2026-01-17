---
title: Save/Load System
status: draft
modifies: null
priority: P1
author: AI
created: 26-01-17
---

# Save/Load System

Persistent game state storage.

## Summary

Implement comprehensive save/load system to persist all player progress, world state, and settings.

## Design

### Save Data Structure

```csharp
[Serializable]
public class SaveData
{
    public string Version = "1.0";
    public DateTime SaveTime;

    // Player
    public PlayerSaveData Player;

    // Worlds
    public Dictionary<int, WorldSaveData> Worlds;

    // Home World
    public HomeWorldSaveData HomeWorld;

    // Meta
    public SettingsSaveData Settings;
}

[Serializable]
public class PlayerSaveData
{
    public Dictionary<string, int> Backpack;
    public int CurrentWorldId;
    public Vector3 Position;
    public float Health;

    // Tools
    public int PickaxeLevel;
    public int AxeLevel;
    public int SwordLevel;

    // Armor
    public int HelmetLevel;
    public int ArmorLevel;

    // Experience
    public int Level;
    public float XP;

    // Booster Cards
    public string[] ActiveCards;

    // Stats
    public int TotalStars;
    public int SpentStars;
    public int QuestTokens;
}

[Serializable]
public class WorldSaveData
{
    public int WorldId;
    public bool IsUnlocked;
    public bool PortalComplete;
    public Dictionary<string, HexSaveData> Hexes;
    public Dictionary<string, bool> DungeonsCleared;
    public List<string> DefeatedBosses;
}

[Serializable]
public class HexSaveData
{
    public Vector2I Coord;
    public HexState State;
    public Dictionary<string, int> PaidResources;
}

[Serializable]
public class HomeWorldSaveData
{
    public Dictionary<string, TileSaveData> Tiles;
    public List<WorkerSaveData> Workers;
    public DateTime LastCollectionTime;
}
```

### Save Manager

```csharp
public partial class SaveManager : Node
{
    private const string SAVE_PATH = "user://saves/";
    private const string SAVE_EXTENSION = ".sav";
    private const int MAX_SAVE_SLOTS = 3;

    [Signal] public delegate void SaveStartedEventHandler();
    [Signal] public delegate void SaveCompletedEventHandler(bool success);
    [Signal] public delegate void LoadStartedEventHandler();
    [Signal] public delegate void LoadCompletedEventHandler(bool success);

    public SaveData CurrentSave { get; private set; }

    public void Save(int slot);
    public void Load(int slot);
    public void AutoSave();
    public bool HasSave(int slot);
    public SaveSlotInfo GetSlotInfo(int slot);
    public void DeleteSave(int slot);
}
```

### Serialization

```csharp
public void Save(int slot)
{
    EmitSignal(SignalName.SaveStarted);

    try
    {
        var data = GatherSaveData();
        var json = JsonSerializer.Serialize(data);

        var path = GetSavePath(slot);
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(json);

        EmitSignal(SignalName.SaveCompleted, true);
    }
    catch (Exception ex)
    {
        GD.PrintErr($"Save failed: {ex.Message}");
        EmitSignal(SignalName.SaveCompleted, false);
    }
}

public void Load(int slot)
{
    EmitSignal(SignalName.LoadStarted);

    try
    {
        var path = GetSavePath(slot);
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();

        CurrentSave = JsonSerializer.Deserialize<SaveData>(json);
        ApplySaveData(CurrentSave);

        EmitSignal(SignalName.LoadCompleted, true);
    }
    catch (Exception ex)
    {
        GD.PrintErr($"Load failed: {ex.Message}");
        EmitSignal(SignalName.LoadCompleted, false);
    }
}
```

### Auto-Save Triggers

```csharp
public partial class AutoSaveController : Node
{
    [Export] public float AutoSaveInterval = 60f;  // seconds

    private float _timer;

    public override void _Ready()
    {
        // Subscribe to important events
        SignalBus.HexUnlocked += OnHexUnlocked;
        SignalBus.PortalConstructed += OnPortalConstructed;
        SignalBus.ToolUpgraded += OnToolUpgraded;
        SignalBus.BossDefeated += OnBossDefeated;
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;
        if (_timer >= AutoSaveInterval)
        {
            _timer = 0;
            saveManager.AutoSave();
        }
    }

    // Also save on major events
    private void OnHexUnlocked(Vector2I coord) => saveManager.AutoSave();
    private void OnPortalConstructed(int worldId) => saveManager.AutoSave();
}
```

### Save Slot UI

```csharp
public partial class SaveSlotUI : Control
{
    [Export] public SaveSlotPanel[] Slots;
    [Export] public Button NewGameButton;
    [Export] public Button LoadButton;
    [Export] public Button DeleteButton;

    public void PopulateSlots();
    public void OnSlotSelected(int slot);
    public void OnNewGame(int slot);
    public void OnLoad(int slot);
    public void OnDelete(int slot);
}

public partial class SaveSlotPanel : Control
{
    public int SlotNumber;
    public Label WorldLabel;        // "Odysseum - World 6"
    public Label PlayTimeLabel;     // "12:34:56"
    public Label DateLabel;         // "Jan 17, 2026"
    public TextureRect Screenshot;

    public void SetSlotInfo(SaveSlotInfo info);
    public void SetEmpty();
}
```

### Migration Support

```csharp
public class SaveMigrator
{
    public SaveData Migrate(SaveData oldData)
    {
        // Handle version upgrades
        if (oldData.Version == "0.9")
        {
            oldData = MigrateFrom09(oldData);
        }

        oldData.Version = "1.0";
        return oldData;
    }
}
```

## Implementation Checklist

- [ ] Create SaveData classes
- [ ] Implement SaveManager
- [ ] Add JSON serialization
- [ ] Create auto-save system
- [ ] Implement save slot UI
- [ ] Add save/load indicators
- [ ] Create backup before overwrite
- [ ] Add save migration system
- [ ] Test save corruption handling
- [ ] Implement cloud save (optional)
