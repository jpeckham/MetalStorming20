namespace MetalStorming20.Core;

public sealed class Upgrades2PlannerSession
{
    public IReadOnlyList<SystemSlotDefinitionV2> SystemSlots { get; private set; } = PlannerV2.GenericSystemSlots;
    public List<Upgrades2SystemPlanRow> SystemPlans { get; } = [];
    public List<Upgrades2SavedBuild> SavedBuilds { get; } = [];
    public Upgrades2AircraftLevelPlan AircraftLevelPlan { get; } = Upgrades2AircraftLevelPlan.FromLevels(5, 5);
    public Upgrades2MasteryLevelPlan MasteryPlan { get; } = Upgrades2MasteryLevelPlan.FromLevels(1, 1);
    public GoldMasteryStatus GoldMasteryStatus { get; private set; } = GoldMasteryStatus.Off;
    public string? SelectedBuildId { get; private set; }

    public int CurrentAircraftLevel => AircraftLevelPlan.CurrentAircraftLevel;
    public int TargetAircraftLevel => AircraftLevelPlan.TargetAircraftLevel;
    public string SelectedBuildName =>
        SavedBuilds.FirstOrDefault(build => build.Id == SelectedBuildId)?.Name ?? "unnamed";

    public void ResetSystemRows(IReadOnlyList<SystemSlotDefinitionV2> systemSlots)
    {
        SystemSlots = systemSlots.Count == 0
            ? PlannerV2.GenericSystemSlots
            : systemSlots;
        SystemPlans.Clear();
        SystemPlans.AddRange(SystemSlots.Select(Upgrades2SystemPlanRow.FromSlot));
    }

    public void LoadState(Upgrades2PlannerState state)
    {
        if (state.SchemaVersion != 2)
        {
            return;
        }

        ResetSelections();
        AircraftLevelPlan.LoadLevels(state.CurrentAircraftLevel, state.TargetAircraftLevel);
        MasteryPlan.LoadLevels(state.CurrentMasteryLevel, state.PlannedMasteryLevel);
        if (!string.IsNullOrWhiteSpace(state.GoldMasteryStatus) &&
            Enum.TryParse<GoldMasteryStatus>(state.GoldMasteryStatus, ignoreCase: true, out var parsed))
        {
            GoldMasteryStatus = parsed;
        }

        foreach (var savedSystem in state.SystemPlans ?? [])
        {
            var row = SystemPlans.FirstOrDefault(p => p.SystemSlotId == savedSystem.SystemSlotId);
            if (row is null)
            {
                continue;
            }

            row.LoadState(savedSystem);
            row.NormalizeSelection();
        }
    }

    public void LoadBuildCollection(Upgrades2SavedBuildCollection? collection)
    {
        SavedBuilds.Clear();
        SavedBuilds.AddRange(collection?.Builds ?? []);
        SelectedBuildId = SavedBuilds.Any(build => build.Id == collection?.SelectedBuildId)
            ? collection!.SelectedBuildId
            : SavedBuilds.FirstOrDefault()?.Id;

        if (SelectedBuildId is null)
        {
            EnsureSelectedBuild();
            return;
        }

        var selected = SavedBuilds.First(build => build.Id == SelectedBuildId);
        LoadState(selected.State);
    }

    public void EnsureSelectedBuild()
    {
        if (SavedBuilds.Count == 0)
        {
            var build = NewBuildFromCurrentState();
            SavedBuilds.Add(build);
            SelectedBuildId = build.Id;
            return;
        }

        if (SelectedBuildId is not null && SavedBuilds.Any(build => build.Id == SelectedBuildId))
        {
            return;
        }

        var selected = SavedBuilds[0];
        SelectedBuildId = selected.Id;
        LoadState(selected.State);
    }

    public void CreateNewBuild()
    {
        UpdateSelectedBuildState();
        ResetSelections();
        var build = NewBuildFromCurrentState();
        SavedBuilds.Add(build);
        SelectedBuildId = build.Id;
    }

    public void SelectBuild(string buildId)
    {
        var selected = SavedBuilds.FirstOrDefault(build => build.Id == buildId);
        if (selected is null)
        {
            EnsureSelectedBuild();
            return;
        }

        SelectedBuildId = selected.Id;
        LoadState(selected.State);
    }

