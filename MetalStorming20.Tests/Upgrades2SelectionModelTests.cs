using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2AircraftLevelSelectionTests
{
    [Fact]
    public void Cycle_WhenDesiredPrerequisiteExists_MarksNextLevelDesiredNotOwned()
    {
        var plan = Upgrades2AircraftLevelPlan.FromLevels(5, 5);

        plan.Cycle(6);
        plan.Cycle(6);
        plan.Cycle(7);

        Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(6));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(7));
        Assert.Equal(5, plan.CurrentAircraftLevel);
        Assert.Equal(7, plan.TargetAircraftLevel);
    }
}

public class Upgrades2MasteryLevelSelectionTests
{
    [Fact]
    public void Cycle_WhenDesiredPrerequisiteExists_MarksNextLevelDesiredNotOwned()
    {
        var plan = Upgrades2MasteryLevelPlan.FromLevels(1, 1);

        plan.Cycle(6);
        plan.Cycle(6);
        plan.Cycle(7);

        Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(6));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(7));
        Assert.Equal(5, plan.CurrentMasteryLevel);
        Assert.Equal(7, plan.PlannedMasteryLevel);
    }
}

public class Upgrades2SystemPlanRowSelectionTests
{
    [Fact]
    public void CycleBranchNodeWithoutPrerequisites_FillsPriorPathAsOwned()
    {
        var row = Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_engines",
            PlannerV2.GenericAircraftId,
            "ENGINES",
            PlannerV2.Currencies.EngineParts,
            "Engines"));

        row.Cycle(7, "A");
        row.Cycle(7, "A");

        foreach (var node in new[] { "1", "2", "3", "4", "5A", "6A" })
        {
            Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(node));
        }

        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(7, "A"));
        Assert.Equal(Upgrades2NodeSelectionState.Off, row.StateFor(5, "B"));
        Assert.Equal(Upgrades2NodeSelectionState.Off, row.StateFor(6, "B"));
    }

    [Fact]
    public void ToPlannerInput_MapsOwnedAndDesiredNodeStates()
    {
        var row = Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_engines",
            PlannerV2.GenericAircraftId,
            "ENGINES",
            PlannerV2.Currencies.EngineParts,
            "Engines"));

        row.Cycle(1, null);
        row.Cycle(2, null);
        row.Cycle(2, null);

        var input = row.ToPlannerInput();

        Assert.Equal(Upgrades2NodeSelectionState.Owned, input.NodeStates["1"]);
        Assert.Equal(Upgrades2NodeSelectionState.Desired, input.NodeStates["2"]);
    }

    [Fact]
    public void LoadState_RestoresSavedNodeStates()
    {
        var row = Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_engines",
            PlannerV2.GenericAircraftId,
            "ENGINES",
            PlannerV2.Currencies.EngineParts,
            "Engines"));

        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_engines",
            ["1", "2"],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["1"] = "has",
                ["2"] = "desired"
            }));

        Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(1, null));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(2, null));
    }
}
