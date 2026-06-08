using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2PlannerSessionTests
{
    [Fact]
    public void ResetSystemRows_BindsRowsToCatalogSlots()
    {
        var session = new Upgrades2PlannerSession();

        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);

        var row = Assert.Single(session.SystemPlans);
        Assert.Equal("generic_engines", row.SystemSlotId);
        Assert.Equal("Engines", row.DisplayName);
    }

    [Fact]
    public void LoadState_AppliesAircraftMasteryGoldAndSystemSelections()
    {
        var session = new Upgrades2PlannerSession();
        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);

        session.LoadState(new Upgrades2PlannerState(
            SchemaVersion: 2,
            CurrentAircraftLevel: 7,
            TargetAircraftLevel: 8,
            SystemPlans:
            [
                new Upgrades2SavedSystemPlan(
                    "generic_engines",
                    ["1", "2"],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["1"] = "has",
                        ["2"] = "desired"
                    })
            ],
            CurrentMasteryLevel: 3,
            PlannedMasteryLevel: 6,
            GoldMasteryStatus: GoldMasteryStatus.Planned.ToString()));

        Assert.Equal(7, session.CurrentAircraftLevel);
        Assert.Equal(8, session.TargetAircraftLevel);
        Assert.Equal(3, session.MasteryPlan.CurrentMasteryLevel);
        Assert.Equal(6, session.MasteryPlan.PlannedMasteryLevel);
        Assert.Equal(GoldMasteryStatus.Planned, session.GoldMasteryStatus);
        Assert.Equal(Upgrades2NodeSelectionState.Owned, session.SystemPlans.Single().StateFor(1, null));
        Assert.Equal(Upgrades2NodeSelectionState.Desired, session.SystemPlans.Single().StateFor(2, null));
    }

    [Fact]
    public void BuildPlannerInput_MapsCurrentSessionSelections()
    {
        var session = new Upgrades2PlannerSession();
        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);

        session.CycleAircraftLevel(6);
        session.CycleAircraftLevel(6);
        session.SystemPlans.Single().Cycle(1, null);

        var input = session.BuildPlannerInput();

        Assert.Equal(5, input.CurrentAircraftLevel);
        Assert.Equal(6, input.TargetAircraftLevel);
        Assert.Equal(Upgrades2NodeSelectionState.Owned, input.Systems.Single().NodeStates["1"]);
    }

    [Fact]
    public void BuildState_MapsCurrentSessionSelectionsForPersistence()
    {
        var session = new Upgrades2PlannerSession();
        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);

        session.CycleMasteryLevel(6);
        session.CycleMasteryLevel(6);
        session.CycleGoldMasteryStatus();
        session.CycleGoldMasteryStatus();
        session.SystemPlans.Single().Cycle(1, null);

        var state = session.BuildState();

        Assert.Equal(2, state.SchemaVersion);
        Assert.Equal(6, state.PlannedMasteryLevel);
        Assert.Equal(GoldMasteryStatus.Planned.ToString(), state.GoldMasteryStatus);
        Assert.Equal("has", state.SystemPlans!.Single().NodeStates!["1"]);
    }

    [Fact]
    public void ResetSelections_ClearsCurrentBuildBackToNewBuildDefaults()
    {
        var session = new Upgrades2PlannerSession();
        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);
        session.CycleAircraftLevel(8);
        session.CycleAircraftLevel(8);
        session.CycleMasteryLevel(6);
        session.CycleMasteryLevel(6);
        session.CycleGoldMasteryStatus();
        session.CycleGoldMasteryStatus();
        session.SystemPlans.Single().Cycle(1, null);

        session.ResetSelections();

        Assert.Equal(0, session.CurrentAircraftLevel);
        Assert.Equal(0, session.TargetAircraftLevel);
        Assert.Equal(0, session.MasteryPlan.CurrentMasteryLevel);
        Assert.Equal(0, session.MasteryPlan.PlannedMasteryLevel);
        Assert.Equal(GoldMasteryStatus.Off, session.GoldMasteryStatus);
        Assert.Empty(session.SystemPlans.Single().NodeStates);
    }
}