    public void RenameSelectedBuild(string? name)
    {
        EnsureSelectedBuild();
        var normalizedName = NormalizeBuildName(name);
        var selectedIndex = SavedBuilds.FindIndex(build => build.Id == SelectedBuildId);
        if (selectedIndex >= 0)
        {
            var selected = SavedBuilds[selectedIndex];
            SavedBuilds[selectedIndex] = selected with { Name = normalizedName };
        }
    }

    public void DeleteSelectedBuild()
    {
        EnsureSelectedBuild();
        var selectedIndex = SavedBuilds.FindIndex(build => build.Id == SelectedBuildId);
        if (selectedIndex < 0)
        {
            return;
        }

        SavedBuilds.RemoveAt(selectedIndex);
        if (SavedBuilds.Count == 0)
        {
            SelectedBuildId = null;
            ResetSelections();
            EnsureSelectedBuild();
            return;
        }

        var nextIndex = Math.Min(selectedIndex, SavedBuilds.Count - 1);
        var next = SavedBuilds[nextIndex];
        SelectedBuildId = next.Id;
        LoadState(next.State);
    }

    public void UpdateSelectedBuildState()
    {
        EnsureSelectedBuild();
        var selectedIndex = SavedBuilds.FindIndex(build => build.Id == SelectedBuildId);
        if (selectedIndex >= 0)
        {
            var selected = SavedBuilds[selectedIndex];
            SavedBuilds[selectedIndex] = selected with { State = BuildState() };
        }
    }

    public void ResetSelections()
    {
        AircraftLevelPlan.LoadLevels(0, 0);
        MasteryPlan.LoadLevels(0, 0);
        GoldMasteryStatus = GoldMasteryStatus.Off;

        foreach (var row in SystemPlans)
        {
            row.NodeStates.Clear();
        }
    }

    public void NormalizeInputs()
    {
        AircraftLevelPlan.NormalizeSelection();
        MasteryPlan.NormalizeSelection();

        foreach (var row in SystemPlans)
        {
            row.NormalizeSelection();
        }
    }

    public void CycleAircraftLevel(int level)
    {
        AircraftLevelPlan.Cycle(level);
    }

    public void CycleMasteryLevel(int level)
    {
        MasteryPlan.Cycle(level);
    }

    public void CycleGoldMasteryStatus()
    {
        GoldMasteryStatus = GoldMasteryStatus switch
        {
            GoldMasteryStatus.Off => GoldMasteryStatus.Owned,
            GoldMasteryStatus.Owned => GoldMasteryStatus.Planned,
            _ => GoldMasteryStatus.Off
        };
    }

    public Upgrades2PlannerInput BuildPlannerInput()
    {
        NormalizeInputs();
        return new Upgrades2PlannerInput(
            CurrentAircraftLevel,
            TargetAircraftLevel,
            SystemPlans.Select(row => row.ToPlannerInput()).ToArray(),
            new MasteryPlanV2(
                MasteryPlan.CurrentMasteryLevel,
                MasteryPlan.PlannedMasteryLevel,
                GoldMasteryStatus));
    }

    public Upgrades2PlannerState BuildState()
    {
        NormalizeInputs();
        return new Upgrades2PlannerState(
            2,
            CurrentAircraftLevel,
            TargetAircraftLevel,
            SystemPlans.Select(row => row.ToSavedSystemPlan()).ToArray(),
            MasteryPlan.CurrentMasteryLevel,
            MasteryPlan.PlannedMasteryLevel,
            GoldMasteryStatus.ToString());
    }

    public Upgrades2SavedBuildCollection BuildBuildCollection()
    {
        UpdateSelectedBuildState();
        return new Upgrades2SavedBuildCollection(SelectedBuildId, SavedBuilds.ToArray());
    }

    private Upgrades2SavedBuild NewBuildFromCurrentState() =>
        new(Guid.NewGuid().ToString("N"), "unnamed", BuildState());

    private static string NormalizeBuildName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? "unnamed" : name.Trim();
}
