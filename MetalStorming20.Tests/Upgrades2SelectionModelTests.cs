using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2AircraftLevelSelectionTests
{
    [Fact]
    public void Cycle_DesiredLevelWithLaterSelection_SkipsOffAndBecomesOwned()
    {
        var plan = Upgrades2AircraftLevelPlan.FromLevels(1, 3);

        plan.Cycle(2);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(2));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(3));
    }

    [Fact]
    public void Cycle_DesiredLevelWithBluePrerequisites_PromotesPrerequisitesInsteadOfClearingDependents()
    {
        var plan = Upgrades2AircraftLevelPlan.FromLevels(13, 20);

        plan.Cycle(17);

        for (var level = 1; level <= 17; level++)
        {
            Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(level));
        }

        for (var level = 18; level <= 20; level++)
        {
            Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(level));
        }
    }

    [Fact]
    public void Cycle_OffLaterLevelToOwned_PromotesDesiredPrerequisitesToOwned()
    {
        var plan = Upgrades2AircraftLevelPlan.FromLevels(5, 5);

        plan.Cycle(6);
        plan.Cycle(6);
        plan.Cycle(7);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(6));
        Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(7));
        Assert.Equal(7, plan.CurrentAircraftLevel);
        Assert.Equal(7, plan.TargetAircraftLevel);
    }
}

public class Upgrades2MasteryLevelSelectionTests
{
    [Fact]
    public void Cycle_DesiredLevelWithLaterSelection_SkipsOffAndBecomesOwned()
    {
        var plan = Upgrades2MasteryLevelPlan.FromLevels(1, 3);

        plan.Cycle(2);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(2));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, plan.StateFor(3));
    }

    [Fact]
    public void Cycle_OffLaterLevelToOwned_PromotesDesiredPrerequisitesToOwned()
    {
        var plan = Upgrades2MasteryLevelPlan.FromLevels(1, 1);

        plan.Cycle(6);
        plan.Cycle(6);
        plan.Cycle(7);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(6));
        Assert.Equal(Upgrades2NodeSelectionState.Owned, plan.StateFor(7));
        Assert.Equal(7, plan.CurrentMasteryLevel);
        Assert.Equal(7, plan.PlannedMasteryLevel);
    }
}

public class Upgrades2SystemPlanRowSelectionTests
{
    [Fact]
    public void Cycle_DesiredSpecialLevelWithLaterSelection_SkipsOffAndBecomesOwned()
    {
        var row = Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_special",
            PlannerV2.GenericAircraftId,
            "SPECIAL",
            PlannerV2.Currencies.SpecialAbilityBlueprints,
            "Special",
            8,
            3,
            false,
            "SPECIAL"));
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_special",
            ["1", "2", "3"],
            new Dictionary<string, string>
            {
                ["1"] = "owned",
                ["2"] = "desired",
                ["3"] = "desired"
            }));

        row.Cycle(2, null);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(2, null));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(3, null));
    }

    [Fact]
    public void Cycle_DesiredLevelWithBluePrerequisite_PromotesPrerequisiteAndBecomesOwned()
    {
        var row = Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_special",
            PlannerV2.GenericAircraftId,
            "SPECIAL",
            PlannerV2.Currencies.SpecialAbilityBlueprints,
            "Special",
            8,
            3,
            false,
            "SPECIAL"));
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_special",
            ["1", "2", "3"],
            new Dictionary<string, string>
            {
                ["1"] = "desired",
                ["2"] = "desired",
                ["3"] = "desired"
            }));

        row.Cycle(2, null);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(1, null));
        Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(2, null));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(3, null));
    }

    [Fact]
    public void Cycle_LastOwnedBranchToDesired_DemotesOwnedDescendantsWhenNoOwnedAlternateRemains()
    {
        var row = BranchRow();
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_engines",
            ["1", "2", "3", "4", "5A", "6A", "6B", "7A"],
            new Dictionary<string, string>
            {
                ["1"] = "owned",
                ["2"] = "owned",
                ["3"] = "owned",
                ["4"] = "owned",
                ["5A"] = "owned",
                ["6A"] = "desired",
                ["6B"] = "owned",
                ["7A"] = "owned"
            }));

        row.Cycle(6, "B");

        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(6, "A"));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(6, "B"));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(7, "A"));
    }

    [Fact]
    public void Cycle_DesiredLastBranchWithBlueDescendant_SkipsOffAndBecomesOwned()
    {
        var row = BranchRow();
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_engines",
            ["1", "2", "3", "4", "5A", "6B", "7A"],
            new Dictionary<string, string>
            {
                ["1"] = "owned",
                ["2"] = "owned",
                ["3"] = "owned",
                ["4"] = "owned",
                ["5A"] = "owned",
                ["6B"] = "desired",
                ["7A"] = "desired"
            }));

        row.Cycle(6, "B");

        Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(6, "B"));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(7, "A"));
    }

    [Fact]
    public void LoadState_DemotesOwnedNodeAfterBluePrerequisiteWithoutOwnedAlternate()
    {
        var row = BranchRow();
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_engines",
            ["1", "2", "3", "4", "5A", "6A", "7A", "8A"],
            new Dictionary<string, string>
            {
                ["1"] = "owned",
                ["2"] = "owned",
                ["3"] = "owned",
                ["4"] = "owned",
                ["5A"] = "owned",
                ["6A"] = "owned",
                ["7A"] = "desired",
                ["8A"] = "owned"
            }));

        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(7, "A"));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, row.StateFor(8, "A"));
    }

    [Fact]
    public void Cycle_OffFarBranchToOwned_PromotesExistingBluePrerequisitePathToOwned()
    {
        var row = BranchRow();
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_engines",
            ["1", "2", "3", "4", "5A", "6A", "7A"],
            new Dictionary<string, string>
            {
                ["1"] = "owned",
                ["2"] = "owned",
                ["3"] = "owned",
                ["4"] = "owned",
                ["5A"] = "desired",
                ["6A"] = "desired",
                ["7A"] = "desired"
            }));

        row.Cycle(8, "A");

        foreach (var node in new[] { "1", "2", "3", "4", "5A", "6A", "7A", "8A" })
        {
            Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(node));
        }
    }

    [Fact]
    public void Cycle_BranchWithAlternateAtSameLevel_CanBecomeOffDespiteLaterSelection()
    {
        var row = Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_engines",
            PlannerV2.GenericAircraftId,
            "ENGINES",
            PlannerV2.Currencies.EngineParts,
            "Engines"));
        row.LoadState(new Upgrades2SavedSystemPlan(
            "generic_engines",
            ["1", "2", "3", "4", "5A", "5B", "6B"],
            new Dictionary<string, string>
            {
                ["1"] = "owned",
                ["2"] = "owned",
                ["3"] = "owned",
                ["4"] = "owned",
                ["5A"] = "desired",
                ["5B"] = "owned",
                ["6B"] = "desired"
            }));

        row.Cycle(5, "A");

        Assert.Equal(Upgrades2NodeSelectionState.Off, row.StateFor(5, "A"));
        Assert.Equal(Upgrades2NodeSelectionState.Owned, row.StateFor(5, "B"));
    }

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

    private static Upgrades2SystemPlanRow BranchRow() =>
        Upgrades2SystemPlanRow.FromSlot(new SystemSlotDefinitionV2(
            "generic_engines",
            PlannerV2.GenericAircraftId,
            "ENGINES",
            PlannerV2.Currencies.EngineParts,
            "Engines"));
}
