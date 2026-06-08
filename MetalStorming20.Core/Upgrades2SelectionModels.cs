namespace MetalStorming20.Core;

public sealed class Upgrades2AircraftLevelPlan
{
    public Dictionary<int, Upgrades2NodeSelectionState> LevelStates { get; } = [];

    public int CurrentAircraftLevel
    {
        get
        {
            var ownedLevels = LevelStates
                .Where(level => level.Value == Upgrades2NodeSelectionState.Owned)
                .Select(level => level.Key)
                .ToArray();

            return ownedLevels.Length == 0 ? 0 : ownedLevels.Max();
        }
    }

    public int TargetAircraftLevel
    {
        get
        {
            var targetLevels = LevelStates
                .Where(level => level.Value is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned)
                .Select(level => level.Key)
                .ToArray();

            return targetLevels.Length == 0 ? 0 : targetLevels.Max();
        }
    }

    public static Upgrades2AircraftLevelPlan FromLevels(int currentLevel, int targetLevel)
    {
        var plan = new Upgrades2AircraftLevelPlan();
        plan.LoadLevels(currentLevel, targetLevel);
        return plan;
    }

    public bool IsTarget(int level) =>
        StateFor(level) is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned;

    public Upgrades2NodeSelectionState StateFor(int level) =>
        LevelStates.TryGetValue(level, out var state) ? state : Upgrades2NodeSelectionState.Off;

    public void Cycle(int level)
    {
        level = Math.Clamp(level, 1, 20);
        var nextState = StateFor(level) switch
        {
            Upgrades2NodeSelectionState.Off when HasDesiredPrerequisite(level) => Upgrades2NodeSelectionState.Desired,
            Upgrades2NodeSelectionState.Off => Upgrades2NodeSelectionState.Owned,
            Upgrades2NodeSelectionState.Owned => Upgrades2NodeSelectionState.Desired,
            _ => Upgrades2NodeSelectionState.Off
        };

        if (nextState == Upgrades2NodeSelectionState.Off)
        {
            LevelStates.Remove(level);
        }
        else
        {
            LevelStates[level] = nextState;
        }

        NormalizeSelection();
    }

    public void LoadLevels(int currentLevel, int targetLevel)
    {
        LevelStates.Clear();

        currentLevel = Math.Clamp(currentLevel, 0, 20);
        targetLevel = Math.Clamp(targetLevel, currentLevel, 20);

        for (var level = 1; level <= currentLevel; level++)
        {
            LevelStates[level] = Upgrades2NodeSelectionState.Owned;
        }

        for (var level = currentLevel + 1; level <= targetLevel; level++)
        {
            LevelStates[level] = Upgrades2NodeSelectionState.Desired;
        }
    }

    public void NormalizeSelection()
    {
        var targetLevel = TargetAircraftLevel;
        var currentLevel = CurrentAircraftLevel;

        if (targetLevel == 0 && currentLevel == 0)
        {
            LevelStates.Clear();
            return;
        }

        if (targetLevel < currentLevel)
        {
            targetLevel = currentLevel;
        }

        for (var level = 1; level <= currentLevel; level++)
        {
            LevelStates[level] = Upgrades2NodeSelectionState.Owned;
        }

        for (var level = currentLevel + 1; level <= targetLevel; level++)
        {
            if (StateFor(level) == Upgrades2NodeSelectionState.Off)
            {
                LevelStates[level] = Upgrades2NodeSelectionState.Desired;
            }
        }

        foreach (var level in LevelStates.Keys.Where(level => level > targetLevel).ToArray())
        {
            LevelStates.Remove(level);
        }
    }

    private bool HasDesiredPrerequisite(int level)
    {
        for (var prerequisiteLevel = 1; prerequisiteLevel < level; prerequisiteLevel++)
        {
            if (StateFor(prerequisiteLevel) == Upgrades2NodeSelectionState.Desired)
            {
                return true;
            }
        }

        return false;
    }
}

public sealed class Upgrades2MasteryLevelPlan
{
    public Dictionary<int, Upgrades2NodeSelectionState> LevelStates { get; } = [];

    public int CurrentMasteryLevel
    {
        get
        {
            var ownedLevels = LevelStates
                .Where(level => level.Value == Upgrades2NodeSelectionState.Owned)
                .Select(level => level.Key)
                .ToArray();

            return ownedLevels.Length == 0 ? 0 : ownedLevels.Max();
        }
    }

    public int PlannedMasteryLevel
    {
        get
        {
            var targetLevels = LevelStates
                .Where(level => level.Value is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned)
                .Select(level => level.Key)
                .ToArray();

            return targetLevels.Length == 0 ? 0 : targetLevels.Max();
        }
    }

