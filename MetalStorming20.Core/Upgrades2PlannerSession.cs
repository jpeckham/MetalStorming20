namespace MetalStorming20.Core;

public sealed class Upgrades2PlannerSession
{
    public IReadOnlyList<SystemSlotDefinitionV2> SystemSlots { get; private set; } = PlannerV2.GenericSystemSlots;
    public List<Upgrades2SystemPlanRow> SystemPlans { get; } = [];
    public Upgrades2AircraftLevelPlan AircraftLevelPlan { get; } = Upgrades2AircraftLevelPlan.FromLevels(5, 5);
    public Upgrades2MasteryLevelPlan MasteryPlan { get; } = Upgrades2MasteryLevelPlan.FromLevels(1, 1);
    public GoldMasteryStatus GoldMasteryStatus { get; private set; } = GoldMasteryStatus.Off;

    public int CurrentAircraftLevel => AircraftLevelPlan.CurrentAircraftLevel;
    public int TargetAircraftLevel => AircraftLevelPlan.TargetAircraftLevel;

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
}
