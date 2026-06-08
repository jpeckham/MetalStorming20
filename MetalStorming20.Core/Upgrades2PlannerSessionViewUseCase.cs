namespace MetalStorming20.Core;

public sealed record Upgrades2LevelSelectionResponse(int Level, Upgrades2NodeSelectionState State);

public sealed record Upgrades2SystemNodeSelectionResponse(
    int Level,
    string? BranchCode,
    Upgrades2NodeSelectionState State);

public sealed record Upgrades2SystemPlanResponse(
    string SystemSlotId,
    string DisplayName,
    IReadOnlyList<Upgrades2SystemNodeSelectionResponse> Nodes);

public sealed record Upgrades2PlannerSessionResponse(
    IReadOnlyList<Upgrades2LevelSelectionResponse> AircraftLevels,
    IReadOnlyList<Upgrades2LevelSelectionResponse> MasteryLevels,
    GoldMasteryStatus GoldMasteryStatus,
    IReadOnlyList<Upgrades2SystemPlanResponse> Systems,
    int CurrentAircraftLevel,
    int TargetAircraftLevel);

public interface IUpgrades2PlannerSessionPresenter
{
    void Present(Upgrades2PlannerSessionResponse response);
}

public sealed class PresentUpgrades2PlannerSessionUseCase
{
    private static readonly int[] BranchLevels = [5, 6, 7, 8];

    public void Handle(Upgrades2PlannerSession session, IUpgrades2PlannerSessionPresenter presenter)
    {
        presenter.Present(new Upgrades2PlannerSessionResponse(
            AircraftLevels: Enumerable.Range(1, 20)
                .Select(level => new Upgrades2LevelSelectionResponse(level, session.AircraftLevelPlan.StateFor(level)))
                .ToArray(),
            MasteryLevels: Enumerable.Range(1, 24)
                .Select(level => new Upgrades2LevelSelectionResponse(level, session.MasteryPlan.StateFor(level)))
                .ToArray(),
            GoldMasteryStatus: session.GoldMasteryStatus,
            Systems: session.SystemPlans.Select(BuildSystemResponse).ToArray(),
            CurrentAircraftLevel: session.CurrentAircraftLevel,
            TargetAircraftLevel: session.TargetAircraftLevel));
    }

    private static Upgrades2SystemPlanResponse BuildSystemResponse(Upgrades2SystemPlanRow row)
    {
        var nodes = new List<Upgrades2SystemNodeSelectionResponse>();
        for (var level = 1; level <= 4; level++)
        {
            nodes.Add(new Upgrades2SystemNodeSelectionResponse(level, null, row.StateFor(level, null)));
        }

        foreach (var level in BranchLevels)
        {
            nodes.Add(new Upgrades2SystemNodeSelectionResponse(level, "A", row.StateFor(level, "A")));
            nodes.Add(new Upgrades2SystemNodeSelectionResponse(level, "B", row.StateFor(level, "B")));
        }

        return new Upgrades2SystemPlanResponse(row.SystemSlotId, row.DisplayName, nodes);
    }
}