    public static Upgrades2MasteryLevelPlan FromLevels(int currentLevel, int plannedLevel)
    {
        var plan = new Upgrades2MasteryLevelPlan();
        plan.LoadLevels(currentLevel, plannedLevel);
        return plan;
    }

    public bool IsTarget(int level) =>
        StateFor(level) is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned;

    public Upgrades2NodeSelectionState StateFor(int level) =>
        LevelStates.TryGetValue(level, out var state) ? state : Upgrades2NodeSelectionState.Off;

    public void Cycle(int level)
    {
        level = Math.Clamp(level, 1, 24);
        var nextState = StateFor(level) switch
        {
            Upgrades2NodeSelectionState.Off when HasDesiredPrerequisite(level) => Upgrades2NodeSelectionState.Desired,
            Upgrades2NodeSelectionState.Off => Upgrades2NodeSelectionState.Owned,
            Upgrades2NodeSelectionState.Owned => Upgrades2NodeSelectionState.Desired,
            _ => Upgrades2NodeSelectionState.Off
        };

        if (nextState == Upgrades2NodeSelectionState.Off)
        {
            LevelStates.Remove(level);
        }
        else
        {
            LevelStates[level] = nextState;
        }

        NormalizeSelection();
    }

    public void LoadLevels(int currentLevel, int plannedLevel)
    {
        LevelStates.Clear();

        currentLevel = Math.Clamp(currentLevel, 0, 24);
        plannedLevel = Math.Clamp(plannedLevel, currentLevel, 24);

        for (var level = 1; level <= currentLevel; level++)
        {
            LevelStates[level] = Upgrades2NodeSelectionState.Owned;
        }

        for (var level = currentLevel + 1; level <= plannedLevel; level++)
        {
            LevelStates[level] = Upgrades2NodeSelectionState.Desired;
        }
    }

    public void NormalizeSelection()
    {
        var plannedLevel = PlannedMasteryLevel;
        var currentLevel = CurrentMasteryLevel;

        if (plannedLevel == 0 && currentLevel == 0)
        {
            LevelStates.Clear();
            return;
        }

        if (plannedLevel < currentLevel)
        {
            plannedLevel = currentLevel;
        }

        for (var level = 1; level <= currentLevel; level++)
        {
            LevelStates[level] = Upgrades2NodeSelectionState.Owned;
        }

        for (var level = currentLevel + 1; level <= plannedLevel; level++)
        {
            if (StateFor(level) == Upgrades2NodeSelectionState.Off)
            {
                LevelStates[level] = Upgrades2NodeSelectionState.Desired;
            }
        }

        foreach (var level in LevelStates.Keys.Where(level => level > plannedLevel).ToArray())
        {
            LevelStates.Remove(level);
        }
    }

    private bool HasDesiredPrerequisite(int level)
    {
        for (var prerequisiteLevel = 1; prerequisiteLevel < level; prerequisiteLevel++)
        {
            if (StateFor(prerequisiteLevel) == Upgrades2NodeSelectionState.Desired)
            {
                return true;
            }
        }

        return false;
    }
}

