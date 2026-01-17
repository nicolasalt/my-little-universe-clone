---
title: Daily Tasks
status: draft
modifies: null
priority: P3
author: AI
created: 26-01-17
---

# Daily Tasks

Unlocks at World 5, provides daily objectives and rewards.

## Summary

Implement the daily task system that gives players daily objectives for Quest Token rewards.

## Design

### Task Manager

```csharp
public partial class DailyTaskManager : Node
{
    [Export] public int TasksPerDay = 3;

    public List<DailyTask> ActiveTasks { get; private set; }
    public DateTime LastRefreshTime { get; private set; }

    [Signal] public delegate void TasksRefreshedEventHandler();
    [Signal] public delegate void TaskCompletedEventHandler(DailyTask task);
    [Signal] public delegate void TaskProgressEventHandler(DailyTask task, int progress);

    public void RefreshTasks();
    public void CheckTaskProgress();
    public bool CanRefreshTasks();
}
```

### Task Data

```csharp
public enum TaskType
{
    GatherResource,
    DefeatEnemies,
    DefeatBoss,
    UnlockHexes,
    ClearDungeon,
    UpgradeEquipment,
    UseAbility
}

[GlobalClass]
public partial class DailyTaskTemplate : Resource
{
    [Export] public TaskType Type;
    [Export] public string DescriptionFormat;  // "Gather {0} {1}"
    [Export] public int MinTarget;
    [Export] public int MaxTarget;
    [Export] public int MinReward;
    [Export] public int MaxReward;
    [Export] public int MinWorld;  // Minimum world to generate
}

public class DailyTask
{
    public TaskType Type;
    public string Description;
    public int TargetAmount;
    public int CurrentProgress;
    public int QuestTokenReward;
    public ResourceType? TargetResource;
    public string TargetEnemyId;
    public bool IsComplete => CurrentProgress >= TargetAmount;
}
```

### Task Generation

```csharp
public List<DailyTask> GenerateTasks(int count)
{
    var tasks = new List<DailyTask>();
    var usedTypes = new HashSet<TaskType>();

    for (int i = 0; i < count; i++)
    {
        var template = SelectRandomTemplate(usedTypes);
        var task = CreateTask(template);
        tasks.Add(task);
        usedTypes.Add(template.Type);
    }

    return tasks;
}

private DailyTask CreateTask(DailyTaskTemplate template)
{
    int target = GD.RandRange(template.MinTarget, template.MaxTarget);
    int reward = GD.RandRange(template.MinReward, template.MaxReward);

    return new DailyTask
    {
        Type = template.Type,
        Description = FormatDescription(template, target),
        TargetAmount = target,
        QuestTokenReward = reward
    };
}
```

### Progress Tracking

```csharp
public void OnResourceGathered(ResourceType type, int amount)
{
    foreach (var task in ActiveTasks)
    {
        if (task.Type == TaskType.GatherResource &&
            task.TargetResource == type &&
            !task.IsComplete)
        {
            task.CurrentProgress += amount;
            EmitTaskProgress(task);
            CheckCompletion(task);
        }
    }
}

public void OnEnemyDefeated(Enemy enemy)
{
    foreach (var task in ActiveTasks)
    {
        if (task.Type == TaskType.DefeatEnemies &&
            !task.IsComplete)
        {
            task.CurrentProgress++;
            EmitTaskProgress(task);
            CheckCompletion(task);
        }
    }
}
```

### Task UI

```csharp
public partial class DailyTaskUI : Control
{
    [Export] public VBoxContainer TaskContainer;
    [Export] public Label RefreshTimerLabel;
    [Export] public Button RefreshButton;

    public void PopulateTasks(List<DailyTask> tasks);
    public void UpdateProgress(DailyTask task);
    public void ShowCompletionPopup(DailyTask task);
    public void UpdateRefreshTimer(TimeSpan remaining);
}

public partial class TaskDisplay : Control
{
    public Label DescriptionLabel;
    public ProgressBar ProgressBar;
    public Label RewardLabel;
    public TextureRect CompletedIcon;

    public void SetTask(DailyTask task);
    public void UpdateProgress(int current, int target);
}
```

### Example Tasks

```csharp
var taskTemplates = new[]
{
    new DailyTaskTemplate {
        Type = TaskType.GatherResource,
        DescriptionFormat = "Gather {0} Wood",
        MinTarget = 50,
        MaxTarget = 200,
        MinReward = 1,
        MaxReward = 3,
        MinWorld = 5
    },
    new DailyTaskTemplate {
        Type = TaskType.DefeatEnemies,
        DescriptionFormat = "Defeat {0} enemies",
        MinTarget = 10,
        MaxTarget = 50,
        MinReward = 2,
        MaxReward = 5,
        MinWorld = 5
    },
    new DailyTaskTemplate {
        Type = TaskType.DefeatBoss,
        DescriptionFormat = "Defeat any boss",
        MinTarget = 1,
        MaxTarget = 1,
        MinReward = 5,
        MaxReward = 10,
        MinWorld = 5
    },
    new DailyTaskTemplate {
        Type = TaskType.UnlockHexes,
        DescriptionFormat = "Unlock {0} hexes",
        MinTarget = 3,
        MaxTarget = 10,
        MinReward = 2,
        MaxReward = 4,
        MinWorld = 5
    }
};
```

### Quest Token Shop

```csharp
public partial class QuestTokenShop : Control
{
    [Export] public ShopItem[] Items;

    public void OnItemPurchased(ShopItem item);
}

public class ShopItem
{
    public string DisplayName;
    public Texture2D Icon;
    public int QuestTokenCost;
    public string UnlockId;  // Skin, decoration, etc.
}
```

## Implementation Checklist

- [ ] Create DailyTaskManager
- [ ] Define task templates
- [ ] Implement task generation
- [ ] Add progress tracking hooks
- [ ] Create task UI panel
- [ ] Add refresh timer (24h)
- [ ] Create completion popup
- [ ] Implement Quest Token rewards
- [ ] Create Quest Token shop
- [ ] Unlock at World 5 check