public sealed class Upgrades2SystemPlanRow
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public string SystemSlotId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int MaxSystemLevel { get; set; } = 8;
    public bool UsesBranches { get; set; } = true;
    public Dictionary<string, Upgrades2NodeSelectionState> NodeStates { get; } = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> TargetNodes => NodeStates
        .Where(node => node.Value is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned)
        .Select(node => node.Key)
        .ToArray();

    public IReadOnlyList<string> OwnedNodes => NodeStates
        .Where(node => node.Value == Upgrades2NodeSelectionState.Owned)
        .Select(node => node.Key)
        .ToArray();

    public int TargetSystemLevel
    {
        get
        {
            var selectedLevels = TargetNodes.Select(ParseLevel).Where(level => level > 0).ToArray();
            return selectedLevels.Length == 0 ? 0 : selectedLevels.Max();
        }
    }

    public static Upgrades2SystemPlanRow FromSlot(SystemSlotDefinitionV2 slot) =>
        new()
        {
            SystemSlotId = slot.SystemSlotId,
            DisplayName = slot.DisplayName,
            MaxSystemLevel = slot.MaxSystemLevel,
            UsesBranches = slot.UsesBranches
        };

    public bool IsTarget(int level, string? branchCode) =>
        StateFor(level, branchCode) is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned;

    public Upgrades2NodeSelectionState StateFor(int level, string? branchCode) =>
        StateFor(NodeKey(level, branchCode));

    public Upgrades2NodeSelectionState StateFor(string nodeKey) =>
        NodeStates.TryGetValue(nodeKey, out var state) ? state : Upgrades2NodeSelectionState.Off;

    public void Cycle(int level, string? branchCode)
    {
        var key = NodeKey(level, branchCode);
        var nextState = StateFor(key) switch
        {
            Upgrades2NodeSelectionState.Off => Upgrades2NodeSelectionState.Owned,
            Upgrades2NodeSelectionState.Owned => Upgrades2NodeSelectionState.Desired,
            _ => Upgrades2NodeSelectionState.Off
        };

        if (nextState == Upgrades2NodeSelectionState.Off)
        {
            NodeStates.Remove(key);
        }
        else
        {
            NodeStates[key] = nextState;
        }

        if (UsesBranches &&
            level >= 5 &&
            nextState is Upgrades2NodeSelectionState.Owned or Upgrades2NodeSelectionState.Desired &&
            !HasSelectedPrerequisitePath(level))
        {
            MarkPrerequisitePathAsOwned(level, branchCode);
        }

        NormalizeSelection();
    }

    public void NormalizeSelection()
    {
        var maxLevel = Math.Min(TargetSystemLevel, MaxSystemLevel);
        var trunkMaxLevel = UsesBranches ? Math.Min(maxLevel, 4) : maxLevel;
        for (var level = 1; level <= trunkMaxLevel; level++)
        {
            EnsureAtLeastDesired(level, null);
        }

        if (UsesBranches && maxLevel >= 5)
        {
            for (var level = 5; level <= maxLevel; level++)
            {
                if (!IsTarget(level, "A") && !IsTarget(level, "B"))
                {
                    EnsureAtLeastDesired(level, "A");
                }
            }
        }
    }

    public void LoadState(Upgrades2SavedSystemPlan savedSystem)
    {
        NodeStates.Clear();

        foreach (var node in savedSystem.SelectedNodes ?? [])
        {
            NodeStates[node] = Upgrades2NodeSelectionState.Desired;
        }

        if (savedSystem.NodeStates is null)
        {
            return;
        }

        foreach (var node in savedSystem.NodeStates)
        {
            NodeStates[node.Key] = node.Value.Equals("has", StringComparison.OrdinalIgnoreCase) ||
                node.Value.Equals("owned", StringComparison.OrdinalIgnoreCase)
                    ? Upgrades2NodeSelectionState.Owned
                    : Upgrades2NodeSelectionState.Desired;
        }
    }

    public Upgrades2SystemPlanInput ToPlannerInput() =>
        new(
            SystemSlotId,
            NodeStates
                .Where(node => node.Value is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned)
                .ToDictionary(node => node.Key, node => node.Value, StringComparer.OrdinalIgnoreCase));

    public Upgrades2SavedSystemPlan ToSavedSystemPlan() =>
        new(
            SystemSlotId,
            TargetNodes.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            NodeStates
                .Where(node => node.Value is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned)
                .OrderBy(node => node.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    node => node.Key,
                    node => node.Value == Upgrades2NodeSelectionState.Owned ? "has" : "desired",
                    StringComparer.OrdinalIgnoreCase));

    private void EnsureAtLeastDesired(int level, string? branchCode)
    {
        var key = NodeKey(level, branchCode);
        if (StateFor(key) == Upgrades2NodeSelectionState.Off)
        {
            NodeStates[key] = Upgrades2NodeSelectionState.Desired;
        }
    }

    private bool HasSelectedPrerequisitePath(int level)
    {
        if (level <= 1)
        {
            return true;
        }

        for (var trunkLevel = 1; trunkLevel <= Math.Min(level - 1, 4); trunkLevel++)
        {
            if (!IsTarget(trunkLevel, null))
            {
                return false;
            }
        }

        if (level <= 5)
        {
            return true;
        }

        for (var branchLevel = 5; branchLevel < level; branchLevel++)
        {
            if (!IsTarget(branchLevel, "A") && !IsTarget(branchLevel, "B"))
            {
                return false;
            }
        }

        return true;
    }

    private void MarkPrerequisitePathAsOwned(int level, string? branchCode)
    {
        for (var trunkLevel = 1; trunkLevel <= Math.Min(level - 1, 4); trunkLevel++)
        {
            NodeStates[NodeKey(trunkLevel, null)] = Upgrades2NodeSelectionState.Owned;
        }

        if (level <= 5)
        {
            return;
        }

        var branch = branchCode ?? "A";
        for (var branchLevel = 5; branchLevel < level; branchLevel++)
        {
            if (StateFor(branchLevel, "A") != Upgrades2NodeSelectionState.Owned &&
                StateFor(branchLevel, "B") != Upgrades2NodeSelectionState.Owned)
            {
                NodeStates[NodeKey(branchLevel, branch)] = Upgrades2NodeSelectionState.Owned;
            }
        }
    }

    private static string NodeKey(int level, string? branchCode) =>
        branchCode is null ? level.ToString() : $"{level}{branchCode}";

    public static int ParseLevel(string key) =>
        int.TryParse(key[..1], out var level) ? level : 0;

    public static string? ParseBranch(string key) =>
        key.Length > 1 ? key[1..].ToUpperInvariant() : null;
}
